using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Anonym.Util
{
    // This class is used only for the Generic Editor of GenericTagList<T>
    [System.Serializable]
    public abstract class GenericTagListBase : ScriptableObject
    {
        public virtual bool bHasEmptyElement(){ return false;   }

        public virtual void AddEmptyElement() { }
        public virtual bool AddNewTag(string tagString) { return false; }

        public virtual void ClearGarbageSubAsset(){ }
        public virtual void ClearAllEmptyElement(){ }
    }
}