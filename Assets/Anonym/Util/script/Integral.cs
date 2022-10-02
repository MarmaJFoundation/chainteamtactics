using UnityEngine;

namespace Anonym.Util
{
    public static class Integral{
        public static float Integrate(this AnimationCurve _Graph, float x_low, float x_high, int N_steps)
        {
            float res = (_Graph.Evaluate(x_low) + _Graph.Evaluate(x_high)) / 2;
            float h = (x_high - x_low) / N_steps;
            for (int i = 1; i < N_steps; i++)
            {
                res += _Graph.Evaluate(x_low + i * h);
            }
            return h * res;
        }
    }
}
