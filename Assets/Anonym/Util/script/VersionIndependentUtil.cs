using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Anonym.Util
{
    public static class VersionIndependentUtil
    {
#if !UNITY_2019_2_OR_NEWER
        public static bool TryGetComponent<T>(this GameObject go, out T result) where T : Component
        {
            result = go != null ? go.GetComponent<T>() : null;
            return result != null;
        }

        public static bool TryGetComponent<T>(this Component com, out T result) where T : Component
        {
            result = com != null ? com.GetComponent<T>() : null;
            return result != null;
        }
#endif
    }
}