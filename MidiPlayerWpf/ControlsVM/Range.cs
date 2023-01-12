namespace MidiPlayerWpf.ControlsVM
{
    internal class Range
    {
        private readonly int _start;
        private readonly int _end;
        private readonly int _default;

        public int Start => _start;
        public int End => _end;
        public int Default => _default;

        public Range(int start, int end, int defaultValue)
        {
            _start = start;
            _end = end;
            _default = defaultValue;
        }
    }
}
