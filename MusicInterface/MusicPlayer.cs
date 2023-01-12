using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MusicInterface
{
    public class MusicPlayer
    {
        private readonly MusicReceiver _musicReceiver;
        private readonly Func<ControlData> _controlsCollector;
        private readonly int _milisecondsOffset;

        private readonly Queue<MidiFile> _nextMidis = new Queue<MidiFile>();

        private CancellationTokenSource _playingCts = null;

        public MusicPlayer(MusicReceiver musicReceiver, Func<ControlData> inputCollector, int milisecondsOffset = 500)
        {
            _musicReceiver = musicReceiver;
            _controlsCollector = inputCollector;
            _milisecondsOffset = milisecondsOffset;
        }

        public Task StartInBackground()
        {
            _playingCts = new CancellationTokenSource();

            return Task.Run(() =>
            {
                using (var outputDevice = /*OutputDevice.GetByName("VirtualMIDISynth #1") ?? */OutputDevice.GetByName("Microsoft GS Wavetable Synth"))
                {
                    var contract = ControlDataContract.FromControlData(_controlsCollector());
                    Task.Run(() => _musicReceiver.SendControls(contract));

                    while (!_playingCts.IsCancellationRequested)
                    {
                        if (_nextMidis.Count > 0)
                        {
                            var midiFile = _nextMidis.Dequeue();
                            using (var playback = midiFile.GetPlayback(outputDevice))
                            {
                                Console.WriteLine("Starting to play next segment");
                                playback.Play();
                            }
                        }
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

            var toWait = (int)length.TotalMilliseconds - _milisecondsOffset;
            if (toWait > 0)
                //await Task.Delay(toWait);
                Task.Delay(toWait).GetAwaiter().GetResult();

            var contract = ControlDataContract.FromControlData(_controlsCollector());
            Task.Run(() => _musicReceiver.SendControls(contract));
        }
    }
}
