using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Anonym.RandomDesigner
{
    [System.Serializable]
    public class Range
    {
        public float Min = 0;
        public float Max = 100;

        public float GetRandomValue()
        {
            return UnityEngine.Random.Range(Min, Max);
        }

        public float Clamp(float value)
        {
            return Mathf.Clamp(value, Min, Max);
        }

        public float GetScaledValue(float fZeroToOne)
        {
            return Mathf.Lerp(Min, Max, fZeroToOne);
        }

        public float GetRelativePosition(float fValue)
        {
            if (ValueRange == 0)
                return 0;
            return Mathf.Clamp01((fValue - Min) / ValueRange);
        }

        public float ValueRange
        {
            get
            {
                return Max - Min;
            }
        }
    }

    [System.Serializable]
    public class RangedValue : Range
    {
        protected float value { get; set; }

    }
}