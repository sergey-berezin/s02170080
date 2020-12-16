using Microsoft.AspNetCore.Mvc;
using ImageRecognition;
using System.Collections.Generic;
using System.Linq;
using System.IO;


namespace MyServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImagesController : ControllerBase
    {
        [HttpGet]
        public List<ImageClass> Get()
        {
            using var db = new LibraryContext();
            return db.ImageClasses.Where(a => a.Count > 0).ToList();
        }

        [HttpPost]
        public List<ImageResult> Post(string path)
        {
            path = "../MyClient/images";
            List<ImageResult> results = new List<ImageResult>();
            foreach (var imagePath in Directory.GetFiles(path))
            {
                results.Add(ImageClassifier.processImage(imagePath));
            }
            return results;
        }

        [HttpDelete]
        public void Delete()
        {
            using (var db = new LibraryContext())
            {
                db.Images.RemoveRange(db.Images);
                db.ImageClasses.RemoveRange(db.ImageClasses);
                db.ImageBlobs.RemoveRange(db.ImageBlobs);
                db.SaveChanges();
            }
        }

    }

}
