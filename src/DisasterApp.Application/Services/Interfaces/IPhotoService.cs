using DisasterApp.Application.DTOs;
using DisasterApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.Services
{
    public interface IPhotoService
    {
        Task<Photo> UploadPhotoAsync(CreatePhotoDto dto);
        Task<Photo> UpdatePhotoAsync(UpdatePhotoDto dto);
        Task DeletePhotoAsync(int photoId);

    }
}
