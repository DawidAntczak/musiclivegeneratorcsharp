using MusicInterface;
using System;
using System.Windows.Controls;

namespace MidiPlayerWpf.ControlsVM
{
    internal class SliderVector
    {
        protected readonly Slider _slider;
        protected readonly Range _range;

        public SliderVector(Slider slider, Range range)
        {
            _slider = slider;
            _range = range;

            InitSlider();
        }

        private void InitSlider()
        {
            _slider.Minimum = _range.Start;
            _slider.Maximum = _range.End;
            _slider.Value = _range.Default;
        }

        public Vector GetValue()
        {
            return Vector.OneHot(_range.End + 1, GetIntValue());
        }

        public int GetIntValue()
        {
            return (int)Math.Round(_slider.Value);
        }
    }
}
