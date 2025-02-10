using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout.Borders;
using iText.IO.Image;
using RhManagementApi.DTOs;

namespace RhManagementApi.Services
{
    public class PdfService
    {
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
                if (!string.IsNullOrEmpty(record.Employee.Picture) && File.Exists(record.Employee.Picture))
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

        private Paragraph CreateBoldUnderlinedParagraph(string text)
        {
            return new Paragraph(text)
                .SetBold()
                .SetUnderline()
                .SetMarginBottom(5);
        }
    }
} 