using System.Collections.Generic;
using System.Linq;

namespace MusicInterface
{
    public static class Controls
    {
        public static IDictionary<string, Vector> Modes = new Dictionary<string, Vector>
        {
            { "major", Vector.OneHot(3, 0) },
            { "minor", Vector.OneHot(3, 1) },
            { "unspecified", Vector.EqualDistribution(3) }
        };

        public static IEnumerable<int> AttackDensities = Enumerable.Range(0, 6);

        public static IEnumerable<int> AvgPitchesPlayed = Enumerable.Range(0, 3);

        public static IEnumerable<int> Entropies = Enumerable.Range(0, 3);


        private static readonly IEnumerable<string> _sounds = new List<string>
        {
            "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
        };
    }
}
