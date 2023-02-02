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
        private readonly MusicGenerationModel _musicReceiver;
        private readonly Func<ControlData> _controlsCollector;
        private readonly Func<PlayingParams> _playingParamsProvider;
        private readonly Action<string> _onLog;
        private readonly Action<Exception> _onError;
        private readonly int _milisecondsOffset;

        private readonly Queue<MidiFile> _nextMidis = new Queue<MidiFile>();

        private CancellationTokenSource _playingCts = null;

        private Playback _playback = null;

        private readonly AutoResetEvent _skipToNextEvent = new AutoResetEvent(false);

        public MusicPlayer(MusicGenerationModel musicReceiver, Func<ControlData> controlsCollector, Func<PlayingParams> playingParamsProvider,
            Action<string> onLog, Action<Exception> onError, int milisecondsOffset = 500)
        {
            _musicReceiver = musicReceiver;
            _controlsCollector = controlsCollector;
            _playingParamsProvider = playingParamsProvider;
            _onLog = onLog;
            _onError = onError;
            _milisecondsOffset = milisecondsOffset;
        }

        public MusicPlayer(MusicGenerationModel musicReceiver, Func<ControlData> inputCollector, Func<PlayingParams> playingParamsProvider, int milisecondsOffset = 500)
            : this(musicReceiver, inputCollector, playingParamsProvider, _ => { }, _ => { }, milisecondsOffset) { }

        public MusicPlayer(MusicGenerationModel musicReceiver, Func<ControlData> inputCollector, Func<PlayingParams> playingParamsProvider, Action<Exception> onError, int milisecondsOffset = 500)
            : this(musicReceiver, inputCollector, playingParamsProvider, _ => { }, onError, milisecondsOffset) { }

        public Task StartInBackground()
        {
            _playingCts = new CancellationTokenSource();

            return Task.Run(() => RunWithOnErrorCallback(() =>
            {
                using (var outputDevice = /*OutputDevice.GetByName("VirtualMIDISynth #1") ?? */OutputDevice.GetByName("Microsoft GS Wavetable Synth"))
                {
                    var contract = ControlDataContract.FromControlData(_controlsCollector());
                    Task.Run(() => RunWithOnErrorCallback(
                        () => _musicReceiver.Request(contract))
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
                var playingParams = _playingParamsProvider();

                midiFile.Transpose(playingParams.KeyAdjustmentInSemitones);
                midiFile.OverrideInstrument(playingParams.Instrument);
                midiFile.OverrideVelocity(playingParams.Velocity);

                _nextMidis.Enqueue(midiFile);

                while (_nextMidis.Count > 0) ;// await Task.Delay(1);

                var length = midiFile.GetDuration<MetricTimeSpan>();

                var toWait = (int)length.TotalMilliseconds - _milisecondsOffset;
                if (toWait > 0)
                    _skipToNextEvent.WaitOne(toWait);

                var contract = ControlDataContract.FromControlData(_controlsCollector());
                Task.Run(() => RunWithOnErrorCallback(() => _musicReceiver.Request(contract)));
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
