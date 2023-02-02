using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicInterface
{
    public class Vector
    {
        private readonly double[] _vector;

        private Vector(double[] vector)
        {
            _vector = vector;
        }

        public static Vector OneHot(int length, int value)
        {
            if (length < 1 || value >= length)
                throw new ArgumentException();


            var vector = new double[length];
            vector[value] = 1;

            return new Vector(vector);
        }

        public static Vector EqualDistribution(int length)
        {
            if (length < 1)
                throw new ArgumentException();

            var value = 1.0 / length;
            var vector = Enumerable.Repeat(value, length).ToArray();

            return new Vector(vector);
        }

        public static Vector NormalizedNormalDistribution(int length, double mean, double standardDeviation)
        {
            if (standardDeviation <= 0)
                throw new ArgumentException();

            var ndd = NormalDistributionDensity(mean, standardDeviation);

            var vector = Enumerable.Range(0, length).Select(ndd).ToArray();

            return FromArray(vector).Normalized();
        }

        public static Vector FromArray(double[] array)
        {
            if (array == null || array.Length < 1)
                throw new ArgumentException();

            return new Vector(array);
        }

        public IEnumerable<double> ToEnumerable()
        {
            return _vector;
        }

        public Vector Normalized()
        {
            var m = _vector.Sum();
            return FromArray(_vector.Select(x => x / m).ToArray());
        }

        public override string ToString()
        {
            return $"[{string.Join(", ", _vector)}]";
        }

        private static Func<int, double> NormalDistributionDensity(double mean, double standardDeviation)
        {
            return x =>
                1
                /
                (standardDeviation * Math.Sqrt(2 * Math.PI))
                *
                Math.Pow(
                    Math.E,
                    - Math.Pow(x - mean, 2) / (2 * Math.Pow(standardDeviation, 2))
                );
        }
    }
}
