using System.Windows.Controls;

namespace MidiPlayerWpf.ControlsVM
{
    internal class TextInput
    {
        private readonly TextBox _textBox;

        public TextInput(TextBox textBox, string defaultValue)
        {
            _textBox = textBox;

            InitTextInput(defaultValue);
        }

        public string GetValue()
        {
            return _textBox.Text;
        }

        private void InitTextInput(string defaultValue)
        {
            _textBox.Text = defaultValue;
        }
    }
}
