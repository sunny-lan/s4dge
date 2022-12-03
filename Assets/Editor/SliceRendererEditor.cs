using UnityEngine;
using System.Collections;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Unity.VisualScripting;
using Assets.v2;

namespace v2
{
    [CustomEditor(typeof(SliceRenderer)), CanEditMultipleObjects]
    public class SliceRendererEditor : Editor
    {

        string addSliceFile = "Assets/Models/cube.s3dge";
        float modifySliceW = 0;
        bool sameWWarning = false, removeWNotExist = false;
        Vector3 addCorner, addScale = new(1, 1, 1);
        string saveShapeFile = "";

        

        // Do nothing if the slice file fails to be read
        public void AddSliceToTarget()
        {
            var slice = S3DGELoader.LoadSlice(addSliceFile);
            if (slice != null)
            {
                // if slice already exists at specified w, warn user and do nothing
                if (!(sameWWarning = !((SliceRenderer)target).Shape.AddSlice(modifySliceW, slice, addScale, addCorner))) {
                    //((SliceRenderer)target).Update();
                }
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            // text field specifying the file to load the slice from
            addSliceFile = EditorGUILayout.TextField("Slice file:", addSliceFile);

            // slider controlling the w coordinate to add the slice at
            modifySliceW = EditorGUILayout.Slider("Add/Remove slice at w coordinate:", modifySliceW, -20.0f, 20.0f);

            // Button add the slice to the 4D shape
            if (EditorGUILayout.LinkButton("Add slice"))
            {
                AddSliceToTarget();
            }

            if (sameWWarning)
            {
                EditorGUILayout.HelpBox("Slice already exists at the requested w coordinate", MessageType.Warning);
            }

            // Button to remove slice from the specified w-coordinate
            if (EditorGUILayout.LinkButton("Remove slice"))
            {
                removeWNotExist = !((SliceRenderer)target).Shape.RemoveSlice(modifySliceW);
            }

            if (removeWNotExist)
            {
                EditorGUILayout.HelpBox("Failed to remove slice at requested w coordinate: slice doesn't exit", MessageType.Warning);
            }

            // fields to modify the 3D slice to be added
            addCorner = EditorGUILayout.Vector3Field("3D shape corner:", addCorner);
            addScale = EditorGUILayout.Vector3Field("3D shape scale:", addScale);

            // Button save the 4D shape
            saveShapeFile = EditorGUILayout.TextField("4D Shape save name:", saveShapeFile);
            if (EditorGUILayout.LinkButton("Save 4D shape"))
            {
                v2.S4DGE.SaveS4DGE(((SliceRenderer)target).Shape, saveShapeFile);
            }

        }
    }
}
