using DisasterApp.Application.DTOs;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.Services
{
    public class PhotoService : IPhotoService
    {
        private readonly ICloudinaryService _cloudinary;
        private readonly IPhotoRepository _photoRepository;

        public PhotoService(ICloudinaryService cloudinary, IPhotoRepository photoRepository)
        {
            _cloudinary = cloudinary;
            _photoRepository = photoRepository;
        }

        public async Task<Photo> UploadPhotoAsync(CreatePhotoDto dto)
        {
            var (url, publicId) = await _cloudinary.UploadImageAsync(dto.File);

            var photo = new Photo
            {
                ReportId = dto.ReportId,
                Url = url,
                PublicId = publicId,
                Caption = dto.Caption,
                UploadedAt = DateTime.UtcNow
            };

            await _photoRepository.AddAsync(photo);
            await _photoRepository.SaveChangesAsync();

            return photo;
        }
        public async Task<Photo> UpdatePhotoAsync(UpdatePhotoDto dto)
        {
            var existingPhoto = await _photoRepository.GetByIdAsync(dto.Id);
            if (existingPhoto == null)
                throw new Exception("Photo not found");

            if (dto.File != null)
            {
                if (!string.IsNullOrEmpty(existingPhoto.PublicId))
                    await _cloudinary.DeleteImageAsync(existingPhoto.PublicId);

                var (url, publicId) = await _cloudinary.UploadImageAsync(dto.File);
                existingPhoto.Url = url;
                existingPhoto.PublicId = publicId;
            }

            existingPhoto.Caption = dto.Caption ?? existingPhoto.Caption;
            existingPhoto.UploadedAt = DateTime.UtcNow;

            await _photoRepository.SaveChangesAsync();
            return existingPhoto;
        }

        public async Task DeletePhotoAsync(int photoId)
        {
            var existingPhoto = await _photoRepository.GetByIdAsync(photoId);
            if (existingPhoto == null)
                throw new Exception("Photo not found");

            if (!string.IsNullOrEmpty(existingPhoto.PublicId))
                await _cloudinary.DeleteImageAsync(existingPhoto.PublicId);

            _photoRepository.Delete(existingPhoto);
            await _photoRepository.SaveChangesAsync();
        }

    }
}
