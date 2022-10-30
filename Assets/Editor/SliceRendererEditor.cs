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
        float addSliceW = 0;

        // reads a 3D slice from the file
        public Dictionary<string, Vector3> readSlice()
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
        void addSliceToTarget()
        {
            var slice = readSlice();
            if (slice != null)
            {
                ((SliceRenderer)target).shape.addSlice(addSliceW, slice);
            }
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            // text field specifying the file to load the slice from
            addSliceFile = EditorGUILayout.TextField("Slice file:", addSliceFile);

            // slider controlling the w coordinate to add the slice at
            addSliceW = EditorGUILayout.Slider("Add slice at w coordinate:", addSliceW, -10.0f, 10.0f);

            // Add the slice to the 4D shape
            if (EditorGUILayout.LinkButton("Add slice"))
            {
                addSliceToTarget();
            }
        }
    }
}
