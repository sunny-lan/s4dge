Shader "Custom/RayTracing"
{
	SubShader
	{
		Cull Off ZWrite Off ZTest Always

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "RayTracingStructs.hlsl"
			#include "Tet.hlsl"
			#include "Hypercube.hlsl"
			#include "Sphere.hlsl"
			#include "HyperSphere.hlsl"
			#include "TetMesh.hlsl"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			// --- Settings and constants ---
			static const float PI = 3.1415;

			// Raytracing Settings
			int MaxBounceCount;
			int NumRaysPerPixel;
			int Frame;
			int UseRayTracedLighting;

			// Camera Settings
			float DefocusStrength;
			float DivergeStrength;
			float3 ViewParams;
			float4x4 CamLocalToWorldMatrix; // Matrix 4x4 representing scale and rotation in 4D
			float4 CamTranslation; // Vector representing 4D translation

			// Environment Settings
			int EnvironmentEnabled;
			float4 GroundColour;
			float4 SkyColourHorizon;
			float4 SkyColourZenith;
			float SunFocus;
			float SunIntensity;

			// Special material types
			static const int CheckerPattern = 1;
			static const int InvisibleLightSource = 2;


			// --- Buffers ---
			StructuredBuffer<Sphere> Spheres;
			int NumSpheres;

			StructuredBuffer<HyperSphere> HyperSpheres;
			int NumHyperSpheres;

			StructuredBuffer<float4> Vertices;
			int NumVertices;

			StructuredBuffer<int4> Tets;
			int NumTets;

			StructuredBuffer<TetMesh> TetMeshes;
			int NumTetMeshes;

			StructuredBuffer<Hypercube> HyperCubes;
			int NumHyperCubes;

			// --- Ray Intersection Functions ---
			void RayTetMesh(inout HitInfo closestHit, in TetMesh mesh, in Ray ray)
			{
				// Transform ray into local space of object
				Ray localRay = TransformRay(ray, mesh.inverseTransform);

				for (int i = mesh.stIdx; i < mesh.edIdx; i++) {
					Tet t;
					int4 indices = Tets[i];
					t.from_points(float4x4(
						Vertices[indices[0]],
						Vertices[indices[1]],
						Vertices[indices[2]],
						Vertices[indices[3]]
					)); //TODO cache

					HitInfo hitInfo = t.intersection(ray, localRay);
					hitInfo.material = mesh.material;

					_compareHitInfo(closestHit, hitInfo);

				}
			}

			// Thanks to https://gist.github.com/DomNomNom/46bb1ce47f68d255fd5d
			bool RayBoundingBox(Ray ray, float3 boxMin, float3 boxMax)
			{
				float3 invDir = 1 / ray.dir3D();
				float3 tMin = (boxMin - ray.origin3D()) * invDir;
				float3 tMax = (boxMax - ray.origin3D()) * invDir;
				float3 t1 = min(tMin, tMax);
				float3 t2 = max(tMin, tMax);
				float tNear = max(max(t1.x, t1.y), t1.z);
				float tFar = min(min(t2.x, t2.y), t2.z);
				return tNear <= tFar;
			};

			// --- RNG Stuff ---

			// PCG (permuted congruential generator). Thanks to:
			// www.pcg-random.org and www.hlsltoy.com/view/XlGcRh
			uint NextRandom(inout uint state)
			{
				state = state * 747796405 + 2891336453;
				uint result = ((state >> ((state >> 28) + 4)) ^ state) * 277803737;
				result = (result >> 22) ^ result;
				return result;
			}

			float RandomValue(inout uint state)
			{
				return NextRandom(state) / 4294967295.0; // 2^32 - 1
			}

			// Random value in normal distribution (with mean=0 and sd=1)
			float RandomValueNormalDistribution(inout uint state)
			{
				// Thanks to https://stackoverflow.com/a/6178290
				float theta = 2 * 3.1415926 * RandomValue(state);
				float rho = sqrt(-2 * log(RandomValue(state)));
				return rho * cos(theta);
			}

			// Calculate a random direction
			float4 RandomDirection(inout uint state)
			{
				// Thanks to https://math.stackexchange.com/a/1585996
				float x = RandomValueNormalDistribution(state);
				float y = RandomValueNormalDistribution(state);
				float z = RandomValueNormalDistribution(state);
				float w = RandomValueNormalDistribution(state);
				return normalize(float4(x, y, z, w));
			}

			float2 RandomPointInCircle(inout uint rngState)
			{
				float angle = RandomValue(rngState) * 2 * PI;
				float2 pointOnCircle = float2(cos(angle), sin(angle));
				return pointOnCircle * sqrt(RandomValue(rngState));
			}

			float2 mod2(float2 x, float2 y)
			{
				return x - y * floor(x/y);
			}

			// Crude sky colour function for background light
			float3 GetEnvironmentLight(Ray ray)
			{
				if (!EnvironmentEnabled) {
					return 0;
				}

				float skyGradientT = pow(smoothstep(0, 0.4, ray.dir3D().y), 0.35);
				float groundToSkyT = smoothstep(-0.01, 0, ray.dir3D().y);
				float3 skyGradient = lerp(SkyColourHorizon, SkyColourZenith, skyGradientT);
				float sun = pow(max(0, dot(ray.dir3D(), _WorldSpaceLightPos0.xyz)), SunFocus) * SunIntensity;
				// Combine ground, sky, and sun
				float3 composite = lerp(GroundColour, skyGradient, groundToSkyT) + sun * (groundToSkyT>=1);
				return composite;
			}

			float4 tmp_checkerboard(float4 p) {
				int4 rounded = round(p * 4);
				int parity = rounded.x + rounded.y + rounded.z + rounded.w;
				return (parity%2==0) ? float4(0, 0.6, 0, 1) : float4(0, 0.2, 0, 1);
			}

			float4 tmp_lighting(float4 normal, float4 p)
			{
				float4 light_src = {0,1,0,1};
				float4 light_dir = light_src - p;
				float light_dist = length(light_dir);
				float light_angle = dot(normalize(normal), normalize(light_dir));//TODO
				return float4(0, 0, 0, 1) + float4(1, 1, 1, 0)  / light_dist;
			}

			// --- Ray Tracing Stuff ---
			// Find the first point that the given ray collides with, and return hit info
			//! More basic version
			HitInfo CalculateRayCollision(Ray ray)
			{
				HitInfo closestHit = (HitInfo)0;
				closestHit.dst = 1.#INF;

				for (int i = 0; i < NumSpheres; i++) {
					Sphere sphere = Spheres[i];
					HitInfo hitInfo = RaySphere(ray, sphere);
					_compareHitInfo(closestHit, hitInfo);
				}

				for (int i = 0; i < NumHyperSpheres; i++) {
					HyperSphere hyperSphere = HyperSpheres[i];
					HitInfo hitInfo = RayHyperSphere(ray, hyperSphere);
					_compareHitInfo(closestHit, hitInfo);
				}

				for (int j = 0; j < NumTetMeshes; j++) {
					TetMesh mesh = TetMeshes[j];
					RayTetMesh(closestHit, mesh, ray);
				}

				for (int i = 0; i < NumHyperCubes; i++) {
					Hypercube hypercube = HyperCubes[i];
					HitInfo hitInfo = hypercube.intersection(ray);

					_compareHitInfo(closestHit, hitInfo);
				}

				return closestHit;
			}

			//! His version
			/*HitInfo CalculateRayCollision(Ray ray)
			{
				HitInfo closestHit = (HitInfo)0;
				// We haven't hit anything yet, so 'closest' hit is infinitely far away
				closestHit.dst = 1.#INF;

				// Raycast against all spheres and keep info about the closest hit
				for (int i = 0; i < NumSpheres; i ++)
				{
					Sphere sphere = Spheres[i];
					HitInfo hitInfo = RaySphere(ray, sphere.position, sphere.radius);

					if (hitInfo.didHit && hitInfo.dst < closestHit.dst)
					{
						closestHit = hitInfo;
						closestHit.material = sphere.material;
					}
				}

				// Raycast against all meshes and keep info about the closest hit
				for (int meshIndex = 0; meshIndex < NumMeshes; meshIndex ++)
				{
					MeshInfo meshInfo = AllMeshInfo[meshIndex];
					if (!RayBoundingBox(ray, meshInfo.boundsMin, meshInfo.boundsMax)) {
						continue;
					}

					for (uint i = 0; i < meshInfo.numTriangles; i ++) {
						int triIndex = meshInfo.firstTriangleIndex + i;
						Triangle tri = Triangles[triIndex];
						HitInfo hitInfo = RayTriangle(ray, tri);

						if (hitInfo.didHit && hitInfo.dst < closestHit.dst)
						{
							closestHit = hitInfo;
							closestHit.material = meshInfo.material;
						}
					}
				}

				return closestHit;
			}

			float3 TraceOld(Ray ray, inout uint rngState)
			{
				float3 rayColour = 1;

				for(int i = 0; i <=MaxBounceCount; i++)
				{
					HitInfo hitInfo = CalculateRayCollision(ray);
					if (hitInfo.didHit)
					{
						ray.origin = hitInfo.hitPoint;
						ray.dir = RandomHemisphereDirection(hitInfo.normal, rngState);

						RayTracingMaterial material = hitInfo.material;
						rayColour = *= material.colour;
					}else{
						rayColour *= material.colour;
					}
				}
			}*/


			float3 Trace(Ray ray, inout uint rngState)
			{
				float3 incomingLight = 0;
				float3 rayColour = 1;

				for (int bounceIndex = 0; bounceIndex <= MaxBounceCount; bounceIndex ++)
				{
					HitInfo hitInfo = CalculateRayCollision(ray);

					if (hitInfo.didHit)
					{
						RayTracingMaterial material = hitInfo.material;
						bool isSpecularBounce = material.specularProbability >= RandomValue(rngState);

						ray.origin = hitInfo.hitPoint;
						float4 diffuseDir = normalize(hitInfo.normal + RandomDirection(rngState)); // Rotations were a bit messed up //TODO: Normal probably local space, need to be world space
						float4 specularDir = reflect(ray.dir, hitInfo.normal);
						ray.dir = normalize(lerp(diffuseDir, specularDir, material.smoothness * isSpecularBounce));


						// Update light calculations
						float3 emittedLight = material.emissionColour * material.emissionStrength;
						incomingLight += emittedLight * rayColour;
						rayColour *= lerp(material.colour, material.specularColour, isSpecularBounce);

						// Random early exit if ray colour is nearly 0 (can't contribute much to final result)
						float p = max(rayColour.r, max(rayColour.g, rayColour.b));
						if (RandomValue(rngState) >= p) {
							break;
						}
						rayColour *= 1.0f / p;
					}
					else
					{
						// incomingLight += GetEnvironmentLight(ray) * rayColour;
						break;
					}
				}

				return incomingLight;
			}


			// Run for every pixel in the display
			float4 frag (v2f i) : SV_Target
			{
				// Random
				if(UseRayTracedLighting)
				{ // Boon Lighting, need to make better

					uint2 numPixels = _ScreenParams.xy;
					uint2 pixelCoord = i.uv * numPixels;
					uint pixelIndex = pixelCoord.y * numPixels.x + pixelCoord.x;
					// uint rngState = pixelIndex;
					uint rngState = pixelIndex + Frame * 719393;



					float3 viewPointLocal = float3(i.uv - 0.5, 1) * ViewParams;
					float4 viewPoint = mul(CamLocalToWorldMatrix, float4(viewPointLocal, 0));
					viewPoint = viewPoint + CamTranslation;

					Ray ray;
					ray.origin = CamTranslation;
					ray.dir = normalize(viewPoint - ray.origin);

					float3 totalIncomingLight = 0;

					for (int rayIndex = 0; rayIndex < NumRaysPerPixel; rayIndex++){
						totalIncomingLight += Trace(ray, rngState);
					}
					float3 pixelCol = totalIncomingLight / NumRaysPerPixel;

					return float4(pixelCol, 1);
				}
				else
				{
					float3 viewPointLocal = float3(i.uv - 0.5, 1) * ViewParams;
					float4 viewPoint = mul(CamLocalToWorldMatrix, float4(viewPointLocal, 0));
					viewPoint = viewPoint + CamTranslation;

					Ray ray;
					ray.origin = CamTranslation;
					ray.dir = normalize(viewPoint - ray.origin);

					HitInfo collision = CalculateRayCollision(ray);

					if (collision.didHit)
					{
						if (collision.numHits % 2 == 1)
						{
							//return float4(2,2,2,2); // return white as color for all edges
						}

						float opacity = collision.material.colour.w;
						opacity = 1.0f - pow(1.0f - opacity, collision.numHits);

						return lerp(float4(0,0,0,0), collision.material.colour * tmp_lighting(collision.normal, collision.hitPoint), opacity); // Sending in opacity in w wasn't working, lerp towards black instead
					}

					return collision.material.colour;

				}




				// Old Lighting
				// HitInfo collision = CalculateRayCollision(ray);

				// if (collision.didHit)
				// {
				// 	if (collision.numHits % 2 == 1)
				// 	{
				// 		//return float4(2,2,2,2); // return white as color for all edges
				// 	}

				// 	float opacity = collision.material.colour.w;
				// 	opacity = 1.0f - pow(1.0f - opacity, collision.numHits);

				// 	return lerp(float4(0,0,0,0), collision.material.colour * tmp_lighting(collision.normal, collision.hitPoint), opacity); // Sending in opacity in w wasn't working, lerp towards black instead
				// }

				// return collision.material.colour;

			}


			ENDCG
		}
	}
}
