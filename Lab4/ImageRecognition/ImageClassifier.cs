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

namespace ImageRecognition
{
    public class ImageClassifier
    {
        private static readonly object lockObj = new object();

        public static readonly ConcurrentQueue<ImageResult> predictionOutputs
                = new ConcurrentQueue<ImageResult>();

        public static readonly CancellationTokenSource cts = new CancellationTokenSource();

        public static ImageResult processImage(string path)
        {
            ImageResult res = new ImageResult(path);
            try
            {
                using (var db = new LibraryContext())
                {
                    var query = db.Images.Where(a => a.Path.Equals(path));
                    if (query.Count() > 0)
                    {
                        foreach (var a in query)
                            lock (lockObj)
                            {
                                byte[] blob = File.ReadAllBytes(path);
                                var query2 = db.ImageBlobs.Where(a => a.Blob.Equals(blob));
                                if (query2.Count() == 0) continue;
                                res = new ImageResult(path, a.OutputLabel, a.Confidence, new ImageDetails(blob));
                                predictionOutputs.Enqueue(res);
                            }
                    }
                    else
                    {
                        using var image = Image.Load<Rgb24>(path);
                        const int TargetWidth = 224;
                        const int TargetHeight = 224;

                        image.Mutate(x =>
                        {
                            x.Resize(new ResizeOptions
                            {
                                Size = new Size(TargetWidth, TargetHeight),
                                Mode = ResizeMode.Crop
                            });
                        });

                        var input = new DenseTensor<float>(new[] { 1, 3, TargetHeight, TargetWidth });
                        var mean = new[] { 0.485f, 0.456f, 0.406f };
                        var stddev = new[] { 0.229f, 0.224f, 0.225f };
                        for (int y = 0; y < TargetHeight; y++)
                        {
                            Span<Rgb24> pixelSpan = image.GetPixelRowSpan(y);
                            for (int x = 0; x < TargetWidth; x++)
                            {
                                input[0, 0, y, x] = ((pixelSpan[x].R / 255f) - mean[0]) / stddev[0];
                                input[0, 1, y, x] = ((pixelSpan[x].G / 255f) - mean[1]) / stddev[1];
                                input[0, 2, y, x] = ((pixelSpan[x].B / 255f) - mean[2]) / stddev[2];
                            }
                        }

                        var inputs = new List<NamedOnnxValue>
                        {
                            NamedOnnxValue.CreateFromTensor("input", input)
                        };

                        using var session = new InferenceSession("../ImageRecognition/shufflenet-v2-10.onnx");
                        using IDisposableReadOnlyCollection<DisposableNamedOnnxValue> results = session.Run(inputs);

                        var output = results.First().AsEnumerable<float>().ToArray();
                        var sum = output.Sum(x => (float)Math.Exp(x));
                        var softmax = output.Select(x => (float)Math.Exp(x) / sum);
                        string[] classLabels = File.ReadAllLines("../MyClient/classLabels.txt");

                        lock (lockObj)
                        {
                            foreach (var p in softmax
                                    .Select((x, i) => new { Label = classLabels[i], Confidence = x })
                                    .OrderByDescending(x => x.Confidence)
                                    .Take(1))
                            {
                                byte[] blob = File.ReadAllBytes(path);
                                res = new ImageResult(path, p.Label, p.Confidence, new ImageDetails(blob));
                                predictionOutputs.Enqueue(res);
                                var queryClasses = db.ImageClasses.Where(a => a.ClassName == p.Label);
                                if (queryClasses.Count() > 0)
                                    foreach (var a in queryClasses)
                                        a.Count += 1;
                                else
                                {
                                    db.ImageClasses.Add(new ImageClass(p.Label));
                                }
                                db.Images.Add(new ImageResult(path, p.Label, p.Confidence, new ImageDetails(blob)));
                                db.SaveChanges();
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                res = new ImageResult(path);
                predictionOutputs.Enqueue(res);
            }
            return res;
        }



        public static async Task parallelProcess(string path, int tasksCount)
        {
            ConcurrentQueue<string> filenames =
                    new ConcurrentQueue<string>(Directory.GetFiles(path));

            Task[] tasks = new Task[tasksCount];

            for (int i = 0; i < tasksCount; ++i)
            {
                tasks[i] = Task.Run(() => {
                    while (filenames.TryDequeue(out path))
                    {
                        if (cts.Token.IsCancellationRequested)
                        {
                            return;
                        }
                        //processImage(path);
                    }
                });
            }

            await Task.WhenAll(tasks);
        }
    }
}
