using System.Windows;
using System.Windows.Forms;
using System.Collections.Generic;
using ImageRecognition;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System;

namespace MyClient
{
    public partial class MainWindow : Window
    {
        public ObservableCollection<Pair> ClassesCounts { get; set; }

        public Dictionary<string, ObservableCollection<string>> ImagesInClass { get; set; }

        private static readonly string url = "https://localhost:5001/images";

        HttpClient client = new HttpClient();

        public MainWindow()
        {
            ClassesCounts = new ObservableCollection<Pair>();
            ImagesInClass = new Dictionary<string, ObservableCollection<string>>();
            DataContext = ClassesCounts;
            InitializeComponent();
        }

        private async void Post()
        {
            string path = (string)labelPath.Content;
            if (path is null) path = "../../../images";
            var content = new StringContent(JsonConvert.SerializeObject(path), Encoding.UTF8, "application/json");
            HttpResponseMessage httpResponse;
            try
            {
                httpResponse = await client.PostAsync(url, content, ImageClassifier.cts.Token);
            }
            catch (HttpRequestException)
            {
                await Dispatcher.BeginInvoke(new Action(() =>
                {
                    System.Windows.Forms.MessageBox.Show("No connection");
                }));
                return;
            }

            if (httpResponse.IsSuccessStatusCode)
            {
                var results = JsonConvert.DeserializeObject<List<ImageResult>>(httpResponse.Content.ReadAsStringAsync().Result);
                foreach (var a in results)
                {
                    Pair replacing = ClassesCounts.FirstOrDefault(x => x.ClassLabel == a.OutputLabel);
                    if (replacing != null)
                    {
                        var index = ClassesCounts.IndexOf(replacing);
                        ClassesCounts[index].Count += 1;
                    }
                    else
                        ClassesCounts.Add(new Pair(a.OutputLabel, 1));

                    ObservableCollection<string> localImages = null;
                    if (!ImagesInClass.TryGetValue(a.OutputLabel, out localImages))
                    {
                        localImages = new ObservableCollection<string>();
                        ImagesInClass[a.OutputLabel] = localImages;
                    }
                    string localPath = a.Path.Substring(a.Path.LastIndexOf('\\') + 1);
                    ImagesInClass[a.OutputLabel].Add(localPath);
                }
            }
            labelWait.Content = "";
        }

        private void buttonStart_Click(object sender, RoutedEventArgs e)
        {
            labelWait.Content = "Please wait...";
            Post();
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

        private void buttonClear_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                client.DeleteAsync(url);
                listBoxStats.ItemsSource = null;
            }
            catch (AggregateException)
            {
                System.Windows.Forms.MessageBox.Show("No connection");
            }
        }

        private void buttonStats_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var httpResponse = client.GetAsync(url).Result;
                var stats = JsonConvert.DeserializeObject<List<ImageClass>>(httpResponse.Content.ReadAsStringAsync().Result);
                listBoxStats.ItemsSource = stats;
            }
            catch (AggregateException)
            {
                System.Windows.Forms.MessageBox.Show("No connection");
            }
        }
    }
}
