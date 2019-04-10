using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Catalog.API.Services.ImageWriter
{
    public interface IImageHandler
    {
        Task<IActionResult> UploadImage(IFormFile file, string pictureFileName);
    }

    public class ImageHandler : IImageHandler
    {
        private readonly IImageWriter _imageWriter;
        public ImageHandler(IImageWriter imageWriter)
        {
            _imageWriter = imageWriter;
        }

        public async Task<IActionResult> UploadImage(IFormFile file, string pictureFileName)
        {
            var result = await _imageWriter.UploadImage(file, pictureFileName);
            System.Console.WriteLine("Writing " + file.Name);
            return new ObjectResult(result);
        }
    }
}