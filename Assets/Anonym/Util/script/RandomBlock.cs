using UnityEngine;

namespace Anonym.Util
{

    /**
     * @brief It has two floating point values.\n
     * You can get intermediate values and random values, as well as two values.
     */
    [System.Serializable]
    public class RandomBlock
    {
        [SerializeField]
        float fLeft;
        [SerializeField]
        float fRight;

        public float Left { get { return fLeft; } } ///< Get minimum value of range (Inclusive)
        public float Right { get { return fRight; } } ///< Get maximum value of range (Inclusive)
        public float Middle { get { return 0.5f * (fRight + fLeft); } } ///< Get intermediate of range
        public float Random { get { return UnityEngine.Random.Range(fLeft, fRight); } } ///< Get random value of range

        /**
         * @brief Constructor with values
         */
        public RandomBlock(float leftValue = 0, float rightValue = 1)
        {
            SetValue(leftValue, rightValue);
        }
        /**
         * @brief Specify a range.
         * @param leftValue Minimum value of range (Inclusive)
         * @param rightValue Maximum value of range (Inclusive)
         */
        public void SetValue(float leftValue, float rightValue)
        {
            fLeft = leftValue;
            fRight = rightValue;
        }
        /**
         * @brief Returns a random range as a string.
         */
        public override string ToString()
        {
            return string.Format("Random({0} ~ {1})", fLeft, fRight);
        }
    }
}