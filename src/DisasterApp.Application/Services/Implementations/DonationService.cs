using DisasterApp.Application.DTOs;
using DisasterApp.Application.Services.Interfaces;
using DisasterApp.Domain.Entities;
using DisasterApp.Domain.Enums;
using DisasterApp.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.Services.Implementations
{
    public class DonationService : IDonationService
    {
        private const int MainOrganizationId = 3;
        private readonly IDonationRepository _donationRepository;

        public DonationService(IDonationRepository donationRepository)
        {
            _donationRepository = donationRepository;
        }

        public async Task<int> CreateDonationAsync(Guid userId, CreateDonationDto dto)
        {
            var donation = new Donation
            {
                UserId = userId,
                OrganizationId = MainOrganizationId,
                DonorName = dto.DonorName,
                DonorContact = dto.DonorContact,
                DonationType = dto.DonationType,
                Amount = dto.Amount,
                Description = dto.Description,
                ReceivedAt = DateTime.UtcNow,
                Status = DonationStatus.Pending
            };

            await _donationRepository.AddAsync(donation);
            return donation.Id;
        }

        public async Task<bool> VerifyDonationAsync(int donationId, Guid adminUserId, VerifyDonationDto dto)
        {
            //var donation = await _donationRepository.GetByIdAsync(donationId);
            //if (donation == null) return false;

            //donation.Status = DonationStatus.Verified;
            //donation.VerifiedAt = DateTime.UtcNow;
            //donation.VerifiedBy = adminUserId;
            //donation.TransactionPhotoUrl = dto.TransactionPhotoUrl;

            //await _donationRepository.UpdateAsync(donation);
            //return true;

            var donation = await _donationRepository.GetByIdAsync(donationId);
            if (donation == null) return false;

            // Convert file to Base64
            string base64Photo = null!;
            if (dto.TransactionPhoto != null)
            {
                using var ms = new MemoryStream();
                await dto.TransactionPhoto.CopyToAsync(ms);
                var bytes = ms.ToArray();
                base64Photo = Convert.ToBase64String(bytes);
            }

            donation.Status = DonationStatus.Verified;
            donation.VerifiedAt = DateTime.UtcNow;
            donation.VerifiedBy = adminUserId;
            donation.TransactionPhotoUrl = base64Photo;

            await _donationRepository.UpdateAsync(donation);
            return true;




        }

        public async Task<List<DonationDto>> GetDonationsByOrganizationIdAsync(int organizationId)
        {
            var donations = await _donationRepository.GetByOrganizationIdAsync(organizationId);

            return donations
                .Where(d => d.Status == DonationStatus.Verified)
                .Select(d => new DonationDto
                {
                    Id = d.Id,
                    DonorName = d.DonorName,
                    DonorContact = d.DonorContact,
                    DonationType = d.DonationType.ToString(),
                    Amount = d.Amount,
                    Description = d.Description,
                    ReceivedAt = d.ReceivedAt,
                    Status = d.Status.ToString(),
                    TransactionPhotoUrl = d.TransactionPhotoUrl
                })
                .ToList();
        }

        public Task<Donation?> GetDonationByIdAsync(int donationId) =>
            _donationRepository.GetByIdAsync(donationId);

        public async Task<List<DonationDto>> GetPendingDonationsAsync()
        {
            var donations = await _donationRepository.GetPendingDonationsAsync();
            return donations.Select(d => new DonationDto
            {
                Id = d.Id,
                DonorName = d.DonorName,
                DonorContact = d.DonorContact,
                DonationType = d.DonationType.ToString(),
                Amount = d.Amount,
                Description = d.Description,
                ReceivedAt = d.ReceivedAt,
                Status = d.Status.ToString(),
                TransactionPhotoUrl = d.TransactionPhotoUrl
            }).ToList();
        }
    }
}
