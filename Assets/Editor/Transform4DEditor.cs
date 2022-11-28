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
        if (transform4D.localRotation.Length != Transform4D.ROTATION_DOF)
        {
            Debug.LogWarning("Warning! Number of rotational axis should not be modified! ");
            transform4D.localRotation = new float[Transform4D.ROTATION_DOF];
        }
        Debug.Assert(Transform4D.ROTATION_DOF == 6, "Update transform4d editor after changes number of rotational degrees of freedom");
        // hide rotations inside foldout
        showRotations = EditorGUILayout.Foldout(showRotations, "Rotations");
        if (showRotations)
        {
            // specify names for all rotational axis
            transform4D.localRotation[0] = EditorGUILayout.FloatField("xy", transform4D.localRotation[0]);
            transform4D.localRotation[1] = EditorGUILayout.FloatField("xz", transform4D.localRotation[1]);
            transform4D.localRotation[2] = EditorGUILayout.FloatField("xw", transform4D.localRotation[2]);
            transform4D.localRotation[3] = EditorGUILayout.FloatField("yz", transform4D.localRotation[3]);
            transform4D.localRotation[4] = EditorGUILayout.FloatField("yw", transform4D.localRotation[4]);
            transform4D.localRotation[5] = EditorGUILayout.FloatField("zw", transform4D.localRotation[5]);
        }
    }

    void OnSceneGUI()
    {
        Transform4D transform4D = target as Transform4D;

        // draw our own handles
        if (Tools.current == Tool.Move)
        {
            //TODO add w movement
            var oldPos = transform4D.position;
            transform4D.position = Handles.PositionHandle(
                oldPos,
                transform4D.localRotation3D
            ).withW(oldPos.w);
        }
        else if (Tools.current == Tool.Rotate)
        {
            transform4D.localRotation3D = Handles.RotationHandle(
                transform4D.localRotation3D,
                transform4D.position
            );
        }
        else if (Tools.current == Tool.Scale)
        {
            transform4D.localScale = Handles.ScaleHandle(
                transform4D.localScale,
                transform4D.position.XYZ(),
                transform4D.localRotation3D
            );
        }
    }

    //
    // Disable default transform handles
    //

    private void OnEnable()
    {
        Tools.hidden = true;
    }

    private void OnDisable()
    {
        Tools.hidden = false;
    }
}

[CustomEditor(typeof(BoxCollider4D))]
public class BoxCollider4DEditor : Editor
{
    void OnSceneGUI()
    {
        var sceneView = SceneView4DController.currentlyDrawingSceneView;
        if (sceneView == null) return;

        if(target is BoxCollider4D b4d)
        {
            float minW = b4d.corner.w, maxW = b4d.corner.w + b4d.size.w;

            float curW = sceneView.t4d.localPosition.w;
            if (curW >= minW && curW <= maxW)
                Handles.DrawWireCube(b4d.corner + b4d.size/2, b4d.size);
        }
    }
}