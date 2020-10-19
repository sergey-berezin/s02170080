using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Linq;
using SixLabors.ImageSharp.Processing;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using ImageRecognition;

namespace Lab1
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            string path = "images";
            int tasksCount = 4;
            bool done = false;
            if (args.Length > 0) path = args[0];
            if (args.Length > 1) tasksCount = Int32.Parse(args[1]);

            Stopwatch watch = new Stopwatch();
            watch.Start();

            Task keyboardTask = Task.Run(() => {
                if (Console.ReadKey().Key == ConsoleKey.Enter) 
                {
                    ImageClassifier.cts.Cancel();
                    Console.WriteLine("Canceled");
                }
            });

            Task extractResults = Task.Run(() => {
                ImageResult predictionOutput;
                while (true) 
                {
                    if (ImageClassifier.cts.Token.IsCancellationRequested || done) 
                    {
                        return;
                    }
                    if (ImageClassifier.predictionOutputs.TryDequeue(out predictionOutput))
                    {
                        Console.WriteLine(predictionOutput);
                    }
                }
            });
            
            await ImageClassifier.parallelProcess(path, tasksCount);
            if (!ImageClassifier.cts.IsCancellationRequested)
            {
                Console.WriteLine("Done");
                done = true;
            }

            await extractResults;

            watch.Stop();
            Console.WriteLine($"{watch.ElapsedMilliseconds} elapsed milliseconds");
        }
    }
}
