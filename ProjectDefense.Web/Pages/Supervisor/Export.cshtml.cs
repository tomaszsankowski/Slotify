using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ProjectDefense.Core.Data;
using System.ComponentModel.DataAnnotations;
using System.Text;
using ClosedXML.Excel;
using ProjectDefense.Web.Data;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

using QuestContainer = QuestPDF.Infrastructure.IContainer;

namespace ProjectDefense.Web.Pages.Supervisor
{
    [Authorize(Roles = "Supervisor")]
    public class ExportModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ExportModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public SelectList Rooms { get; set; }

        public class InputModel
        {
            [Required]
            [Display(Name = "Room")]
            public int RoomId { get; set; }

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "Start date")]
            public DateTime StartDate { get; set; } = DateTime.Today;

            [Required]
            [DataType(DataType.Date)]
            [Display(Name = "End date")]
            public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);

            [Required]
            [Display(Name = "Export format")]
            public string ExportFormat { get; set; }
        }

        public async Task OnGetAsync()
        {
            await PopulateRoomsDropDownList();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await PopulateRoomsDropDownList();
                return Page();
            }

            var reservations = await _context.Reservations
                .Include(r => r.Student)
                .Include(r => r.SupervisorAvailability.Room)
                .Where(r => r.SupervisorAvailability.RoomId == Input.RoomId &&
                            r.StartTime >= Input.StartDate &&
                            r.StartTime < Input.EndDate.AddDays(1))
                .OrderBy(r => r.StartTime)
                .ToListAsync();

            var roomName = await _context.Rooms
                .Where(r => r.Id == Input.RoomId)
                .Select(r => r.Name)
                .FirstOrDefaultAsync() ?? "Unknown Room";

            var fileName = $"Reservations_{roomName}_{Input.StartDate:yyyy-MM-dd}_to_{Input.EndDate:yyyy-MM-dd}";

            switch (Input.ExportFormat.ToLower())
            {
                case "txt":
                    return File(GenerateTxt(reservations), "text/plain", $"{fileName}.txt");

                case "xlsx":
                    return File(GenerateXlsx(reservations),
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"{fileName}.xlsx");

                case "pdf":
                    QuestPDF.Settings.License = LicenseType.Community;
                    return File(GeneratePdf(reservations), "application/pdf", $"{fileName}.pdf");

                default:
                    return BadRequest("Incorrect export format.");
            }
        }

        private async Task PopulateRoomsDropDownList()
        {
            var roomsQuery = from d in _context.Rooms
                             orderby d.Name
                             select d;

            Rooms = new SelectList(await roomsQuery.AsNoTracking().ToListAsync(), "Id", "Name");
        }

        private byte[] GenerateTxt(List<Reservation> reservations)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Reservations");
            builder.AppendLine($"From: {Input.StartDate:d} To: {Input.EndDate:d}");
            builder.AppendLine("-------------------------------------------------");

            foreach (var res in reservations)
            {
                var studentInfo = res.Student != null ? res.Student.UserName : "Free";
                builder.AppendLine($"{res.StartTime:g} - {res.EndTime:t} | {studentInfo}");
            }

            return Encoding.UTF8.GetBytes(builder.ToString());
        }

        private byte[] GenerateXlsx(List<Reservation> reservations)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Reservations");

            worksheet.Cell("A1").Value = "Start time";
            worksheet.Cell("B1").Value = "End time";
            worksheet.Cell("C1").Value = "Student";
            worksheet.Cell("D1").Value = "Status";

            var header = worksheet.Row(1);
            header.Style.Font.Bold = true;
            header.Style.Fill.BackgroundColor = XLColor.LightGray;

            int currentRow = 2;

            foreach (var res in reservations)
            {
                worksheet.Cell(currentRow, 1).Value = res.StartTime;
                worksheet.Cell(currentRow, 2).Value = res.EndTime;
                worksheet.Cell(currentRow, 3).Value = res.Student?.UserName ?? "-";
                worksheet.Cell(currentRow, 4).Value = res.StudentId != null ? "Reserved" : "Free";
                currentRow++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        private static QuestContainer CellStyle(QuestContainer container)
        {
            return container
                .DefaultTextStyle(x => x.SemiBold())
                .PaddingVertical(5)
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2);
        }

        private byte[] GeneratePdf(List<Reservation> reservations)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header()
                        .Text("Reservations")
                        .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);

                    page.Content()
                        .PaddingVertical(1, Unit.Centimetre)
                        .Column(x =>
                        {
                            x.Spacing(20);
                            x.Item().Text($"Date: {Input.StartDate:d} - {Input.EndDate:d}");

                            x.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(3);
                                    columns.RelativeColumn(4);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("Start");
                                    header.Cell().Element(CellStyle).Text("End");
                                    header.Cell().Element(CellStyle).Text("Student");
                                });

                                foreach (var res in reservations)
                                {
                                    table.Cell().Text(res.StartTime.ToString("g"));
                                    table.Cell().Text(res.EndTime.ToString("t"));
                                    table.Cell().Text(res.Student?.UserName ?? "Free");
                                }
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                        });
                });
            }).GeneratePdf();
        }
    }
}
