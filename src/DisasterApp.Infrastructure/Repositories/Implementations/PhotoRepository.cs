using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Infrastructure.Repositories
{
    public class PhotoRepository : IPhotoRepository
    {
        private readonly DisasterDbContext _context;

        public PhotoRepository(DisasterDbContext context)
        {
            _context = context;
        }

        public async Task<Photo> AddAsync(Photo photo)
        {
            await _context.Photos.AddAsync(photo);
            return photo;
        }
        public async Task<Photo?> GetByIdAsync(int id)
        {
            return await _context.Photos.FindAsync(id);
        }

        public void Delete(Photo photo)
        {
            _context.Photos.Remove(photo);
        }




        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
