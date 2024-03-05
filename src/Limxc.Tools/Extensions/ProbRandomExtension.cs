using System;
using System.Linq;
using Limxc.Tools.Common;

namespace Limxc.Tools.Extensions
{
    public static class ProbRandomExtension
    {
        public static int RandomIndex(this float[] probabilities)
        {
            return new ProbRandom(probabilities.Select(Convert.ToSingle).ToArray()).NextIndex();
        }

        public static int RandomIndex(this double[] probabilities)
        {
            return new ProbRandom(probabilities.Select(Convert.ToSingle).ToArray()).NextIndex();
        }

        public static int RandomIndex(this int[] probabilities)
        {
            return new ProbRandom(probabilities.Select(Convert.ToSingle).ToArray()).NextIndex();
        }
    }
}
