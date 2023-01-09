﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicInterface
{
	public static class Inputs
	{
		private static readonly IEnumerable<string> _sounds = new List<string>
		{
			"C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B"
		};

		private static readonly (IEnumerable<int> Major, IEnumerable<int> Minor) _cHistograms =
		(
			"3,0,1,0,1,2,0,2,0,1,0,1".Split(',').Select(int.Parse),
			"3,0,1,1,0,2,0,2,1,0,1,0".Split(',').Select(int.Parse)
		);

		public static Dictionary<string, IEnumerable<int>> Histograms { get; private set; } = GetHistograms();

        public static Dictionary<string, IEnumerable<int>> Modes { get; private set; } = GetModes();

        private static Dictionary<string, IEnumerable<int>> GetHistograms()
		{
			return Enumerable.Empty<KeyValuePair<string, IEnumerable<int>>>()
				.Concat(
					_sounds.SelectMany((sound, i) =>
					{
						return new List<KeyValuePair<string, IEnumerable<int>>>()
						{
							new KeyValuePair<string, IEnumerable<int>>($"{sound} Major", _cHistograms.Major.MoveHistogram(i)),
                            new KeyValuePair<string, IEnumerable<int>>($"{sound} Minor", _cHistograms.Minor.MoveHistogram(i))
						};
					})
				)
				.Append(
                    new KeyValuePair<string, IEnumerable<int>>("Equal", Enumerable.Repeat(1, 12)))
				.Append(
                    new KeyValuePair<string, IEnumerable<int>>("Empty", null))
				.Append(
                    new KeyValuePair<string, IEnumerable<int>>("Only C", Enumerable.Repeat(1, 1).Concat(Enumerable.Repeat(0, 11))))
				.ToDictionary(x => x.Key, x => x.Value);
		}

        private static Dictionary<string, IEnumerable<int>> GetModes()
        {
            return new Dictionary<string, IEnumerable<int>>
            {
                { "Major", new [] { 1, 0, 0 } },
                { "Minor", new [] { 0, 1, 0 } },
                { "Mixed", new [] { 0, 0, 1 } },
            };
        }

        private static IEnumerable<int> MoveHistogram(this IEnumerable<int> histogram, int soundsToMove)
		{
			if (soundsToMove >= histogram.Count())
				throw new ArgumentException(null, nameof(soundsToMove));

            //var lastSounds = histogram.TakeLast(soundsToMove);
            //var withoutSkiped = histogram.SkipLast(soundsToMove);
            return histogram; //lastSounds.Concat(withoutSkiped);
		}
	}
}
