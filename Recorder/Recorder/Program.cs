using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Recorder
{
    class Program
    {
        private static Process audio = new Process();
        private static Process video = new Process();
        private static Process merge = new Process();

        static void Main(string[] args)
        {
            String microphone = "Microphone Array (Realtek High Definition Audio(SST))";
            String audioCommand = "-y -f dshow -i audio=\"" + microphone + "\" audio.wav";
            String videoCommand = "-y -f gdigrab -framerate 10 -i desktop video.avi";
            bool wait = true;

            Task audioTask = new Task(() =>
            {
                Console.WriteLine("In audio thread");
                ExecuteCommand(audioCommand, audio);
                while (wait) { }
                // stop
                audio.Kill();
                Console.WriteLine("Closed audio process");
            });

            Task videoTask = new Task(() =>
            {
                Console.WriteLine("In Video thread");
                ExecuteCommand(videoCommand, video);
                while (wait) { }
                // stop
                video.Kill();
                Console.WriteLine("Closed video process");
            });

            Task waitThread = new Task(() =>
            {
                Console.WriteLine("Waiting");
                Thread.Sleep(15000);
                Console.WriteLine("Done waiting");
                wait = false;
            });

            audioTask.Start();
            videoTask.Start();
            waitThread.Start();

            waitThread.Wait();
            audioTask.Wait();
            videoTask.Wait();

            Splice();
        }

        static void ExecuteCommand(String command, Process process)
        {
            Console.WriteLine(command);
            process.StartInfo.FileName = @"C:\ffmpeg\bin\ffmpeg.exe";
            process.StartInfo.Arguments = command;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = false;
            process.Start();
        }

        static void Splice()
        {
            string mergeCommand = "-y -i video.avi -i audio.wav -c:v copy -c:a aac output.avi";
            ExecuteCommand(mergeCommand, merge);
            merge.WaitForExit();
            Console.WriteLine("Here?");
        }
    }
}
