# musiclivegeneratorcsharp
Tools created while working on the master's thesis: ***Automatic music generation methods for video games***



## ***MusicInterface*** - library in .NET Standard 2.0

Contains methods to communicate with Python WebSocket server serving RNN model for MIDI music generation.
Example use can be seen in *MidiPlayerWpf* project.



## ***MidiPlayerWpf*** - test aplication for communication with Python model

Connects to WebSocket server, requests next music segments with entered data, plays them smoothly one by one.



## ***Splitter*** - first data preparation step

Removes non piano tracks, sets volume to max value for all notes, splits MIDIS to 30s segments, copies and speeds them up and down by 5%, saves new output.

InputDirectory and OutputDirectory should be defined in *Data.cs*.

Logs are written to *logs.txt* located in *Splitter.exe* directory.
