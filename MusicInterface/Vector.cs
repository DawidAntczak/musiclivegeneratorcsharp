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
    }
}
