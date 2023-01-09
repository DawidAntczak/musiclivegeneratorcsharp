namespace DataTransporter
{
	public class InputData
	{
        public IEnumerable<int> Mode { get; set; }
		public int NoteDensity { get; set; }
		public bool Reset { get; set; }

        [Obsolete]
        public IEnumerable<int> PitchHistogram { get; set; }
        [Obsolete]
        public string HistogramName { get; set; }
    }
}
