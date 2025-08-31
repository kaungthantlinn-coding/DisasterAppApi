using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.Services.Interfaces
{
    public interface IFileStorageService
    {
        Task<string> SaveAsync(IFormFile file, string rootPath, string subFolder = "uploads");
        Task DeleteAsync(string rootPath, string relativeUrl);
    }
}
