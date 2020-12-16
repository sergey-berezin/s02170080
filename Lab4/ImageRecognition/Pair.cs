using System.ComponentModel;

namespace ImageRecognition
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
}
