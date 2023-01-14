using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MusicInterface
{
    public class MusicPlayer
    {
        private readonly MusicReceiver _musicReceiver;
        private readonly Func<ControlData> _controlsCollector;
        private readonly Action<string> _onLog;
        private readonly Action<Exception> _onError;
        private readonly int _milisecondsOffset;

        private readonly Queue<MidiFile> _nextMidis = new Queue<MidiFile>();

        private CancellationTokenSource _playingCts = null;

        private int _keyAdjustmentInSemitones = 0;

        private Playback _playback = null;

        private readonly AutoResetEvent _skipToNextEvent = new AutoResetEvent(false);

        public MusicPlayer(MusicReceiver musicReceiver, Func<ControlData> inputCollector, Action<string> onLog, Action<Exception> onError, int milisecondsOffset = 500)
        {
            _musicReceiver = musicReceiver;
            _controlsCollector = inputCollector;
            _onLog = onLog;
            _onError = onError;
            _milisecondsOffset = milisecondsOffset;
        }

        public MusicPlayer(MusicReceiver musicReceiver, Func<ControlData> inputCollector, int milisecondsOffset = 500)
            : this(musicReceiver, inputCollector, _ => { }, _ => { }, milisecondsOffset) { }

        public MusicPlayer(MusicReceiver musicReceiver, Func<ControlData> inputCollector, Action<Exception> onError, int milisecondsOffset = 500)
            : this(musicReceiver, inputCollector, _ => { }, onError, milisecondsOffset) { }

        public int AddSemitonesToKeyAdjustment(int semitones)
        {
            Interlocked.Add(ref _keyAdjustmentInSemitones, semitones);
            return _keyAdjustmentInSemitones;
        }

        public void SetKeyAdjustment(int semitones)
        {
            _keyAdjustmentInSemitones = semitones;
        }

        public Task StartInBackground()
        {
            _playingCts = new CancellationTokenSource();

            return Task.Run(() => RunWithOnErrorCallback(() =>
            {
                using (var outputDevice = /*OutputDevice.GetByName("VirtualMIDISynth #1") ?? */OutputDevice.GetByName("Microsoft GS Wavetable Synth"))
                {
                    var contract = ControlDataContract.FromControlData(_controlsCollector());
                    Task.Run(() => RunWithOnErrorCallback(
                        () => _musicReceiver.SendControls(contract))
                    );

                    while (!_playingCts.IsCancellationRequested)
                    {
                        RunWithOnErrorCallback(() =>
                        {
                            if (_nextMidis.Count > 0)
                            {
                                var midiFile = _nextMidis.Dequeue();
                                using (_playback = midiFile.GetPlayback(outputDevice))
                                {
                                    _onLog($"Starting to play next segment (length: {midiFile.GetDuration<MetricTimeSpan>().TotalSeconds} s)");
                                    _playback.Play();
                                }
                            }
                        });
                    }
                }
            }));
        }

        public void Stop()
        {
            _playingCts?.Cancel();

            var playback = _playback;
            try { playback?.Stop(); }
            catch { }
        }

        public void SkipToNext()
        {
            _nextMidis.Clear();
            var playback = _playback;
            try { playback?.Stop(); }
            catch { }
            finally { _skipToNextEvent.Set(); }
        }

        public void EnqueueAndRequestNext(byte[] music)
        {
            RunWithOnErrorCallback(() =>
            {
                _playingCts = new CancellationTokenSource();

                var midiStream = new MemoryStream(music);
                var midiFile = MidiFile.Read(midiStream);
                MidiUtils.Transpose(midiFile, _keyAdjustmentInSemitones);
                _nextMidis.Enqueue(midiFile);

                while (_nextMidis.Count > 0) ;// await Task.Delay(1);

                var length = midiFile.GetDuration<MetricTimeSpan>();

                var toWait = (int)length.TotalMilliseconds - _milisecondsOffset;
                if (toWait > 0)
                    _skipToNextEvent.WaitOne(toWait);

                var contract = ControlDataContract.FromControlData(_controlsCollector());
                Task.Run(() => RunWithOnErrorCallback(() => _musicReceiver.SendControls(contract)));
            });
        }

        private void RunWithOnErrorCallback(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                _onError(e);
            }
        }
    }
}
