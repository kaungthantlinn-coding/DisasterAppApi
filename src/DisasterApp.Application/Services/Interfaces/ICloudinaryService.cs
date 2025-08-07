using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.Services
{
    public interface ICloudinaryService
    {
        Task<(string Url, string PublicId)> UploadImageAsync(IFormFile file);
        Task DeleteImageAsync(string publicId);

    }
}
