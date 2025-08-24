using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Nexora.Application.Interfaces;
using Nexora.Application.Services;
using Nexora.Core.Entities;
using System.Data;

namespace Nexora.Web.Controllers;

[Authorize]
public class ExportController : BaseController
{
    private readonly IPdfExportService _pdfService;
    private readonly ITicketService _ticketService;
    private readonly IUserService _userService;
    private readonly IBlogPostService _blogService;
    
    public ExportController(
        IPdfExportService pdfService,
        ITicketService ticketService,
        IUserService userService,
        IBlogPostService blogService)
    {
        _pdfService = pdfService;
        _ticketService = ticketService;
        _userService = userService;
        _blogService = blogService;
    }
    
    [HttpGet]
    public async Task<IActionResult> ExportTicketPdf(int id)
    {
        var ticketResult = await _ticketService.GetTicketByIdAsync(id);
        if (!ticketResult.IsSuccess)
        {
            return NotFound();
        }
        
        var commentsResult = await _ticketService.GetTicketCommentsAsync(id, true);
        var comments = commentsResult.IsSuccess ? commentsResult.Data : new List<TicketComment>();
        
        var pdfBytes = _pdfService.GenerateTicketReport(ticketResult.Data, comments);
        
        return File(pdfBytes, "application/pdf", $"Ticket_{ticketResult.Data.TicketNumber}.pdf");
    }
    
    [HttpPost]
    public async Task<IActionResult> ExportTicketListPdf([FromBody] ExportRequest request)
    {
        var ticketsResult = await _ticketService.GetTicketsAsync();
        if (!ticketsResult.IsSuccess)
        {
            return BadRequest();
        }
        
        var tickets = ticketsResult.Data;
        
        // Apply filters if provided
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<TicketStatus>(request.Status, out var status))
        {
            tickets = tickets.Where(t => t.Status == status).ToList();
        }
        
        if (!string.IsNullOrEmpty(request.Priority) && Enum.TryParse<TicketPriority>(request.Priority, out var priority))
        {
            tickets = tickets.Where(t => t.Priority == priority).ToList();
        }
        
        var title = string.IsNullOrEmpty(request.Title) ? "Ticket Report" : request.Title;
        var pdfBytes = _pdfService.GenerateTicketListReport(tickets, title);
        
