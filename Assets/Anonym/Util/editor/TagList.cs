using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Anonym.Util
{
    [System.Serializable]
    [CreateAssetMenu(fileName = "TagList", menuName = "Anonym/TagList", order = 101)]
    public class TagList : GenericTagList<Tag>{ }
}