using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Anonym.Util
{
    [System.Serializable]
    public class GenericTagList<T> : GenericTagListBase, ISerializationCallbackReceiver where T : Tag
    {
        [SerializeField]
        string desc;

        [SerializeField]
        List<T> tags = new List<T>();
        public string[] getTagStringArray()
        {
            return tags.Where(t => t != null && !string.IsNullOrEmpty(t.tag)).Select(t => t.tag).ToArray();
        }

        void discint()
        {
            var newList = tags.Distinct().ToList();
            if (tags.Count != newList.Count)
            {
                Debug.Log("Taglist can not contain duplicate content.");
                tags = newList;
            }
        }

        public override void ClearGarbageSubAsset()
        {
            var childs = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(this));
            var expiredList = childs.Where(c => c != this && !tags.Contains(c as T)).ToArray();
            for (int i = 0; i < expiredList.Length; ++i)
            {
                DestroyImmediate(expiredList[i], true);
            }
            AssetDatabase.SaveAssets();
        }

        public override void AddEmptyElement()
        {
            tags.Add(null);
        }

        public T GetOtherTag(string tagString)
        {
            return tags.Find(t => t != null && !t.tag.Equals(tagString));
        }

        public T GetTag(string tagString)
        {
            return tags.Find(t => t != null && t.tag.Equals(tagString));
        }

        public override bool AddNewTag(string tagString)
        {
            if (GetTag(tagString) != null)
                return false;

            AddEmptyElement();
            T _instance = Tag.CreateAsset<T>();
            tags[tags.Count - 1] = _instance;
            _instance.Set(string.Format("T[{0}]", tagString), tagString);

            AssetDatabase.AddObjectToAsset(_instance, this);

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return true;
        }

        public override void ClearAllEmptyElement()
        {
            tags.RemoveAll(t => t == null);
        }

        public override bool bHasEmptyElement()
        {
            return tags.Contains(null);
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {

        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            discint();
        }
    }
}