        return File(pdfBytes, "application/pdf", $"TicketReport_{DateTime.Now:yyyyMMdd}.pdf");
    }
    
    [HttpGet]
    public async Task<IActionResult> ExportStatisticsPdf()
    {
        var statsResult = await _ticketService.GetTicketStatisticsAsync();
        if (!statsResult.IsSuccess)
        {
            return BadRequest();
        }
        
        var pdfBytes = _pdfService.GenerateStatisticsReport(statsResult.Data);
        
        return File(pdfBytes, "application/pdf", $"Statistics_{DateTime.Now:yyyyMMdd}.pdf");
    }
    
    [HttpPost]
    public async Task<IActionResult> ExportTicketsExcel([FromBody] ExportRequest request)
    {
        var ticketsResult = await _ticketService.GetTicketsAsync();
        if (!ticketsResult.IsSuccess)
        {
            return BadRequest();
        }
        
        var tickets = ticketsResult.Data;
        
        // Apply filters
        if (!string.IsNullOrEmpty(request.Status) && Enum.TryParse<TicketStatus>(request.Status, out var status))
        {
            tickets = tickets.Where(t => t.Status == status).ToList();
        }
        
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Tickets");
        
        // Headers
        worksheet.Cell(1, 1).Value = "Ticket #";
        worksheet.Cell(1, 2).Value = "Title";
        worksheet.Cell(1, 3).Value = "Customer";
        worksheet.Cell(1, 4).Value = "Status";
        worksheet.Cell(1, 5).Value = "Priority";
        worksheet.Cell(1, 6).Value = "Category";
        worksheet.Cell(1, 7).Value = "Assigned To";
        worksheet.Cell(1, 8).Value = "Created";
        worksheet.Cell(1, 9).Value = "Updated";
        worksheet.Cell(1, 10).Value = "Resolved";
        
        // Style headers
        var headerRange = worksheet.Range(1, 1, 1, 10);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thick;
        
        // Data
        int row = 2;
        foreach (var ticket in tickets)
        {
            worksheet.Cell(row, 1).Value = ticket.TicketNumber;
            worksheet.Cell(row, 2).Value = ticket.Title;
            worksheet.Cell(row, 3).Value = ticket.Customer?.FullName ?? "N/A";
            worksheet.Cell(row, 4).Value = ticket.Status.ToString();
            worksheet.Cell(row, 5).Value = ticket.Priority.ToString();
            worksheet.Cell(row, 6).Value = ticket.Category ?? "N/A";
            worksheet.Cell(row, 7).Value = ticket.AssignedTo?.FullName ?? "Unassigned";
            worksheet.Cell(row, 8).Value = ticket.CreatedAt;
            worksheet.Cell(row, 9).Value = ticket.UpdatedAt;
            worksheet.Cell(row, 10).Value = ticket.ResolvedAt?.ToString() ?? "";
            
            // Apply status coloring
            var statusCell = worksheet.Cell(row, 4);
            switch (ticket.Status)
            {
                case TicketStatus.Open:
                    statusCell.Style.Fill.BackgroundColor = XLColor.LightYellow;
                    break;
                case TicketStatus.InProgress:
                    statusCell.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    break;
                case TicketStatus.Resolved:
                    statusCell.Style.Fill.BackgroundColor = XLColor.LightGreen;
                    break;
                case TicketStatus.Closed:
                    statusCell.Style.Fill.BackgroundColor = XLColor.LightGray;
                    break;
            }
            
            // Apply priority coloring
            var priorityCell = worksheet.Cell(row, 5);
            switch (ticket.Priority)
            {
                case TicketPriority.Low:
                    priorityCell.Style.Fill.BackgroundColor = XLColor.LightGreen;
                    break;
                case TicketPriority.Medium:
                    priorityCell.Style.Fill.BackgroundColor = XLColor.LightYellow;
                    break;
                case TicketPriority.High:
                    priorityCell.Style.Fill.BackgroundColor = XLColor.LightCoral;
                    break;
                case TicketPriority.Urgent:
                    priorityCell.Style.Fill.BackgroundColor = XLColor.Red;
                    priorityCell.Style.Font.FontColor = XLColor.White;
                    break;
            }
            
            row++;
        }
        
        // Auto-fit columns
        worksheet.Columns().AdjustToContents();
        
        // Add summary sheet
        var summarySheet = workbook.Worksheets.Add("Summary");
        summarySheet.Cell(1, 1).Value = "Ticket Summary Report";
        summarySheet.Cell(1, 1).Style.Font.Bold = true;
        summarySheet.Cell(1, 1).Style.Font.FontSize = 16;
        
        summarySheet.Cell(3, 1).Value = "Total Tickets:";
        summarySheet.Cell(3, 2).Value = tickets.Count;
        
        summarySheet.Cell(4, 1).Value = "Open:";
        summarySheet.Cell(4, 2).Value = tickets.Count(t => t.Status == TicketStatus.Open);
        
        summarySheet.Cell(5, 1).Value = "In Progress:";
        summarySheet.Cell(5, 2).Value = tickets.Count(t => t.Status == TicketStatus.InProgress);
        
        summarySheet.Cell(6, 1).Value = "Resolved:";
        summarySheet.Cell(6, 2).Value = tickets.Count(t => t.Status == TicketStatus.Resolved);
        
        summarySheet.Cell(7, 1).Value = "Closed:";
        summarySheet.Cell(7, 2).Value = tickets.Count(t => t.Status == TicketStatus.Closed);
        
        summarySheet.Cell(9, 1).Value = "Generated:";
        summarySheet.Cell(9, 2).Value = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        
        // Save to memory stream
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        
        return File(content, 
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Tickets_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }
    
    [HttpGet]
    public async Task<IActionResult> ExportUsersExcel()
    {
        var usersResult = await _userService.GetAllUsersAsync();
        if (!usersResult.IsSuccess)
        {
            return BadRequest();
        }
        var users = usersResult.Data;
        
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Users");
        
        // Headers
        worksheet.Cell(1, 1).Value = "Username";
        worksheet.Cell(1, 2).Value = "Full Name";
        worksheet.Cell(1, 3).Value = "Email";
        worksheet.Cell(1, 4).Value = "Phone";
        worksheet.Cell(1, 5).Value = "Status";
        worksheet.Cell(1, 6).Value = "Created";
        worksheet.Cell(1, 7).Value = "Last Login";
        
        // Style headers
        var headerRange = worksheet.Range(1, 1, 1, 7);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        
        // Data
        int row = 2;
        foreach (var user in users)
        {
            worksheet.Cell(row, 1).Value = user.UserName;
            worksheet.Cell(row, 2).Value = user.FullName ?? "N/A";
            worksheet.Cell(row, 3).Value = user.Email;
            worksheet.Cell(row, 4).Value = user.PhoneNumber ?? "N/A";
            worksheet.Cell(row, 5).Value = user.IsActive ? "Active" : "Inactive";
            worksheet.Cell(row, 6).Value = user.CreatedAt;
            worksheet.Cell(row, 7).Value = user.UpdatedAt?.ToString() ?? "Never";
            
            if (!user.IsActive)
            {
                worksheet.Row(row).Style.Font.FontColor = XLColor.Gray;
            }
            
            row++;
        }
        
        worksheet.Columns().AdjustToContents();
        
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        
        return File(content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Users_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }
    
    [HttpGet]
    public async Task<IActionResult> ExportBlogPostPdf(int id)
    {
        var postResult = await _blogService.GetByIdAsync(id);
        if (!postResult.IsSuccess)
        {
            return NotFound();
        }
        
        var pdfBytes = _pdfService.GenerateBlogPostPdf(postResult.Data);
        
        var fileName = postResult.Data.Title.Replace(" ", "_").Replace("/", "_");
        return File(pdfBytes, "application/pdf", $"{fileName}.pdf");
    }
    
    [HttpPost]
    public async Task<IActionResult> ExportDataTableExcel([FromBody] DataTableExportRequest request)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(request.SheetName ?? "Data");
        
        // Add headers
        for (int i = 0; i < request.Headers.Count; i++)
        {
            worksheet.Cell(1, i + 1).Value = request.Headers[i];
        }
        
        // Style headers
        var headerRange = worksheet.Range(1, 1, 1, request.Headers.Count);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;
        headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thick;
        
        // Add data
        for (int row = 0; row < request.Data.Count; row++)
        {
            for (int col = 0; col < request.Data[row].Count; col++)
            {
                worksheet.Cell(row + 2, col + 1).Value = request.Data[row][col];
            }
        }
        
        // Auto-fit columns
        worksheet.Columns().AdjustToContents();
        
        // Add filters
        worksheet.RangeUsed().SetAutoFilter();
        
        // Freeze header row
        worksheet.SheetView.FreezeRows(1);
        
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();
        
        return File(content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"{request.FileName ?? "Export"}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
    }
}

public class ExportRequest
{
    public string Title { get; set; }
    public string Status { get; set; }
    public string Priority { get; set; }
    public string Category { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class DataTableExportRequest
{
    public List<string> Headers { get; set; }
    public List<List<string>> Data { get; set; }
    public string FileName { get; set; }
    public string SheetName { get; set; }
}