using Microsoft.AspNetCore.Mvc;
using ImageRecognition;
using System.Collections.Generic;
using System.Linq;


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

        [HttpGet("{className}")]
        public List<ImageDetails> GetImage(string className)
        {
            using var db = new LibraryContext();
            var imagesInClass = db.Images.Where(a => a.OutputLabel == className);
            List<ImageDetails> result = new List<ImageDetails>();
            foreach (var image in imagesInClass)
            {
                result.AddRange(db.ImageBlobs.Where(a => a.Id == image.Id));
            }
            return result;
        }
    }
}
