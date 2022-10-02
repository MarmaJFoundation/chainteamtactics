using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Anonym.Util
{
    public static class ScriptableObjectEditorEX
    {
        public static List<T> GetSubObjects<T>(Object asset) where T : Object
        {
            List<T> ofType = new List<T>();
#if UNITY_EDITOR
            Object[] objs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(asset));
            foreach (Object o in objs)
            {
                if (o is T)
                {
                    ofType.Add(o as T);
                }
            }
#endif
            return ofType;
        }

    }
}
