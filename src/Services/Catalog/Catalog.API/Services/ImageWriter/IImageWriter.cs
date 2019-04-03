using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Catalog.API.Services.ImageWriter
{
    public interface IImageWriter
    {
        Task<string> UploadImage(IFormFile file);
    }
}