using UnityEngine;
using System.Collections;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace v2
{
    [CustomEditor(typeof(SliceRenderer)), CanEditMultipleObjects]
    public class SliceRendererEditor : Editor
    {
        string addSliceFile = "Assets/Models/cube.s3dge";
        float modifySliceW = 0;
        bool sameWWarning = false, removeWNotExist = false;

        // reads a 3D slice from the file
        public Dictionary<string, Vector3> ReadSlice()
        {
            if (!File.Exists(addSliceFile))
            {
                // log a message and do nothing if the file doesn't exist
                Debug.Log(string.Format("Could not read slice from file {}: not found", addSliceFile));
                return null;
            }

            Dictionary<string, Vector3> points = new();

            string[] fileLines = File.ReadAllLines(addSliceFile);

            foreach (string fileLine in fileLines)
            {
                string[] terms = fileLine.Split(':', '(', ')');
                string pName = terms[0];
                string[] pTerms = terms[2].Split(',');

                points.Add(pName, new Vector3(
                    int.Parse(pTerms[0]),
                    int.Parse(pTerms[1]),
                    int.Parse(pTerms[2])
                    ));
            }

            return points;
        }

        // Do nothing if the slice file fails to be read
        void AddSliceToTarget()
        {
            var slice = ReadSlice();
            if (slice != null)
            {
                // if slice already exists at specified w, warn user and do nothing
                if (!(sameWWarning = !((SliceRenderer)target).Shape.AddSlice(modifySliceW, slice))) {
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

        }
    }
}
