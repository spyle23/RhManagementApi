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
                    .Add(new Paragraph($"Nom: {record.Employee.FirstName} {record.Employee.LastName}"))
                    .Add(new Paragraph($"Email: {record.Employee.Email}"))
                    .Add(new Paragraph($"Date d'entrée: {record.Employee.DateOfHiring.ToShortDateString()}"))
                    .Add(new Paragraph($"Position: {record.Poste}"))
                    .Add(new Paragraph($"Status: {record.Status}"))
                    .Add(new Paragraph($"Salaire brute: {record.GrossSalary:C}"))
                    .Add(new Paragraph($"Address: {record.Adresse}"))
                    .Add(new Paragraph($"Phone: {record.Telephone}"))
                    .Add(new Paragraph($"Date de naissance: {record.Birthday.ToShortDateString()}"))
                    .Add(new Paragraph($"Profil: {record.Profil}"))
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
    }
} 