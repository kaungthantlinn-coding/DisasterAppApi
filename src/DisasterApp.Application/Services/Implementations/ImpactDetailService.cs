using DisasterApp.Application.DTOs;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace DisasterApp.Application.Services
{
    public class ImpactDetailService : IImpactDetailService
    {
        private readonly IImpactDetailRepository _impactDetailRepository;
        private readonly IImpactTypeRepository _impactTypeRepository;
        public ImpactDetailService(IImpactDetailRepository impactDetailRepository, IImpactTypeRepository impactTypeRepository)
        {
            _impactDetailRepository = impactDetailRepository;
            _impactTypeRepository = impactTypeRepository;
        }
        public async Task<IEnumerable<ImpactDetailDto>> GetAllAsync()
        {
            var impactDetails = await _impactDetailRepository.GetAllAsync();
            return impactDetails.Select(id => new ImpactDetailDto
            {
                Id = id.Id,
                Description = id.Description,
                Severity = id.Severity,
                IsResolved = id.IsResolved,
                ResolvedAt = id.ResolvedAt,
                ImpactTypes = id.ImpactTypes.Select(it => new ImpactTypeDto
                {
                    Id = it.Id,
                    Name = it.Name
                }).ToList()

            });
        }
        public async Task<ImpactDetailDto?> GetByIdAsync(int id)
        {
            var impactDetail = await _impactDetailRepository.GetByIdAsync(id);
            if (impactDetail == null) return null;
            return new ImpactDetailDto
            {
                Id = impactDetail.Id,
                Description = impactDetail.Description,
                Severity = impactDetail.Severity,
                IsResolved = impactDetail.IsResolved,
                ResolvedAt = impactDetail.ResolvedAt,
                ImpactTypes = impactDetail.ImpactTypes.Select(it => new ImpactTypeDto
                {
                    Id = it.Id,
                    Name = it.Name
                }).ToList()
            };
        }
        public async Task<ImpactDetailDto> CreateAsync(ImpactDetailCreateDto impactDetail)
        {
            var alImpactTypes = await _impactTypeRepository.GetAllAsync();
            var validImpactTypes = alImpactTypes.Where(it => impactDetail.ImpactTypeIds.Contains(it.Id)).ToList();

            if (validImpactTypes.Count != impactDetail.ImpactTypeIds.Count)
                throw new Exception("Some ImpactTypeIds are invalid");

            var entity = new ImpactDetail
            {
                Description = impactDetail.Description,
                Severity = impactDetail.Severity,
                ImpactTypes = validImpactTypes
            };

            var createdEntity = await _impactDetailRepository.CreateAsync(entity);
            return new ImpactDetailDto
            {
                Id = createdEntity.Id,
                Description = createdEntity.Description,
                Severity = createdEntity.Severity,
                IsResolved = createdEntity.IsResolved,
                ResolvedAt = createdEntity.ResolvedAt,
                ImpactTypes = createdEntity.ImpactTypes.Select(it => new ImpactTypeDto
                {
                    Id = it.Id,
                    Name = it.Name
                }).ToList()
            };
        }
        public async Task<ImpactDetailDto> UpdateAsync(int id, ImpactDetailUpdateDto dto)
        {
            var existing = await _impactDetailRepository.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("ImpactDetail not found");

            existing.Description = dto.Description ?? existing.Description;
            existing.Severity = dto.Severity ?? existing.Severity;
            existing.IsResolved = dto.IsResolved ?? existing.IsResolved;
            existing.ResolvedAt = dto.ResolvedAt ?? existing.ResolvedAt;

            if (dto.ImpactTypeIds != null)
            {
                var allImpactTypes = await _impactTypeRepository.GetAllAsync();
                var validImpactTypes = allImpactTypes.Where(it => dto.ImpactTypeIds.Contains(it.Id)).ToList();

                if (validImpactTypes.Count != dto.ImpactTypeIds.Count)
                    throw new Exception("Some ImpactTypeIds are invalid");

                existing.ImpactTypes.Clear();
                foreach (var it in validImpactTypes)
                    existing.ImpactTypes.Add(it);
            }

            var updated = await _impactDetailRepository.UpdateAsync(existing);

            return new ImpactDetailDto
            {
                Id = updated.Id,
                Description = updated.Description,
                Severity = updated.Severity,
                IsResolved = updated.IsResolved,
                ResolvedAt = updated.ResolvedAt,
                ImpactTypes = updated.ImpactTypes.Select(it => new ImpactTypeDto
                {
                    Id = it.Id,
                    Name = it.Name
                }).ToList()
            };
        }
        public async Task DeleteAsync(int id)
        {
            var existing = await _impactDetailRepository.GetByIdAsync(id);
            if (existing == null)
                throw new Exception("ImpactDetail not found");

            await _impactDetailRepository.DeleteAsync(id);
        }
    }
}
