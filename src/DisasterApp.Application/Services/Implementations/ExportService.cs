using CloudinaryDotNet.Core;
using DisasterApp.Application.DTOs;
using DisasterApp.Domain.Entities;
using DisasterApp.Infrastructure.Repositories;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DisasterApp.Application.Services
{
    public class ExportService : IExportService
    {

        private readonly IDisasterReportRepository _repository;
        public ExportService(IDisasterReportRepository repository)
        {
            _repository = repository;
        }
        private List<DisasterReportExportDto> MapToExportDto(List<DisasterReport> reports)
        {
            return reports.Select(r => new DisasterReportExportDto
            {

                Title = r.Title,
                Description = r.Description,
                Latitude = r.Location.Latitude,
                Longitude = r.Location.Longitude,
                Address = r.Location.Address,
                Timestamp = r.Timestamp,
                Severity = r.Severity,
                Status = r.Status,
                VerifiedAt = r.VerifiedAt,
                VerifiedBy = r.VerifiedByNavigation?.Name ?? "-",
                UserName = r.User.Name,
                UserEmail = r.User.Email,
                DisasterTypeName = r.DisasterEvent.DisasterType.Name,
                PhotoUrls = r.Photos.Select(p => p.Url).ToList(),
                DisasterEventName = r.DisasterEvent.Name,
                //ImpactDetails = string.Join("\n", r.ImpactDetails.Select(i => $"{i.ImpactTypes}: {i.Description}").ToList()       
            }).ToList();
        }


        public async Task<byte[]> ExportDisasterReportsToExcelAsync()
        {

            var reports = await _repository.GetAllForExportReportsAsync();
            var dtoReports = MapToExportDto(reports);

            using var workbook = new ClosedXML.Excel.XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Reports");
            worksheet.Cell(1, 1).Value = "Title";
            worksheet.Cell(1, 2).Value = "Description";
            worksheet.Cell(1, 3).Value = "Timestamp";
            worksheet.Cell(1, 4).Value = "Severity";
            worksheet.Cell(1, 5).Value = "Status";
            worksheet.Cell(1, 6).Value = "User";
            worksheet.Cell(1, 7).Value = "Disaster Event";

            int row = 2;
            foreach (var r in dtoReports)
            {
                worksheet.Cell(row, 1).Value = r.Title;
                worksheet.Cell(row, 2).Value = r.Description;
                worksheet.Cell(row, 3).Value = r.Timestamp;
                worksheet.Cell(row, 4).Value = r.Severity.ToString();
                worksheet.Cell(row, 5).Value = r.Status.ToString();
                worksheet.Cell(row, 6).Value = r.UserName;
                worksheet.Cell(row, 7).Value = r.DisasterEventName;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportDisasterReportsToPdfAsync()
        {

            QuestPDF.Settings.License = LicenseType.Community;
            var reports = await _repository.GetAllForExportReportsAsync();
            var dtoReports = MapToExportDto(reports);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);

                    page.Header()
                        .Text("Disaster Reports Export")
                        .FontSize(18)
                        .Bold()
                        .AlignCenter();

                    page.Content()
                        .Table(table =>
                        {
                            // Define columns (relative width)
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2); // Title
                                columns.RelativeColumn(2); // User
                                columns.RelativeColumn(2); // Severity
                                columns.RelativeColumn(2); // Status
                                columns.RelativeColumn(3);//Description
                                columns.RelativeColumn(2); // VerifiedBy
                                columns.RelativeColumn(2); // VerifiedAt
                                columns.RelativeColumn(2); // DisasterEvent
                                columns.RelativeColumn(2); //DisasterType
                                columns.RelativeColumn(2); // Location
                                columns.RelativeColumn(3); // ImpactDetails

                            });

                            // Header row
                            table.Header(header =>
                            {
                                header.Cell().Text("Title").Bold();
                                header.Cell().Text("User").Bold();
                                header.Cell().Text("UserEmail").Bold();
                                header.Cell().Text("Severity").Bold();
                                header.Cell().Text("Status").Bold();
                                header.Cell().Text("Description").Bold();
                                header.Cell().Text("Verified By").Bold();
                                header.Cell().Text("Verified At").Bold();
                                header.Cell().Text("DisasterType").Bold();
                                header.Cell().Text("Disaster Event").Bold();
                                header.Cell().Text("Location").Bold();
                                header.Cell().Text("Impact Details").Bold();
                            });

                            // Data rows
                            foreach (var r in dtoReports)
                            {
                                table.Cell().Text(r.Title).FontSize(8).WrapAnywhere();
                                table.Cell().Text(r.UserName).FontSize(8).WrapAnywhere();
                                table.Cell().Text(r.UserEmail).FontSize(8).WrapAnywhere();
                                table.Cell().Text(r.Severity).FontSize(8).WrapAnywhere(); // already string
                                table.Cell().Text(r.Status).FontSize(8).WrapAnywhere();   // already string
                                table.Cell().Text(r.Description).FontSize(8).WrapAnywhere();
                                table.Cell().Text(r.VerifiedBy ?? "-").FontSize(8).WrapAnywhere();
                                table.Cell().Text(r.VerifiedAt?.ToString("yyyy-MM-dd") ?? "-").FontSize(8).WrapAnywhere();
                                
                                table.Cell().Text(r.DisasterTypeName).FontSize(8).WrapAnywhere();
                                table.Cell().Text(r.DisasterEventName).FontSize(8).WrapAnywhere();

                                table.Cell().Text(r.Address).FontSize(8).WrapAnywhere();
                                table.Cell().Text(r.ImpactDetails).FontSize(8).WrapAnywhere();
                                
                            }
                        });
                });
            });

            return document.GeneratePdf();
        }
    }
    }
    

