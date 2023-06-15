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
			
			#include "RayTracingStructs.cginc"

			struct Sphere
			{
				float4 position;
				float radius;
				RayTracingMaterial material;
			};

			struct Triangle
			{
				float4 posA, posB, posC;
				float4 normalA, normalB, normalC;
			};

			// --- Buffers ---	
			StructuredBuffer<Sphere> Spheres;
			int NumSpheres;

			StructuredBuffer<Triangle> Triangles;
			StructuredBuffer<MeshInfo> AllMeshInfo;
			int NumMeshes;

			// --- Ray Intersection Functions ---
		
			// Calculate the intersection of a ray with a sphere
			HitInfo RaySphere(Ray ray, float4 sphereCentre, float sphereRadius)
			{
				HitInfo hitInfo = (HitInfo)0;
				float3 offsetRayOrigin = ray.origin3D() - sphereCentre;
				// From the equation: sqrLength(rayOrigin + rayDir * dst) = radius^2
				// Solving for dst results in a quadratic equation with coefficients:
				float a = dot(ray.dir3D(), ray.dir3D()); // a = 1 (assuming unit vector)
				float b = 2 * dot(offsetRayOrigin, ray.dir3D());
				float c = dot(offsetRayOrigin, offsetRayOrigin) - sphereRadius * sphereRadius;
				// Quadratic discriminant
				float discriminant = b * b - 4 * a * c; 

				// No solution when d < 0 (ray misses sphere)
				if (discriminant >= 0) {
					// Distance to nearest intersection point (from quadratic formula)
					float dst = (-b - sqrt(discriminant)) / (2 * a);

					// Ignore intersections that occur behind the ray
					if (dst >= 0) {
						hitInfo.didHit = true;
						hitInfo.dst = dst;
						hitInfo.hitPoint = ray.origin + ray.dir * dst;
						hitInfo.numHits = discriminant > 10 ? 2 : 1;
						hitInfo.normal = normalize(hitInfo.hitPoint - sphereCentre);
					}
				}
				return hitInfo;
			}

			// Calculate the intersection of a ray with a triangle using Möller–Trumbore algorithm
			// Thanks to https://stackoverflow.com/a/42752998
			HitInfo RayTriangle(Ray ray, Triangle tri)
			{
				float3 edgeAB = tri.posB - tri.posA;
				float3 edgeAC = tri.posC - tri.posA;
				float3 normalVector = cross(edgeAB, edgeAC);
				float3 ao = ray.origin3D() - tri.posA;
				float3 dao = cross(ao, ray.dir3D());

				float determinant = -dot(ray.dir3D(), normalVector);
				float invDet = 1 / determinant;
				
				// Calculate dst to triangle & barycentric coordinates of intersection point
				float dst = dot(ao, normalVector) * invDet;
				float u = dot(edgeAC, dao) * invDet;
				float v = -dot(edgeAB, dao) * invDet;
				float w = 1 - u - v;
				
				// Initialize hit info
				HitInfo hitInfo;
				hitInfo.didHit = determinant >= 1E-6 && dst >= 0 && u >= 0 && v >= 0 && w >= 0;
				hitInfo.hitPoint = ray.origin + ray.dir * dst;
				hitInfo.normal = normalize(tri.normalA * w + tri.normalB * u + tri.normalC * v);
				hitInfo.dst = dst;
				return hitInfo;
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
			// www.pcg-random.org and www.shadertoy.com/view/XlGcRh
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

			// --- Ray Tracing Stuff ---
			// Find the first point that the given ray collides with, and return hit info
			//! More basic version
			HitInfo CalculateRayCollision(Ray ray)
			{
				HitInfo closestHit = (HitInfo)0;
				closestHit.dst = 1.#INF;

				for (int i = 0; i < NumSpheres; i++) {
					Sphere sphere = Spheres[i];
					HitInfo hitInfo = RaySphere(ray, sphere.position, sphere.radius);

					if (hitInfo.didHit && abs(hitInfo.dst - closestHit.dst) > 0.01){
						
						if (hitInfo.dst < closestHit.dst)
						{
							hitInfo.numHits += closestHit.numHits;
							closestHit = hitInfo;
							closestHit.material = sphere.material;
						}
						else
						{
							closestHit.numHits += hitInfo.numHits;
						}
					}
				}

				return closestHit;
			}

			//! His version
			// HitInfo CalculateRayCollision(Ray ray)
			// {
			// 	HitInfo closestHit = (HitInfo)0;
			// 	// We haven't hit anything yet, so 'closest' hit is infinitely far away
			// 	closestHit.dst = 1.#INF;

			// 	// Raycast against all spheres and keep info about the closest hit
			// 	for (int i = 0; i < NumSpheres; i ++)
			// 	{
			// 		Sphere sphere = Spheres[i];
			// 		HitInfo hitInfo = RaySphere(ray, sphere.position, sphere.radius);

			// 		if (hitInfo.didHit && hitInfo.dst < closestHit.dst)
			// 		{
			// 			closestHit = hitInfo;
			// 			closestHit.material = sphere.material;
			// 		}
			// 	}

			// 	// Raycast against all meshes and keep info about the closest hit
			// 	for (int meshIndex = 0; meshIndex < NumMeshes; meshIndex ++)
			// 	{
			// 		MeshInfo meshInfo = AllMeshInfo[meshIndex];
			// 		if (!RayBoundingBox(ray, meshInfo.boundsMin, meshInfo.boundsMax)) {
			// 			continue;
			// 		}

			// 		for (uint i = 0; i < meshInfo.numTriangles; i ++) {
			// 			int triIndex = meshInfo.firstTriangleIndex + i;
			// 			Triangle tri = Triangles[triIndex];
			// 			HitInfo hitInfo = RayTriangle(ray, tri);
	
			// 			if (hitInfo.didHit && hitInfo.dst < closestHit.dst)
			// 			{
			// 				closestHit = hitInfo;
			// 				closestHit.material = meshInfo.material;
			// 			}
			// 		}
			// 	}

			// 	return closestHit;
			// }


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
						// Handle special material types:
						if (material.flag == CheckerPattern) 
						{
							float2 c = mod2(floor(hitInfo.hitPoint.xz), 2.0);
							material.colour = c.x == c.y ? material.colour : material.emissionColour;
						}
						else if (material.flag == InvisibleLightSource && bounceIndex == 0)
						{
							ray.origin = hitInfo.hitPoint + ray.dir * 0.001;
							continue;
						}

						// Figure out new ray position and direction
						bool isSpecularBounce = material.specularProbability >= RandomValue(rngState);
					
						ray.origin = hitInfo.hitPoint;
						float4 diffuseDir = normalize(hitInfo.normal + RandomDirection(rngState));
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
						incomingLight += GetEnvironmentLight(ray) * rayColour;
						break;
					}
				}

				return incomingLight;
			}

		
			// Run for every pixel in the display
			float4 frag (v2f i) : SV_Target
			{
				// return float4(2, 0, 0, 0); // Red
				// return float4(ray.dir3D(), 0); // Multicolor lol
				// return RaySphere(ray, 0, 1).didHit; // Singular sphere

				float3 viewPointLocal = float3(i.uv - 0.5, 1) * ViewParams;
				float4 viewPoint = mul(CamLocalToWorldMatrix, float4(viewPointLocal, 1));
				viewPoint = viewPoint + CamTranslation;

				Ray ray;
				ray.origin = CamTranslation;
				ray.dir = normalize(viewPoint - ray.origin);

				HitInfo collision = CalculateRayCollision(ray);

				if (collision.didHit) 
				{
					if (collision.numHits % 2 == 1)
					{
						return float4(2,2,2,2); // return white as color for all edges
					}

					float opacity = collision.material.colour.w;
					opacity = 1.0f - pow(1.0f - opacity, collision.numHits);

					return lerp(float4(0,0,0,0), collision.material.colour, opacity); // Sending in opacity in w wasn't working, lerp towards black instead
				}
				
				return collision.material.colour;
			}

			ENDCG
		}
	}
}