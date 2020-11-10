namespace ImageRecognition
{
    public class ImageResult
    {
        public string path = "";
        public string outputLabel = "";
        public double confidence = 0.0;
        public bool error = false;

        public ImageResult(string path, string outputLabel, double confidence)
        {
            this.path = path;
            this.outputLabel = outputLabel;
            this.confidence = confidence;
            this.error = false;
        }

        public ImageResult(string path)
        {
            this.path = path;
            this.error = true;
        }

        public override string ToString()
        {
            return error ? "Error processing file " + path + "\n" :
                    "Image path: " + path + "\n" + outputLabel +
                    " with confidence " + confidence + "\n";
        }
    }
}