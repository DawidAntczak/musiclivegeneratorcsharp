using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;

namespace MusicInterface
{
    public static class MidiUtils
    {
        public static void Transpose(this MidiFile midiFile, int keyAdjustmentInSemitones)
        {
            if (keyAdjustmentInSemitones == 0)
                return;

            midiFile.ProcessNotes(n => n.NoteNumber = ProcessCalculateNoteNumber(n.NoteNumber, keyAdjustmentInSemitones));
        }

        public static void OverrideInstrument(this MidiFile midiFile, int instrument)
        {
            if (instrument == 1)
                return;

            midiFile.ProcessTimedEvents(e => {
                if (e.Event.EventType == MidiEventType.ProgramChange)
                {
                    var pce = (ProgramChangeEvent)e.Event;
                    pce.ProgramNumber = (SevenBitNumber)instrument;
                }
            });
        }

        public static void OverrideVelocity(this MidiFile midiFile, int velocity)
        {
            midiFile.ProcessNotes(n => n.Velocity = new SevenBitNumber((byte)velocity));
        }

        private static SevenBitNumber ProcessCalculateNoteNumber(SevenBitNumber noteNumber, int keyAdjustmentInSemitones)
        {
            int targetNoteNumber = (int)noteNumber + keyAdjustmentInSemitones;

            if (targetNoteNumber < 0)
                targetNoteNumber += 12;

            if (targetNoteNumber > 127)
                targetNoteNumber -= 12;

            return (SevenBitNumber)targetNoteNumber;
        }
    }
}
