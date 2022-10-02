using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Anonym.Util
{
    using Isometric;

    public enum InGameDirection
    {
        BaseField = 0,
        No_Move = 0,
        Right_Move = 1,
        Right_Rotate = -1 * Right_Move,
        RD_Move = 2,
        DR_Move = RD_Move,
        RD_Rotate = -1 * RD_Move,
        Down_Move = 3,
        Down_Rotate = -1 * Down_Move,
        LD_Move = 4,
        DL_Move = LD_Move,
        LD_Rotate = -1 * LD_Move,
        Left_Move = 5,
        Left_Rotate = -1 * Left_Move,
        LT_Move = 6,
        TL_Move = LT_Move,
        LT_Rotate = -1 * LT_Move,
        Top_Move = 7,
        Top_Rotate = -1 * Top_Move,
        RT_Move = 8,
        TR_Move = RT_Move,
        RT_Rotate = -1 * RT_Move,
        Dash = 9,

        Jump_Move = 101,

        OppositeDir = 4,
        OutField = 100,
        ParentField = 102,
        None = 1000,
    }

    public static class InGameDirectionUtil
    {
        const int way = 8;

        static int To8WayDir(int iDir)
        {
            iDir = iDir % way;
            return iDir == 0 ? way : iDir;
        }
        public static InGameDirection[] Get(this InGameDirection dir, bool self = false, bool sides = false, bool rights = false, bool sides_of_opposite = false, bool opposite = false)
        {
            // 8 Way directions
            // 1(Right) ~ 8(TopRight) loop

            int iDir = (int)dir;
            List<int> result = new List<int>();
            if (self)
                result.Add(To8WayDir(iDir));

            if (sides)
            {
                result.Add(To8WayDir(iDir + 1));
                result.Add(To8WayDir(iDir + way - 1));
            }

            if (rights)
            {
                result.Add(To8WayDir(iDir + 2));
                result.Add(To8WayDir(iDir + way - 2));
            }

            if (sides_of_opposite)
            {
                result.Add(To8WayDir(iDir + 3));
                result.Add(To8WayDir(iDir + way - 3));
            }

            if (opposite)
                result.Add(To8WayDir(iDir + 4));

            return result.Select(i => (InGameDirection)i).ToArray();
        }
    }
}