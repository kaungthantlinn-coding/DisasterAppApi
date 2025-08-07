using DisasterApp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Infrastructure.Repositories
{
    public interface IPhotoRepository
    {
        Task<Photo> AddAsync(Photo photo);
        Task<Photo?> GetByIdAsync(int id);
        void Delete(Photo photo);

        Task SaveChangesAsync();

    }
}
