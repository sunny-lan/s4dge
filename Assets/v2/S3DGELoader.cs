using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.v2
{
    [ExecuteAlways]
    public class S3DGELoader : MonoBehaviour
    {

        // reads a 3D slice from the file
        public static Dictionary<string, Vector3> LoadSlice(string addSliceFile)
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
    }
}