using RasterizationRenderer;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


/// <summary>
/// This class describes the raw data for a single 4D tetrahedral mesh.
/// The tetrahedral mesh is stored using a vertex and index buffer.
/// The vertex buffer contains position and normal data for each vertex,
/// while the index buffer contains a series of indices pointing into the vertex array,
/// with every 4 indices reprenting a single tetrahedron.
/// </summary>
namespace S4DGE
{
    [Serializable]
    public class TetMesh_raw
    {
        /// <summary>
        /// The vertex array
        /// </summary>
        [SerializeField]
        public List<TetMesh4D.VertexData> vertices;

        /// <summary>
        /// The index array
        /// </summary>
        [SerializeField]
        public List<TetMesh4D.Tet4D> tets;

        /// <summary>
        /// Add a single tetrahedron into this mesh
        /// </summary>
        /// <param name="tet">The list of points in the tetrahedron (should be length 4)</param>
        public void Append(IEnumerable<Vector4> tet)
        {
            tets.Add(new()
            {
                tetPoints = Enumerable.Range(vertices.Count, vertices.Count + 4).ToArray()
            });

            vertices.AddRange(tet.Select(p => new TetMesh4D.VertexData()
            {
                position = p
            }));
        }

        /// <summary>
        /// Converts the raw tetrahedron data into a form that can be used with the rasterization renderer
        /// </summary>
        /// <returns>The rasterizable tetrahedron mesh</returns>
        public TetMesh4D ToRasterizableTetMesh()
        {
            return new TetMesh4D(vertices.ToArray(), tets.ToArray());
        }

        public TetMesh_raw()
        {
            vertices = new();
            tets = new();
        }

        public TetMesh_raw(TetMesh4D m)
        {
            vertices = m.vertices.ToList();
            tets = m.tets.ToList();
        }
    }
}