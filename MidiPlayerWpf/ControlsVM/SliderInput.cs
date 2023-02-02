using MusicInterface;
using System;
using System.Windows.Controls;

namespace MidiPlayerWpf.ControlsVM
{
    internal class SliderInput
    {
        protected readonly Slider _slider;
        protected readonly Range _range;
        protected readonly CheckBox? _checkBox;

        public SliderInput(Slider slider, Range range, CheckBox? checkBox = null, bool isChecked = true)
        {
            _slider = slider;
            _range = range;
            _checkBox = checkBox;

            InitSlider(isChecked);
        }

        private void InitSlider(bool isChecked)
        {
            _slider.Minimum = _range.Start;
            _slider.Maximum = _range.End;
            _slider.Value = _range.Default;
            if (_checkBox != null)
                _checkBox.IsChecked = isChecked;
        }

        public Vector? GetVectorValue()
        {
            var intValue = GetIntValue();
            return intValue.HasValue ? Vector.OneHot((int)Math.Round(_range.End) + 1, intValue.Value) : null;
        }

        public double? GetDoubleValue()
        {
            return _checkBox == null || _checkBox.IsChecked == true ? _slider.Value : null;
        }

        public int? GetIntValue()
        {
            return _checkBox == null || _checkBox.IsChecked == true ? (int)Math.Round(_slider.Value) : null;
        }
    }
}
