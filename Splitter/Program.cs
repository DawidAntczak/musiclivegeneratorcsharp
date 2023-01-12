using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Splitter;
using System.Threading.Tasks.Dataflow;


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

var opts = new ExecutionDataflowBlockOptions { BoundedCapacity = 15, MaxMessagesPerTask = 1 };

var getPathsBlock = new TransformManyBlock<string, string>(inputDirectory => Directory.GetFiles(inputDirectory, "*.mid*", SearchOption.AllDirectories), opts);
var loadBlock = new TransformManyBlock<string, MidiWithId>(Load, opts);
var splitBlock = new TransformManyBlock<MidiWithId, MidiWithId>(Split, opts);
var removeNonPianoTracksBlock = new TransformManyBlock<MidiWithId, MidiWithId>(RemoveNonPianoTracks, opts);
var removeDrumTrackBlock = new TransformManyBlock<MidiWithId, MidiWithId>(RemoveDrumTrack, opts);
var setVolumeToMaxBlock = new TransformManyBlock<MidiWithId, MidiWithId>(SetVolumeToMax, opts);
var copyWithSpeedFractionBlock = new TransformManyBlock<MidiWithId, MidiWithId>(CopyWithSpeedFraction, opts);
var saveBlock = new ActionBlock<MidiWithId>(midiWithId => Save(midiWithId, Data.OutputDirectory), opts);

getPathsBlock.LinkTo(loadBlock, new DataflowLinkOptions { PropagateCompletion = true });
loadBlock.LinkTo(splitBlock, new DataflowLinkOptions { PropagateCompletion = true });
splitBlock.LinkTo(removeNonPianoTracksBlock, new DataflowLinkOptions { PropagateCompletion = true });
removeNonPianoTracksBlock.LinkTo(removeDrumTrackBlock, new DataflowLinkOptions { PropagateCompletion = true });
removeDrumTrackBlock.LinkTo(setVolumeToMaxBlock, new DataflowLinkOptions { PropagateCompletion = true });
setVolumeToMaxBlock.LinkTo(copyWithSpeedFractionBlock, new DataflowLinkOptions { PropagateCompletion = true });
copyWithSpeedFractionBlock.LinkTo(saveBlock, new DataflowLinkOptions { PropagateCompletion = true });

getPathsBlock.Post(Data.InputDirectory);

getPathsBlock.Complete();

await saveBlock.Completion;

static IEnumerable<MidiWithId> RemoveNonPianoTracks(MidiWithId midiWithId)
{
    try
    {
        var tracks = midiWithId.Midi.GetTrackChunks().ToList();
        foreach (var track in tracks)
        {
            var programChanges = track.Events.OfType<ProgramChangeEvent>();
            if (programChanges.Any(e => e.ProgramNumber > 7))
            {
                midiWithId.Midi.Chunks.Remove(track);
            }
        }
        return midiWithId.Midi.Chunks.Any() && midiWithId.Midi.GetTrackChunks().Any(x => x.Events.OfType<NoteOnEvent>().Any()) 
            ? new[] { midiWithId }
            : Enumerable.Empty<MidiWithId>();
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
        //midiWithId.Midi.SetVolumeTo(127);
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
        return new[] { new MidiWithId(MidiFile.Read(path), Path.GetDirectoryName(Path.GetRelativePath(Data.InputDirectory, path)), Path.GetFileNameWithoutExtension(path)) };
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
