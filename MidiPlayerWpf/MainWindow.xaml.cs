using MusicInterface;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using MidiPlayerWpf.ControlsVM;

namespace MidiPlayerWpf
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly MusicReceiver _musicReceiver;
        private readonly MusicPlayer _musicPlayer;

        private readonly ListBoxVector _modeListBox;
        private readonly SliderVector _attackDensitySlider;
        private readonly SliderVector _avgPitchesPlayedSlider;
        private readonly SliderVector _entropySlider;
        private readonly SliderVector _requestedEventCount;

        public MainWindow()
		{
			InitializeComponent();

            _musicReceiver = new MusicReceiver(new WsClient("ws://localhost:7890/listener"));
            _musicPlayer = new MusicPlayer(_musicReceiver, CollectCurrentControlData);

            _modeListBox = new ListBoxVector(ModeInputList, Controls.Modes);
            _attackDensitySlider = new SliderVector(AttackDensitySlider, new ControlsVM.Range(Controls.AttackDensities.First(), Controls.AttackDensities.Last(), 0));
            _avgPitchesPlayedSlider = new SliderVector(AvgPitchesPlayedSlider, new ControlsVM.Range(Controls.AvgPitchesPlayed.First(), Controls.AvgPitchesPlayed.Last(), 0));
            _entropySlider = new SliderVector(EntropySlider, new ControlsVM.Range(Controls.Entropies.First(), Controls.Entropies.Last(), 0));
            _requestedEventCount = new SliderVector(RequestedEventCountSlider, new ControlsVM.Range(25, 250, 100));
        }

        private ControlData CollectCurrentControlData()
		{
			ControlData data = null;
			Dispatcher.Invoke(() =>
            {
                data = new ControlData
                {
                    Mode = _modeListBox.GetSelectedValue(),
                    AttackDensity = _attackDensitySlider.GetValue(),
                    AvgPitchesPlayed = _avgPitchesPlayedSlider.GetValue(),
                    Entropy = _entropySlider.GetValue(),
                    Reset = ResetCheckbox.IsChecked.Value,
                    RequestedEventCount = _requestedEventCount.GetIntValue()
                };
				ResetCheckbox.IsChecked = false;
			}
			);
			return data;
		}

        private void StartGeneratingButton_Click(object sender, RoutedEventArgs e)
		{
			_musicReceiver.StartListening(_musicPlayer.EnqueueAndRequest);
            _musicPlayer.StartInBackground();
        }

		private void StopGeneratingButton_Click(object sender, RoutedEventArgs e)
		{
            _musicPlayer.Stop();
            _musicReceiver.StopListening();
		}
	}
}
