using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using System;

namespace MusicInterface
{
    public static class MidiUtils
    {
        public static void Transpose(MidiFile midiFile, int keyAdjustmentInSemitones)
        {
            if (keyAdjustmentInSemitones == 0)
                return;

            midiFile.ProcessNotes(n => n.NoteNumber = ProcessCalculateNoteNumber(n.NoteNumber, keyAdjustmentInSemitones));
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
