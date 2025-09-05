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
        private const int MainOrganizationId = 6;
        private readonly IDonationRepository _donationRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly string _wwwRootPath;

        public DonationService(
            IDonationRepository donationRepository,
            IFileStorageService fileStorageService,
         string wwwRootPath)
        {
            _donationRepository = donationRepository;
            _fileStorageService = fileStorageService;
            _wwwRootPath = wwwRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }




        public async Task<int> CreateDonationAsync(Guid userId, CreateDonationDto dto)
        {
            string? fileUrl = null;

            if (dto.TransactionPhoto != null)
            {
                fileUrl = await _fileStorageService.SaveAsync(
                    dto.TransactionPhoto,
                    _wwwRootPath,
                    "uploads/donations"
                );
            }
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
                Status = DonationStatus.Pending,
                TransactionPhotoUrl = fileUrl
            };

            await _donationRepository.AddAsync(donation);
            return donation.Id;
        }

        public async Task<bool> VerifyDonationAsync(int donationId, Guid adminUserId)
        {
            var donation = await _donationRepository.GetByIdAsync(donationId);
            if (donation == null) return false;

            donation.Status = DonationStatus.Verified;
            donation.VerifiedAt = DateTime.UtcNow;
            donation.VerifiedBy = adminUserId;

            await _donationRepository.UpdateAsync(donation);
            return true;
        }


        public async Task<List<DonationDto>> GetDonationsByOrganizationIdAsync(int organizationId)
        {
            var donations = await _donationRepository.GetByOrganizationIdAsync(organizationId);

            return donations
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

        public async Task<List<PendingDonationDto>> GetPendingDonationsAsync()
        {
            var donations = await _donationRepository.GetPendingDonationsAsync();
            return donations.Select(d => new PendingDonationDto
            {
                Id = d.Id,
                DonorName = d.DonorName,
                UserName = d.User.Name,
                DonorContact = d.DonorContact,
                DonationType = d.DonationType.ToString(),
                Amount = d.Amount,
                Description = d.Description,
                ReceivedAt = d.ReceivedAt,
                Status = d.Status.ToString(),
                TransactionPhotoUrl = d.TransactionPhotoUrl
            }).ToList();
        }

        public async Task<List<DonationDto>> GetVerifiedDonationsAsync()
        {
            var donations = await _donationRepository.GetAllAsync();
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

        public async Task<object> GetDonationSummaryAsync()
        {
            var donations = await _donationRepository.GetAllAsync();

            return new
            {
                TotalAmount = donations.Where(d => d.Status == DonationStatus.Verified).Sum(d => d.Amount ?? 0),
                TotalDonations = donations.Count(d => d.Status == DonationStatus.Verified),
                ByType = donations
                    .Where(d => d.Status == DonationStatus.Verified)
                    .GroupBy(d => d.DonationType)
                    .Select(g => new { Type = g.Key.ToString(), Amount = g.Sum(x => x.Amount ?? 0) })
                    .ToList(),
                MonthlyStats = donations
                    .Where(d => d.Status == DonationStatus.Verified)
                    .GroupBy(d => new { d.ReceivedAt.Year, d.ReceivedAt.Month })
                    .Select(g => new { Month = $"{g.Key.Year}-{g.Key.Month:D2}", Amount = g.Sum(x => x.Amount ?? 0) })
                    .OrderBy(g => g.Month)
                    .ToList()
            };
        }

    }
}
