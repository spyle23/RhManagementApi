using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Borders;
using iText.IO.Image;
using RhManagementApi.DTOs;
using RhManagementApi.Model;
using Microsoft.Extensions.Logging;

namespace RhManagementApi.Services
{
    public class PdfService
    {
        private readonly ILogger<PdfService> _logger;

        public PdfService(ILogger<PdfService> logger)
        {
            _logger = logger;
        }

        public byte[] GenerateEmployeeRecordPdf(EmployeeRecordDto record)
        {
            using (var stream = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                // Create a table for the header with picture
                Table headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 70, 30 }))
                    .UseAllAvailableWidth();

                // Add employee information on the left
                Cell infoCell = new Cell()
                    .Add(new Paragraph("Fiche Employée")
                        .SetTextAlignment(TextAlignment.CENTER)
                        .SetFontSize(20))
                    .Add(CreateBoldUnderlinedParagraph("Nom:"))
                    .Add(new Paragraph($"{record.Employee.FirstName} {record.Employee.LastName}").SetMarginBottom(10))
                    .Add(CreateBoldUnderlinedParagraph("Email:"))
                    .Add(new Paragraph($"{record.Employee.Email}").SetMarginBottom(10))
                    .Add(CreateBoldUnderlinedParagraph("Date d'entrée:"))
                    .Add(new Paragraph($"{record.Employee.DateOfHiring.ToShortDateString()}").SetMarginBottom(10))
                    .Add(CreateBoldUnderlinedParagraph("Position:"))
                    .Add(new Paragraph($"{record.Poste}").SetMarginBottom(10))
                    .Add(CreateBoldUnderlinedParagraph("Status:"))
                    .Add(new Paragraph($"{record.Status}").SetMarginBottom(10))
                    .Add(CreateBoldUnderlinedParagraph("Salaire brute:"))
                    .Add(new Paragraph($"{record.GrossSalary} Ar").SetMarginBottom(10))
                    .Add(CreateBoldUnderlinedParagraph("Address:"))
                    .Add(new Paragraph($"{record.Adresse}").SetMarginBottom(10))
                    .Add(CreateBoldUnderlinedParagraph("Phone:"))
                    .Add(new Paragraph($"{record.Telephone}").SetMarginBottom(10))
                    .Add(CreateBoldUnderlinedParagraph("Date de naissance:"))
                    .Add(new Paragraph($"{record.Birthday.ToShortDateString()}").SetMarginBottom(10))
                    .Add(CreateBoldUnderlinedParagraph("Profil:"))
                    .Add(new Paragraph($"{record.Profil}"))
                    .SetBorder(Border.NO_BORDER);

                headerTable.AddCell(infoCell);

                // Add employee picture on the right if available
                if (!string.IsNullOrEmpty(record.Employee.Picture) && System.IO.File.Exists(record.Employee.Picture))
                {
                    ImageData imageData = ImageDataFactory.Create(record.Employee.Picture);
                    Image image = new Image(imageData)
                        .SetWidth(100)
                        .SetAutoScaleHeight(true);

                    Cell imageCell = new Cell()
                        .Add(image)
                        .SetTextAlignment(TextAlignment.RIGHT)
                        .SetBorder(Border.NO_BORDER);

                    headerTable.AddCell(imageCell);
                }
                else
                {
                    headerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER));
                }

                document.Add(headerTable);
                document.Close();
                return stream.ToArray();
            }
        }

        public byte[] GeneratePayslipPdf(Payslip payslip)
        {
            using (var stream = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(stream);
                PdfDocument pdf = new PdfDocument(writer);
                Document document = new Document(pdf);

                // Create a table for the header with logo
                Table headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 80, 20 }))
                    .UseAllAvailableWidth()
                    .SetMarginBottom(20);

                // Add empty cell on the left
                headerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER));

                // Add logo if exists
                var logoPath = Path.Combine("Uploads", "MC.png");
                if (System.IO.File.Exists(logoPath))
                {
                    _logger.LogInformation("Logo found at path: {LogoPath}", logoPath);
                    ImageData imageData = ImageDataFactory.Create(logoPath);
                    Image logo = new Image(imageData)
                        .SetWidth(50)
                        .SetAutoScaleHeight(true);

                    Cell logoCell = new Cell()
                        .Add(logo)
                        .SetTextAlignment(TextAlignment.RIGHT)
                        .SetBorder(Border.NO_BORDER);

                    headerTable.AddCell(logoCell);
                }
                else
                {
                    _logger.LogWarning("Logo not found at path: {LogoPath}", logoPath);
                    headerTable.AddCell(new Cell().SetBorder(Border.NO_BORDER));
                }

                document.Add(headerTable);

                // Add title
                document.Add(new Paragraph("Fiche de paie")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(20));

                // Add employee information
                document.Add(new Paragraph($"Employé: {payslip.Employee.FirstName} {payslip.Employee.LastName}"));
                document.Add(new Paragraph($"Mois: {payslip.Month.ToString("MMMM yyyy")}"));

                // Add salary details
                Table table = new Table(UnitValue.CreatePercentArray(2)).UseAllAvailableWidth();
                
                // Earnings
                table.AddCell("Salaire brut");
                table.AddCell($"{payslip.GrossSalary:N2} Ar");
                
                table.AddCell("Primes");
                table.AddCell($"{payslip.Bonuses:N2} Ar");
                
                table.AddCell("Heures supplémentaires");
                table.AddCell($"{payslip.Overtime:N2} Ar");

                // Total Earnings
                table.AddCell("Total des gains");
                decimal totalEarnings = payslip.GrossSalary + payslip.Bonuses + payslip.Overtime;
                table.AddCell($"{totalEarnings:N2} Ar");

                // Deductions
                table.AddCell("Impôts (20%)");
                decimal taxes = payslip.GrossSalary * 0.20m;
                table.AddCell($"-{taxes:N2} Ar");

                // Net Salary
                table.AddCell("Salaire net");
                table.AddCell($"{payslip.NetSalary:N2} Ar");

                document.Add(table);

                document.Close();
                return stream.ToArray();
            }
        }

        private Paragraph CreateBoldUnderlinedParagraph(string text)
        {
            return new Paragraph(text)
                .SetBold()
                .SetUnderline()
                .SetMarginBottom(5);
        }
    }
}