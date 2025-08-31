using DisasterApp.Application.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.Services.Implementations
{
    public class FileStorageService : IFileStorageService
    {
        public async Task<string> SaveAsync(IFormFile file, string rootPath, string subFolder = "uploads")
        {
            var folderPath = Path.Combine(rootPath, subFolder);
            Directory.CreateDirectory(folderPath);

            var extension = Path.GetExtension(file.FileName);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/{subFolder}/{fileName}".Replace("\\", "/");
        }

        public Task DeleteAsync(string rootPath, string relativeUrl)
        {
            if (string.IsNullOrEmpty(relativeUrl)) return Task.CompletedTask;

            var path = Path.Combine(rootPath, relativeUrl.TrimStart('/').Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (File.Exists(path)) File.Delete(path);

            return Task.CompletedTask;
        }
    }
}
