using System;

namespace MidiPlayerWpf.ControlsVM
{
    internal class Range
    {
        private readonly double _start;
        private readonly double _end;
        private readonly double _default;

        public double Start => _start;
        public double End => _end;
        public double Default => _default;

        public Range(double start, double end, double defaultValue)
        {
            _start = start;
            _end = end;
            _default = defaultValue;
        }
    }
}
