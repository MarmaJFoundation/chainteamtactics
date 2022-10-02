using System.Collections.Generic;
using UnityEngine;

namespace Anonym.Util
{
    using Isometric;
    public static partial class RectUtil
    {
        public static Rect Sub_Vertical(ref Rect rt, float sub_y)
        {
            Rect new_rt = new Rect(rt.position, new Vector2(rt.width, sub_y));
            rt.yMin = new_rt.yMax;
            return new_rt;
        }
        public static Rect Sub_Horizontal(ref Rect rt, float sub_x)
        {
            Rect new_rt = new Rect(rt.position, new Vector2(sub_x, rt.height));
            rt.xMin = new_rt.xMax;
            return new_rt;
        }
#if UNITY_EDITOR
        public static Rect Sub_Vertical(ref Rect rt)
        {
            return Sub_Vertical(ref rt, UnityEditor.EditorGUIUtility.singleLineHeight);
        }
        public static Rect[] Divid_TileSide(this Rect rt)
        {
            Rect[] result = new Rect[3];            
            result[0] = rt.Divid_TileSide(Iso2DObject.Type.Side_X);
            result[1] = rt.Divid_TileSide(Iso2DObject.Type.Side_Y);
            result[2] = rt.Divid_TileSide(Iso2DObject.Type.Side_Z);        
            return result;
        }
        public static Rect Divid_TileSide(this Rect rt, Iso2DObject.Type _side)
        {
            float _fDivision = IsoMap.fMagicValue;
            float[] _x_List = new float[] {1/2f, 1/2f};
            float[] _y_List_U = new float[] {(_fDivision + 1) / (_fDivision + 2)};
            float[] _y_List_D = new float[] {_fDivision / (_fDivision + 2), 
                2 / (_fDivision + 2)};
            switch(_side)
            {
                case Iso2DObject.Type.Side_X:
                    return rt.Division(_x_List, _y_List_U)[0];
                case Iso2DObject.Type.Side_Y:
                    return rt.Division(null, _y_List_D)[1];
                case Iso2DObject.Type.Side_Z:
                    return rt.Division(_x_List, _y_List_U)[1];
            }
            return rt;
        }
#endif
    }
}