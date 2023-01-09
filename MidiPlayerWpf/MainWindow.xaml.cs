using MusicInterface;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace MidiPlayerWpf
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private readonly MusicReceiver _musicReceiver;
        private readonly MusicPlayer _musicPlayer;

        public MainWindow()
		{
			InitializeComponent();
			InitHistogramInputList();
            InitModeInputList();
            _musicReceiver = new MusicReceiver(new WsClient("ws://localhost:7890/listener"));
            _musicPlayer = new MusicPlayer(_musicReceiver, CollectCurrentInputData);
        }

		private InputData CollectCurrentInputData()
		{
			InputData data = null;
			Dispatcher.Invoke(() =>
			{
                var modeName = ModeInputList.SelectedItem.ToString();
                var histogramName = HistogramInputList.SelectedItem.ToString();
				data = new InputData
				{
                    Mode = Inputs.Modes[modeName],
					NoteDensity = (int)Math.Round(DensityInputSlider.Value),
					Reset = ResetCheckbox.IsChecked.Value,
                    PitchHistogram = Inputs.Histograms[histogramName],
                    HistogramName = histogramName,
                };
				ResetCheckbox.IsChecked = false;
			}
			);
			return data;
		}

		private void InitHistogramInputList()
		{
			foreach (var key in Inputs.Histograms.Keys)
			{
				HistogramInputList.Items.Add(key);
			}
			HistogramInputList.SelectedItem = HistogramInputList.Items[0];
		}

        private void InitModeInputList()
        {
            foreach (var key in Inputs.Modes.Keys)
            {
                ModeInputList.Items.Add(key);
            }
            ModeInputList.SelectedItem = ModeInputList.Items[0];
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

		private IEnumerable<FileStream> LoadMidis()
		{
			var dirPath = @"C:\Repos\EmotionBox\output\2022-10-15 14-09-32";
			return Directory.GetFiles(dirPath)
				.Select(f => new FileStream(f, FileMode.Open)).ToArray();
		}
	}
}
