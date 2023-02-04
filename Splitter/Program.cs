using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Splitter;
using System.Threading.Tasks.Dataflow;


if (Data.InputDirectory == null)
    throw new ArgumentNullException(nameof(Data.InputDirectory));
if (Data.OutputDirectory == null)
    throw new ArgumentNullException(nameof(Data.OutputDirectory));

if (!Directory.Exists(Data.OutputDirectory))
{
    Directory.CreateDirectory(Data.OutputDirectory);
}
var subDirectories = Directory.GetDirectories(Data.InputDirectory, "*", SearchOption.AllDirectories).ToArray();
foreach (var subDirectory in subDirectories)
{
    var targetSubDirectory = Path.Combine(Data.OutputDirectory, Path.GetRelativePath(Data.InputDirectory, subDirectory));
    if (!Directory.Exists(targetSubDirectory))
    {
        Directory.CreateDirectory(targetSubDirectory);
    }
}

var options = new ExecutionDataflowBlockOptions { EnsureOrdered = false/*, MaxMessagesPerTask = 1, MaxDegreeOfParallelism = 20*/ };

var getPathsBlock = new TransformManyBlock<string, string>(inputDirectory => Directory.GetFiles(inputDirectory, "*.mid*", SearchOption.AllDirectories), options);
var loadBlock = new TransformManyBlock<string, MidiWithId>(Load, options);
var splitBlock = new TransformManyBlock<MidiWithId, MidiWithId>(Split, options);
var removeDrumTrackBlock = new TransformManyBlock<MidiWithId, MidiWithId>(RemoveDrumTrack, options);
var removeNonPianoTracksBlock = new TransformManyBlock<MidiWithId, MidiWithId>(RemoveNonPianoTracks, options);
var setVolumeToMaxBlock = new TransformManyBlock<MidiWithId, MidiWithId>(SetVolumeToMax, options);
var copyWithSpeedFractionBlock = new TransformManyBlock<MidiWithId, MidiWithId>(CopyWithSpeedFraction, options);
var saveBlock = new ActionBlock<MidiWithId>(midiWithId => Save(midiWithId, Data.OutputDirectory), options);

getPathsBlock.LinkTo(loadBlock, new DataflowLinkOptions { PropagateCompletion = true });
loadBlock.LinkTo(splitBlock, new DataflowLinkOptions { PropagateCompletion = true });
splitBlock.LinkTo(removeDrumTrackBlock, new DataflowLinkOptions { PropagateCompletion = true });
removeDrumTrackBlock.LinkTo(removeNonPianoTracksBlock, new DataflowLinkOptions { PropagateCompletion = true });
removeNonPianoTracksBlock.LinkTo(setVolumeToMaxBlock, new DataflowLinkOptions { PropagateCompletion = true });
setVolumeToMaxBlock.LinkTo(copyWithSpeedFractionBlock, new DataflowLinkOptions { PropagateCompletion = true });
copyWithSpeedFractionBlock.LinkTo(saveBlock, new DataflowLinkOptions { PropagateCompletion = true });

var streamwriter = new StreamWriter(new FileStream("logs.txt", FileMode.Create))
{
    AutoFlush = true
};
Console.SetOut(streamwriter);

Console.WriteLine($"Starting processing");

getPathsBlock.Post(Data.InputDirectory);

getPathsBlock.Complete();

await saveBlock.Completion;

Console.WriteLine($"Ended processing");

