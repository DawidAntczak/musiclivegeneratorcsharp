﻿using System.Collections.Generic;

namespace MusicInterface
{
	public class ControlDataContract
	{
        public IEnumerable<double> Mode { get; private set; }
		public IEnumerable<double> AttackDensity { get; private set; }
        public IEnumerable<double> AvgPitchesPlayed { get; private set; }
        public IEnumerable<double> Entropy { get; private set; }
        public bool Reset { get; private set; }
        public int RequestedEventCount { get; private set; }

        private ControlDataContract(IEnumerable<double> mode, IEnumerable<double> noteDensity, IEnumerable<double> avgPitchesPlayed, IEnumerable<double> entropy,
            bool reset, int requestedEventCount)
        {
            Mode = mode;
            AttackDensity = noteDensity;
            AvgPitchesPlayed = avgPitchesPlayed;
            Entropy = entropy;
            Reset = reset;
            RequestedEventCount = requestedEventCount;
        }

        public static ControlDataContract FromControlData(ControlData inputData)
        {
            return new ControlDataContract(
                inputData.Mode.ToEnumerable(),
                inputData.AttackDensity.ToEnumerable(),
                inputData.AvgPitchesPlayed.ToEnumerable(),
                inputData.Entropy.ToEnumerable(),
                inputData.Reset,
                inputData.RequestedEventCount
                );
        }
    }
}
