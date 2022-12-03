using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace v2
{
    [ExecuteAlways]
    public class S4DGELoader : MonoBehaviour
    {
        public string filePath;


        // reloading the file as workaround for broken shape bug
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            //TODO this is so sus
            foreach (var obj in FindObjectsOfType<S4DGELoader>())
            {
                obj.ReloadFile();
            }
        }

        public void ReloadFile()
        {
            GetComponents<IShape4DRenderer>().ToList().ForEach(x => x.Shape = S4DGE.LoadS4DGE(filePath));
        }

        private void Awake()
        {
            ReloadFile();
        }
    }
}