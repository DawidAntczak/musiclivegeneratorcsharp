// See https://aka.ms/new-console-template for more information
using NAudio.Wave;
using Strangetimez;

var soundPlayer = new SoundPlayer(@"C:\Repos\EmotionBox\output\2022-10-15 00-03-43\output-0happy.mid", 95);

soundPlayer.Play();

Console.ReadKey();