static IEnumerable<MidiWithId> RemoveNonPianoTracks(MidiWithId midiWithId)
{
    try
    {
        var chunks = midiWithId.Midi.GetTrackChunks().ToList();
        foreach (var chunk in chunks)
        {
            var programChanges = chunk.Events.OfType<ProgramChangeEvent>();
            if (programChanges.Any(e => e.ProgramNumber > 7))
            {
                if (!midiWithId.Midi.Chunks.Remove(chunk))
                    throw new Exception($"Chunk {chunk.ChunkId} not found");
            }
        }

        var duration = midiWithId.Midi.GetDuration<MetricTimeSpan>();

        if (!midiWithId.Midi.Chunks.Any() || duration.TotalSeconds < 15.0)
            return Enumerable.Empty<MidiWithId>();

        var tempoMap = midiWithId.Midi.GetTempoMap();


        var noteOnTimes = midiWithId.Midi.GetTimedEvents()
                .Where(e => e.Event.EventType == MidiEventType.NoteOn)
                .Select(e => e.TimeAs<MetricTimeSpan>(tempoMap))
                .Distinct()
                .Append(duration)
                .OrderBy(t => t.TotalMicroseconds);

        var lastNoteOnTime = 0.0;
        foreach (var noteOnTime in noteOnTimes)
        {
            if (noteOnTime.TotalSeconds - lastNoteOnTime > 5.0)
            {
                return Enumerable.Empty<MidiWithId>();
            }
            lastNoteOnTime = noteOnTime.TotalSeconds;
        }

        return new[] { midiWithId };
    }
    catch (Exception e)
    {
        Console.WriteLine($"Exception when removing non piano tracks for {midiWithId.Id}: {e}");
        return Enumerable.Empty<MidiWithId>();
    }
}

static IEnumerable<MidiWithId> RemoveDrumTrack(MidiWithId midiWithId)
{
    try
    {
        midiWithId.Midi.RemoveNotes(new Predicate<Note>(note => note.Channel == new FourBitNumber(9)));
        return new[] { midiWithId };
    }
    catch (Exception e)
    {
        Console.WriteLine($"Exception when removing drums for {midiWithId.Id}: {e}");
        return Enumerable.Empty<MidiWithId>();
    }
}

static IEnumerable<MidiWithId> SetVolumeToMax(MidiWithId midiWithId)
{
    try
    {
        midiWithId.Midi.SetVolumeTo(127);
        return new[] { midiWithId };
    }
    catch (Exception e)
    {
        Console.WriteLine($"Exception when setting volume for {midiWithId.Id}: {e}");
        return Enumerable.Empty<MidiWithId>();
    }
}

static IEnumerable<MidiWithId> CopyWithSpeedFraction(MidiWithId midiWithId)
{
    try
    {
        return midiWithId.Midi.CopyWithSpeedFraction(0.95, 1.05)
            .Select((midi, i) => new MidiWithId(midi, midiWithId.Directory, $"{midiWithId.Id}-speed_{i:00}"));
    }
    catch (Exception e)
    {
        Console.WriteLine($"Exception when copying with speed fraction for {midiWithId.Id}: {e}");
        return Enumerable.Empty<MidiWithId>();
    }
}

static IEnumerable<MidiWithId> Split(MidiWithId midiWithId)
{
    try
    {
        return midiWithId.Midi.SplitByTime(TimeSpan.FromSeconds(30))
            .Where(m => m.GetDuration<MetricTimeSpan>().Seconds >= 15)
            .Select((midi, i) => new MidiWithId(midi, midiWithId.Directory, $"{midiWithId.Id}-timesplit_{i:00}"));
    }
    catch (Exception e)
    {
        Console.WriteLine($"Exception when splitting for {midiWithId.Id}: {e}");
        return Enumerable.Empty<MidiWithId>();
    }
}

static IEnumerable<MidiWithId> Load(string path)
{
    try
    {
        var midi = MidiFile.Read(path);
        var midiDuration = midi.GetDuration<MetricTimeSpan>();

        if (midiDuration.TotalHours > 1)
            throw new Exception($"MIDI is {midiDuration.TotalHours} hours long!");

        return new[] {
            new MidiWithId(
                MidiFile.Read(path),
                Path.GetDirectoryName(Path.GetRelativePath(Data.InputDirectory, path)),
                Path.GetFileNameWithoutExtension(path)
                )
        };
    }
    catch (Exception e)
    {
        Console.WriteLine($"Exception when loading from path {path}: {e}");
        return Enumerable.Empty<MidiWithId>();
    }
}

static void Save(MidiWithId midiWithId, string outputDirectory)
{
    midiWithId.Midi.Write(Path.Combine(new string[] { outputDirectory, midiWithId.Directory, $"{midiWithId.Id}.midi" }.Where(x => x != null).ToArray()));
}


class MidiWithId
{
    public MidiFile Midi { get; private set; }
    public string? Directory { get; private set; }
    public string Id { get; private set; }

    public MidiWithId(MidiFile midi, string? directory, string id)
    {
        Midi = midi;
        Directory = directory;
        Id = id;
    }
}
