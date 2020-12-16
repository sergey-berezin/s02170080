using Microsoft.EntityFrameworkCore;

namespace ImageRecognition
{
    public class ImageClass
    {
        public int Id { get; set; }
        public string ClassName { get; set; }
        public int Count { get; set; }

        public ImageClass()
        {
            ClassName = "";
            Count = 0;
        }

        public ImageClass(string className)
        {
            ClassName = className;
            Count = 1;
        }

        public override string ToString()
        {
            return $"{ClassName} {Count}";
        }
    }

    public class ImageDetails
    {
        public int Id { get; set; }
        public byte[] Blob { get; set; }

        public ImageDetails(byte[] blob) 
        {
            Blob = blob;
        }
    }

    public class LibraryContext : DbContext
    {
        public DbSet<ImageResult> Images { get; set; }
        public DbSet<ImageClass> ImageClasses { get; set; }
        public DbSet<ImageDetails> ImageBlobs { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder o)
            => o.UseSqlite("Data Source=../ImageRecognition/ImageDataBase.db");
    }
}