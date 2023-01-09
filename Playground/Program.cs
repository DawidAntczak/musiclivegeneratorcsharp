// See https://aka.ms/new-console-template for more information
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Melanchall.DryWetMidi.Multimedia;
using Melanchall.DryWetMidi.Tools;

Console.WriteLine("Hello, World!");

/*
var midis = Directory.GetFiles(@"C:\Repos\short_midis")
	.Select(path => MidiFile.Read(path))
	.ToArray();

using var outputDevice = OutputDevice.GetByName("Microsoft GS Wavetable Synth");

var mergedMidi = Merge(midis);

using var playback2 = mergedMidi.GetPlayback(outputDevice);
playback2.Play();
*/
//var midi = MidiFile.Read(@"H:\Pobrane\AE_fin_dest.mid");
var midi = MidiFile.Read(@"C:\Repos\EmotionBox\dataset\maestro-v3.0.0\2018\MIDI-Unprocessed_Chamber6_MID--AUDIO_20_R3_2018_wav--1.midi");

var trackList = midi.GetTrackChunks().ToList();
//for (int i = 2; i < trackList.Count; i++)
//	trackList[1].AddObjects(trackList[i].GetTimedEvents());

//midi.Chunks.Clear();
//midi.Chunks.Add(trackList[1]);

var setTempos = trackList
	.SelectMany(x => x.Events.OfType<SetTempoEvent>())
	.ToArray();

foreach (var setTempo in setTempos)
{
    setTempo.MicrosecondsPerQuarterNote = (long)(setTempo.MicrosecondsPerQuarterNote * 0.95);
}

var dev = OutputDevice.GetAll();
using var outputDevice = OutputDevice.GetByName("VirtualMIDISynth #1") ?? OutputDevice.GetByName("Microsoft GS Wavetable Synth");
using var playback = midi.GetPlayback(outputDevice);
playback.Play();


MidiFile Merge(IEnumerable<MidiFile> midis)
{
	var midiFileOut = new MidiFile()
	{
		TimeDivision = midis.First().TimeDivision
	};

	long addedSoFarMicroseconds = 0;
	foreach (var midi in midis)
	{
		var currentDuration = midi.GetDuration<MetricTimeSpan>();
		midi.ShiftEvents(new MetricTimeSpan(addedSoFarMicroseconds));
		midiFileOut.Chunks.AddRange(midi.Chunks);
		addedSoFarMicroseconds += currentDuration.TotalMicroseconds;
	}
	return midiFileOut;
}