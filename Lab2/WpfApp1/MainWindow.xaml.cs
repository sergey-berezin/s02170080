using System;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using ImageRecognition;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Collections.Specialized;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.VisualStyles;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    public class Pair : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public string ClassLabel { get; set; }
        private int count;
        public int Count 
        { 
            get { return count; } 
            set 
            { 
                count = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Count"));
            } 
        }
        public Pair(string s, int i)
        {
            ClassLabel = s;
            Count = i;
        }
        public override string ToString()
        {
            return ClassLabel + " " + Count.ToString();
        }
    }

    public partial class MainWindow : Window
    {
        public ObservableCollection<Pair> ClassesCounts { get; set; }

        public Dictionary<string, ObservableCollection<string>> ImagesInClass { get; set; }

        public MainWindow()
        {
            ClassesCounts = new ObservableCollection<Pair>();
            ImagesInClass = new Dictionary<string, ObservableCollection<string>>();
            DataContext = ClassesCounts;
            InitializeComponent();
        }

        private async Task Processing()
        {
            string path = (string) labelPath.Content;
            if (path is null) path = "..\\..\\..\\images";
            int tasksCount = 4;
            bool done = false;

            Task extractResults = Task.Run(() => {
                ImageResult predictionOutput;
                while (true)
                {
                    Thread.Sleep(0);
                    if (ImageClassifier.cts.Token.IsCancellationRequested || done)
                    {
                        return;
                    }
                    if (ImageClassifier.predictionOutputs.TryDequeue(out predictionOutput))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            
                            Pair replacing = ClassesCounts.FirstOrDefault(x => x.ClassLabel == predictionOutput.outputLabel);
                            if (replacing != null)
                            {
                                var index = ClassesCounts.IndexOf(replacing);
                                ClassesCounts[index].Count += 1;
                            }
                            else
                                ClassesCounts.Add(new Pair(predictionOutput.outputLabel, 1));

                            ObservableCollection<string> localImages = null;
                            if (!ImagesInClass.TryGetValue(predictionOutput.outputLabel, out localImages))
                            {
                                localImages = new ObservableCollection<string>();
                                ImagesInClass[predictionOutput.outputLabel] = localImages;
                            }
                            string localPath = predictionOutput.path.Substring(predictionOutput.path.LastIndexOf('\\') + 1);
                            ImagesInClass[predictionOutput.outputLabel].Add(localPath);
                        });
                    }
                }
            });

            await ImageClassifier.parallelProcess(path, tasksCount);
            if (!ImageClassifier.cts.IsCancellationRequested)
            {
                done = true;
            }

            await extractResults;
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            Processing();
        }

        private void buttonStop_Click(object sender, RoutedEventArgs e)
        {
            ImageClassifier.cts.Cancel();
        }

        private void buttonFolder_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                labelPath.Content = fbd.SelectedPath;
            }
        }

        private void listBoxClasses_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string label = ((Pair)listBoxClasses.SelectedItem).ClassLabel;
            ObservableCollection<string> localImages = ImagesInClass[label];

            System.Windows.Data.Binding b = new System.Windows.Data.Binding();
            b.Source = localImages;
            listBoxImages.SetBinding(ItemsControl.ItemsSourceProperty, b);
        }
    }
}
