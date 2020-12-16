namespace ImageRecognition
{
    public class ImageResult
    {
        public int Id { get; set; }
        public string Path { get; set; }
        public string OutputLabel { get; set; }
        public double Confidence { get; set; }
        virtual public ImageDetails Details { get; set; }
        public bool Error { get; set; }

        public ImageResult(string path, string outputLabel, double confidence, ImageDetails details)
        {
            Path = path;
            OutputLabel = outputLabel;
            Confidence = confidence;
            Details = details;
            Error = false;
        }

        public ImageResult(string path)
        {
            Path = path;
            Error = true;
        }

        public ImageResult() 
        {
            Path = "";
            OutputLabel = "";
            Confidence = 0.0;
            Error = false;
        }

        public override string ToString()
        {
            return Error ? "Error processing file " + Path + "\n" :
                    "Image path: " + Path + "\n" + OutputLabel +
                    " with confidence " + Confidence + "\n";
        }
    }
}