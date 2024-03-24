using UnityEditor;
using UnityEngine;
using S4DGE;

namespace S4DGE
{
    [CustomEditor(typeof(BoxCollider4D))]
    public class BoxCollider4DEditor : Editor
    {
        void OnSceneGUI()
        {
            var sceneView = SceneView4DController.currentlyDrawingSceneView;
            if (sceneView == null) return;

            if(target is BoxCollider4D b4d)
            {
                Vector4 begin = b4d.t4d.LocalToWorld(b4d.corner),
                        end = b4d.t4d.LocalToWorld(b4d.corner + b4d.size);

                if(begin.w > end.w)
                {
                    Util.Swap(ref begin, ref end);
                }

                float minW = begin.w, maxW = end.w;

                float curW = sceneView.t4d.localPosition.w;
                if (curW >= minW && curW <= maxW)
                    Handles.DrawWireCube((begin+end)/2, end-begin);
            }
        }
    }
}