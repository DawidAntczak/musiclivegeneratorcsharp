using MusicInterface;
using System.Collections.Generic;
using System.Windows.Controls;

namespace MidiPlayerWpf.ControlsVM
{
    internal class ListBoxVector
    {
        private readonly ListBox _listBox;
        private readonly IDictionary<string, Vector> _values;

        public ListBoxVector(ListBox listBox, IDictionary<string, Vector> values)
        {
            _listBox = listBox;
            _values = values;

            InitListBox();
        }

        public Vector GetSelectedValue()
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
