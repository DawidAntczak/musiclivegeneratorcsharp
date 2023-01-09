using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataTransporter
{
    public class MusicPlayer
    {
        private readonly MusicReceiver _musicReceiver;
        private readonly Func<InputData> _inputCollector;

        private readonly Queue<MidiFile> _nextMidis = new();

        private CancellationTokenSource _playingCts = null;

        public MusicPlayer(MusicReceiver musicReceiver, Func<InputData> inputCollector)
        {
            _musicReceiver = musicReceiver;
            _inputCollector = inputCollector;
        }

        public Task StartInBackground()
        {
            _playingCts = new CancellationTokenSource();

            return Task.Run(() =>
            {
                using var outputDevice = /*OutputDevice.GetByName("VirtualMIDISynth #1") ?? */OutputDevice.GetByName("Microsoft GS Wavetable Synth");

                Task.Run(() => _musicReceiver.SendInput(_inputCollector()));

                while (!_playingCts.IsCancellationRequested)
                {
                    if (_nextMidis.TryDequeue(out var midiFile))
                    {
                        using var playback = midiFile.GetPlayback(outputDevice);
                        Console.WriteLine("Starting to play next segment");
                        playback.Play();
                    }
                }
            });
        }

        public void Stop()
        {
            _playingCts?.Cancel();
        }

        public void EnqueueAndRequest(byte[] music)
        {
            _playingCts = new CancellationTokenSource();
            var midiStream = new MemoryStream(music);
            var midiFile = MidiFile.Read(midiStream);
            _nextMidis.Enqueue(midiFile);

            while (_nextMidis.Count > 0) ;// await Task.Delay(1);

            var length = midiFile.GetDuration<MetricTimeSpan>();
            Console.WriteLine($"Received track with length: {length.TotalSeconds} s");

            var toWait = (int)length.TotalMilliseconds - 1000;
            if (toWait > 0)
                //await Task.Delay(toWait);
                Task.Delay(toWait).GetAwaiter().GetResult();

            Task.Run(() => _musicReceiver.SendInput(_inputCollector()));
        }
    }
}
