using System;
using System.Linq;

namespace Limxc.Tools.Common
{
    public class ProbRandom
    {
        private readonly (int Index, float Probability)[] _ip;
        private readonly Random _rnd;

        public ProbRandom(float[] probabilities)
        {
            if (probabilities.Length < 1)
                throw new ArgumentException(
                    $"The length of [{nameof(probabilities)}] must be greater than  0."
                );

            _rnd = new Random(DateTime.Now.GetHashCode());

            _ip = probabilities.Select((p, i) => (i, p)).OrderByDescending(p => p.p).ToArray();

            var total = probabilities.Sum();
            if (Math.Abs(total - 1) > 0.001f)
                _ip = probabilities.Select((p, i) => (i, p / total)).ToArray();
        }

        public int NextIndex()
        {
            var currentValue = _rnd.NextDouble();

            for (var i = 0; i < _ip.Length; i++)
                if (currentValue < _ip[i].Probability)
                    return _ip[i].Index;
                else
                    currentValue -= _ip[i].Probability;
            return _ip.Max(p => p.Index) - 1;
        }
    }
}
