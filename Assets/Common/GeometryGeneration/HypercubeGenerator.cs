using RasterizationRenderer;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HypercubeGenerator
{
	/// <summary>
	/// Generates a hypercube spanning [0,0,0,0] and [1,1,1,1]
	/// </summary>
	/// <param name="output">Mesh to append the hypercube to</param>
	/// 
	public static TetMesh4D GenerateHypercube()
	{
		TetMesh_raw outputTmp = new();
		GenerateHypercubeTmp(outputTmp);

		return new TetMesh4D(outputTmp.vertices.ToArray(), outputTmp.tets.ToArray());
	}


	public static void GenerateHypercubeTmp(TetMesh_raw output)
	{
		// hypercube is formed by 8 bounding 3-cubes
		Vector4[][] colors = new Vector4[][]
		{
			Enumerable.Repeat<Vector4>( new(0,1,1,1),6).ToArray(),
			Enumerable.Repeat<Vector4>( new(0,1,0,1),6).ToArray(),
			Enumerable.Repeat<Vector4>( new(1,1,0,1),6).ToArray(),
			Enumerable.Repeat<Vector4>( new(1,0.4f,0,1),6).ToArray(),
			Enumerable.Repeat<Vector4>( new(1,0,1,1),6).ToArray(),
			Enumerable.Repeat<Vector4>( new(1,1f,1f,1),6).ToArray(),
			Enumerable.Repeat<Vector4>( new(0,0,1,1),6).ToArray(),
			Enumerable.Repeat<Vector4>( new(1,0,0,1),6).ToArray(),
		};

		//left and right
		Generate3Cube(new(0, 0, 0, 0), Vector3.one, colors[0], new(0, 1, 0, 0), new(0, 0, 1, 0), new(0, 0, 0, 1), output, -1);
		Generate3Cube(new(1, 0, 0, 0), Vector3.one, colors[1], new(0, 1, 0, 0), new(0, 0, 1, 0), new(0, 0, 0, 1), output);

		//top and bottom
		Generate3Cube(new(0, 0, 0, 0), Vector3.one, colors[2], new(1, 0, 0, 0), new(0, 0, 1, 0), new(0, 0, 0, 1), output, -1);
		Generate3Cube(new(0, 1, 0, 0), Vector3.one, colors[3], new(1, 0, 0, 0), new(0, 0, 1, 0), new(0, 0, 0, 1), output);

		//front and back
		Generate3Cube(new(0, 0, 0, 0), Vector3.one, colors[4], new(1, 0, 0, 0), new(0, 1, 0, 0), new(0, 0, 0, 1), output, -1);
		Generate3Cube(new(0, 0, 1, 0), Vector3.one, colors[5], new(1, 0, 0, 0), new(0, 1, 0, 0), new(0, 0, 0, 1), output);

		//past and future
		Generate3Cube(new(0, 0, 0, 0), Vector3.one, colors[6], new(1, 0, 0, 0), new(0, 1, 0, 0), new(0, 0, 1, 0), output, -1);
		Generate3Cube(new(0, 0, 0, 1), Vector3.one, colors[7], new(1, 0, 0, 0), new(0, 1, 0, 0), new(0, 0, 1, 0), output);
	}

	/// <summary>
	/// Adds a 3-cube to the given mesh
	/// </summary>
	/// <param name="start">Position of lower corner of cube</param>
	/// <param name="dims">Size of cube</param>
	/// <param name="x_unit">1st axis of cube in 4D</param>
	/// <param name="y_unit">2nd axis of cube in 4D</param>
	/// <param name="z_unit">3rd axis of cube in 4D</param>
	/// <param name="output">The mesh to add the 3-cube to</param>
	public static void Generate3Cube(
		Vector4 start, Vector3 dims, Vector4[] colour,
		Vector4 x_unit, Vector4 y_unit, Vector4 z_unit,
		TetMesh_raw output, int normalMultiplier = 1
	)
	{
		var normal = Util.CrossProduct4D(x_unit, y_unit, z_unit) * normalMultiplier;

		var vertices = new List<TetMesh4D.VertexData>();

		//generate edges of cube in binary order
		for (int i = 0; i < (1 << 3); i++)
		{
			int z = (i >> 0) & 1;
			int y = (i >> 1) & 1;
			int x = (i >> 2) & 1;

			Vector3 pt = new(x, y, z);
			pt = Vector3.Scale(pt, dims);

			vertices.Add(new()
			{
				position = pt.x * x_unit + pt.y * y_unit + pt.z * z_unit + start,
				normal = normal,
			});
		}

		Decompose3Cube(vertices, colour, output);
	}

	/// <summary>
	/// Decomposes a 3-cube into tetrahedron
	/// </summary>
	/// <param name="cube">The vertex indices of the cube. Must be in binary order</param>
	/// <param name="tets_out">The list to add the output tetrahedra to</param>
	public static void Decompose3Cube(List<TetMesh4D.VertexData> cube, Vector4[] colors, TetMesh_raw mesh)
	{
		var faces = new TetMesh4D.VertexData[6, 4]
		{
			
			// (0, 0, 0) -> (1, 0, 0) -> (1, 1, 0) -> (0, 0, 1)
			{
								cube[0], cube[4], cube[6], cube[1]
			},
			
		// (0, 0, 0) -> (0, 1, 0) -> (0, 0, 1) -> (1, 1, 0)
			{
								cube[0], cube[2], cube[1], cube[6]
			},

			
		// (0, 1, 0) -> (0, 1, 1) -> (0, 0, 1) -> (1, 1, 0)
			{
								cube[2], cube[3], cube[1], cube[6]
			},

			
		// (1, 0, 0) -> (1, 0, 1) -> (0, 0, 1) -> (1, 1, 0)
			{
								cube[4], cube[5], cube[6], cube[1]
			},

			
		// (0, 0, 1) -> (1, 1, 1) -> (1, 1, 0) -> (1, 0, 1)
			{
								cube[1], cube[7], cube[5], cube[6]
			},

			
		// (0, 0, 1) -> (0, 1, 1) -> (1, 1, 0) -> (1, 1, 1)
			{
								cube[1], cube[3], cube[7], cube[6]
			}
		};

		for (int i = 0; i < 6; i++)
		{
			int baseIdx = mesh.vertices.Count;
			for (int j = 0; j < 4; j++)
			{
				faces[i, j].colour = colors[i];
				mesh.vertices.Add(faces[i, j]);
			}


			mesh.tets.Add(new TetMesh4D.Tet4D(Enumerable.Range(baseIdx, 4).ToArray()));
		}

	}
}
