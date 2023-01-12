namespace MusicInterface
{
    public class ControlData
    {
        public Vector Mode { get; set; }
        public Vector AttackDensity { get; set; }
        public Vector AvgPitchesPlayed { get; set; }
        public Vector Entropy { get; set; }
        public bool Reset { get; set; }
        public int RequestedEventCount { get; set; }
    }
}
