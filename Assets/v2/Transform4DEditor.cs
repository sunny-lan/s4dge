using UnityEditor;
using UnityEngine;
using UnityEditor.UIElements;
using v2;

[CustomEditor(typeof(Transform4D))]
public class Transform4DEditor : Editor
{
    bool showRotations = false;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Transform4D transform4D = target as Transform4D;
        if ( transform4D.rotation.Length != Transform4D.ROTATION_DOF ) {
            Debug.LogWarning("Warning! Number of rotational axis should not be modified! ");
            transform4D.rotation = new float[Transform4D.ROTATION_DOF];
        }
        Debug.Assert(Transform4D.ROTATION_DOF == 6, "Update transform4d editor after changes number of rotational degrees of freedom");
        // hide rotations inside foldout
        showRotations = EditorGUILayout.Foldout(showRotations, "Rotations");
        if ( showRotations ) {
            // specify names for all rotational axis
            transform4D.rotation[0] = EditorGUILayout.FloatField("xy", transform4D.rotation[0] );
            transform4D.rotation[1] = EditorGUILayout.FloatField("xz", transform4D.rotation[1] );
            transform4D.rotation[2] = EditorGUILayout.FloatField("xw", transform4D.rotation[2] );
            transform4D.rotation[3] = EditorGUILayout.FloatField("yz", transform4D.rotation[3] );
            transform4D.rotation[4] = EditorGUILayout.FloatField("yw", transform4D.rotation[4] );
            transform4D.rotation[5] = EditorGUILayout.FloatField("zw", transform4D.rotation[5] );
        }
    }
}