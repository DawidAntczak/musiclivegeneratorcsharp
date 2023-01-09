using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Tools;

namespace Splitter
{
    public static class MidiTimeSplitter
    {
        public static List<MidiFile> SplitByTime(this MidiFile midi, TimeSpan maxLenght)
        {
            var midis = new List<MidiFile>();

            var partLength = new MetricTimeSpan(maxLenght);
            var midiDuration = midi.GetDuration<MetricTimeSpan>();
            for (var startTimeSpan = new MetricTimeSpan(0); startTimeSpan < midiDuration; startTimeSpan += partLength)
            {
                midis.Add(midi.TakePart(startTimeSpan, partLength));
            }

            return midis;
        }

        public static List<MidiFile> CopyWithSpeedFraction(this MidiFile midi, params double[] speedFractions)
        {
            var midis = new List<MidiFile> { midi };

            foreach (var speedFraction in speedFractions)
            {
                var newMidi = midi.Clone();

                var setTempoEvents = newMidi.GetTrackChunks()
                    .SelectMany(x => x.Events.OfType<SetTempoEvent>());

                foreach (var setTempoEvent in setTempoEvents)
                {
                    setTempoEvent.MicrosecondsPerQuarterNote = (long)(setTempoEvent.MicrosecondsPerQuarterNote / speedFraction);
                }

                midis.Add(newMidi);
            }

            return midis;
        }

        public static void SetVolumeTo(this MidiFile midi, byte targetVolume)
        {
            var noteOnEvents = midi.GetTrackChunks()
                .SelectMany(x => x.Events.OfType<NoteOnEvent>());

            var noteOfEvents = midi.GetTrackChunks()
                .SelectMany(x => x.Events.OfType<NoteOnEvent>());

            foreach (var noteEvent in noteOnEvents)
            {
                noteEvent.Velocity = new SevenBitNumber(targetVolume);
            }

            foreach (var noteEvent in noteOfEvents)
            {
                noteEvent.Velocity = new SevenBitNumber(targetVolume);
            }
        }
    }
}
