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
    public class ImpactTypeService : IImpactTypeService
    {
        private readonly IImpactTypeRepository _impactTypeRepository;
        public ImpactTypeService(IImpactTypeRepository impactTypeRepository)
        {
            _impactTypeRepository = impactTypeRepository;
        }
        public async Task<List<ImpactTypeDto>> GetAllAsync()
        {
            var impactTypes = await _impactTypeRepository.GetAllAsync();
            return impactTypes.Select(it => new ImpactTypeDto
            {
                Id = it.Id,
                Name = it.Name
            }).ToList();
        }
        public async Task<ImpactTypeDto?> GetByIdAsync(int id)
        {
            var impactType = await _impactTypeRepository.GetByIdAsync(id);
            if (impactType == null) return null;
            return new ImpactTypeDto
            {
                Id = impactType.Id,
                Name = impactType.Name
            };
        }
        public async Task<ImpactTypeDto> CreateAsync(ImpactTypeCreateDto createDto)
        {
            var impactType = new ImpactType
            {
                Name = createDto.Name
            };
            var createdImpactType = await _impactTypeRepository.AddAsync(impactType);
            return new ImpactTypeDto
            {
                Id = createdImpactType.Id,
                Name = createdImpactType.Name
            };
        }
    }

}
