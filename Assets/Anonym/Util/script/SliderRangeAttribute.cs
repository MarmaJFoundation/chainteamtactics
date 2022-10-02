using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Anonym.RandomDesigner
{
    public class SliderRangeAttribute : PropertyAttribute
    {
        public float minLimit, maxLimit;

        public SliderRangeAttribute(float minLimit, float maxLimit)
        {
            this.minLimit = minLimit;
            this.maxLimit = maxLimit;
        }
    }
}