using MusicInterface;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using MidiPlayerWpf.ControlsVM;
using System;
using Range = MidiPlayerWpf.ControlsVM.Range;

namespace MidiPlayerWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ListBoxInput _modeListBox;
        private readonly SliderInput _attackDensitySlider;
        private readonly SliderInput _avgPitchesPlayedSlider;
        private readonly SliderInput _entropySlider;
        private readonly SliderInput _requestedEventCount;
        private readonly SliderInput _keyAdjustmentSlider;

        private MusicReceiver? _musicReceiver;
        private MusicPlayer? _musicPlayer;

        public MainWindow()
        {
            InitializeComponent();

            _modeListBox = new ListBoxInput(ModeInputList, Controls.Modes);
            _attackDensitySlider = new SliderInput(AttackDensitySlider, new Range(Controls.AttackDensities.First(), Controls.AttackDensities.Last(), 2));
            _avgPitchesPlayedSlider = new SliderInput(AvgPitchesPlayedSlider, new Range(Controls.AvgPitchesPlayed.First(), Controls.AvgPitchesPlayed.Last(), 1));
            _entropySlider = new SliderInput(EntropySlider, new Range(Controls.Entropies.First(), Controls.Entropies.Last(), 1));
            _requestedEventCount = new SliderInput(RequestedEventCountSlider, new Range(25, 250, 50));
            _keyAdjustmentSlider = new SliderInput(KeyAdjustmentSlider, new Range(-12, 12, 0));
        }

        private ControlData CollectCurrentControlData()
        {
            ControlData data = null;
            Dispatcher.Invoke(() =>
            {
                data = new ControlData
                {
                    Mode = _modeListBox.GetVectorValue(),
                    AttackDensity = _attackDensitySlider.GetVectorValue(),
                    AvgPitchesPlayed = _avgPitchesPlayedSlider.GetVectorValue(),
                    Entropy = _entropySlider.GetVectorValue(),
                    Reset = ResetCheckbox.IsChecked.Value,
                    RequestedEventCount = _requestedEventCount.GetIntValue()
                };
                ResetCheckbox.IsChecked = false;
                var musicPlayer = _musicPlayer;
                musicPlayer?.SetKeyAdjustment(_keyAdjustmentSlider.GetIntValue());
            }
            );
            return data;
        }

        private void StartGeneratingButton_Click(object sender, RoutedEventArgs e)
        {
            _musicReceiver = new MusicReceiver(new WsClient("ws://localhost:7890/listener"));
            _musicPlayer = new MusicPlayer(_musicReceiver, CollectCurrentControlData, Console.WriteLine, Console.WriteLine);

            _musicReceiver.StartListening(_musicPlayer.EnqueueAndRequestNext);
            _musicPlayer.StartInBackground();
        }

        private void StopGeneratingButton_Click(object sender, RoutedEventArgs e)
        {
            _musicPlayer?.Stop();
            _musicReceiver?.StopListening();

            _musicReceiver?.Dispose();
            _musicReceiver = null;
            _musicPlayer = null;
        }
	}
}
