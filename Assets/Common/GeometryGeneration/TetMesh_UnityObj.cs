using RenderingCommon;
using UnityEngine;

/// <summary>
/// An Unity serializable asset containing all the information describing
/// a single 4D tetrahedral mesh.
/// </summary>
[CreateAssetMenu]
public class TetMesh_UnityObj : ScriptableObject
{
    /// <summary>
    /// The raw internal data of the 4D tetrahedral mesh
    /// </summary>
    [SerializeField]
    public TetMesh_raw mesh_Raw;
}
