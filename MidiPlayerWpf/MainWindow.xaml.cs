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
        private readonly TextInput _serverAddressText;
        private readonly ListBoxInput _modeListBox;
        private readonly SliderInput _attackDensitySlider;
        private readonly SliderInput _avgPitchesPlayedSlider;
        private readonly SliderInput _entropySlider;
        private readonly SliderInput _requestedTimeLength;
        private readonly SliderInput _temperatureSlider;
        private readonly SliderInput _keyAdjustmentSlider;
        private readonly SliderInput _instrumentSlider;

        private MusicGenerationModel? _musicGenerationModel;
        private MusicPlayer? _musicPlayer;

        public MainWindow()
        {
            InitializeComponent();

            _serverAddressText = new TextInput(ServerAddressInput, "ws://localhost:7890/listener");
            _modeListBox = new ListBoxInput(ModeInputList, Controls.Modes);
            _attackDensitySlider = new SliderInput(AttackDensitySlider, new Range(Controls.AttackDensities.First(), Controls.AttackDensities.Last(), 2), AttackDensityCheckbox);
            _avgPitchesPlayedSlider = new SliderInput(AvgPitchesPlayedSlider, new Range(Controls.AvgPitchesPlayed.First(), Controls.AvgPitchesPlayed.Last(), 1), AvgPitchesPlayedCheckbox);
            _entropySlider = new SliderInput(EntropySlider, new Range(Controls.Entropies.First(), Controls.Entropies.Last(), 1), EntropyCheckbox);
            _requestedTimeLength = new SliderInput(RequestedEventCountSlider, new Range(1, 15, 5));
            _temperatureSlider = new SliderInput(TemperatureSlider, new Range(0.5, 5, 1), TemperatureCheckbox);
            _keyAdjustmentSlider = new SliderInput(KeyAdjustmentSlider, new Range(-12, 12, 0));
            _instrumentSlider = new SliderInput(InstrumentSlider, new Range(0, 127, 1));
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
                    Temperature = _temperatureSlider.GetDoubleValue().Value,
                    RequestedTimeLength = _requestedTimeLength.GetIntValue()
                };
                ResetCheckbox.IsChecked = false;
            }
            );
            return data;
        }

        private PlayingParams CollectPlayingParams()
        {
            PlayingParams playingParams = null;
            Dispatcher.Invoke(() =>
            {
                playingParams = new PlayingParams
                {
                    KeyAdjustmentInSemitones = _keyAdjustmentSlider.GetIntValue().Value,
                    Instrument = _instrumentSlider.GetIntValue().Value
                };
                ResetCheckbox.IsChecked = false;
            }
            );
            return playingParams;
        }

        private void StartGeneratingButton_Click(object sender, RoutedEventArgs args)
        {
            try
            {
                var serverAddress = _serverAddressText.GetValue();
                _musicGenerationModel = new MusicGenerationModel(serverAddress);
                _musicPlayer = new MusicPlayer(_musicGenerationModel, CollectCurrentControlData, CollectPlayingParams, Console.WriteLine, Console.WriteLine);

                _musicGenerationModel.Connect(_musicPlayer.EnqueueAndRequestNext);
                _musicPlayer.StartInBackground();
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private void StopGeneratingButton_Click(object sender, RoutedEventArgs args)
        {
            _musicPlayer?.Stop();
            _musicGenerationModel?.Disconnect();

            _musicGenerationModel?.Dispose();
            _musicGenerationModel = null;
            _musicPlayer = null;
        }
	}
}
