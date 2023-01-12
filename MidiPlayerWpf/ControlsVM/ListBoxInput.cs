using MusicInterface;
using System.Collections.Generic;
using System.Windows.Controls;

namespace MidiPlayerWpf.ControlsVM
{
    internal class ListBoxInput
    {
        private readonly ListBox _listBox;
        private readonly IDictionary<string, Vector> _values;

        public ListBoxInput(ListBox listBox, IDictionary<string, Vector> values)
        {
            _listBox = listBox;
            _values = values;

            InitListBox();
        }

        public Vector GetVectorValue()
        {
            return _values[(string)_listBox.SelectedValue];
        }

        private void InitListBox()
        {
            foreach (var key in _values.Keys)
            {
                _listBox.Items.Add(key);
            }
            _listBox.SelectedItem = _listBox.Items[0];
        }
    }
}
