using UnityEngine;

[CreateAssetMenu(fileName = "New Tet", menuName = "General Objects/Tet")]
public class TetMesh_UnityObj : ScriptableObject
{
    [SerializeField]
    public Transform4DScriptableObject transform4D;
    public TetMesh_raw mesh_Raw;
    public bool useTransform;
}
