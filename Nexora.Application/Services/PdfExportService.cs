using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Hosting;
using Nexora.Core.Entities;
using System.IO;

namespace Nexora.Application.Services;

public interface IPdfExportService
{
    byte[] GenerateTicketReport(Ticket ticket, List<TicketComment>? comments = null);
    byte[] GenerateTicketListReport(List<Ticket> tickets, string title = "Ticket Report");
    byte[] GenerateStatisticsReport(dynamic statistics);
    byte[] GenerateUserReport(List<ApplicationUser> users);
    byte[] GenerateBlogPostPdf(BlogPost post);
}

public class PdfExportService : IPdfExportService
{
    private readonly IWebHostEnvironment _environment;
    
    public PdfExportService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }
    
    public byte[] GenerateTicketReport(Ticket ticket, List<TicketComment>? comments = null)
    {
        using var ms = new MemoryStream();
        var document = new Document(PageSize.A4, 50, 50, 50, 50);
        var writer = PdfWriter.GetInstance(document, ms);
        
        document.Open();
        
        // Header
        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
        var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
        var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
        
        // Title
        document.Add(new Paragraph($"Ticket #{ticket.TicketNumber}", titleFont));
        document.Add(new Paragraph($"{ticket.Title}\n\n", headerFont));
        
        // Ticket Details Table
        var detailsTable = new PdfPTable(2);
        detailsTable.WidthPercentage = 100;
        detailsTable.SetWidths(new float[] { 30f, 70f });
        
        AddTableRow(detailsTable, "Status:", ticket.Status.ToString(), headerFont, normalFont);
        AddTableRow(detailsTable, "Priority:", ticket.Priority.ToString(), headerFont, normalFont);
        AddTableRow(detailsTable, "Category:", ticket.Category ?? "N/A", headerFont, normalFont);
        AddTableRow(detailsTable, "Created:", ticket.CreatedAt.ToString("yyyy-MM-dd HH:mm"), headerFont, normalFont);
        AddTableRow(detailsTable, "Customer:", ticket.Customer?.FullName ?? "N/A", headerFont, normalFont);
        AddTableRow(detailsTable, "Assigned To:", ticket.AssignedTo?.FullName ?? "Unassigned", headerFont, normalFont);
        
        if (ticket.ResolvedAt.HasValue)
        {
            AddTableRow(detailsTable, "Resolved:", ticket.ResolvedAt.Value.ToString("yyyy-MM-dd HH:mm"), headerFont, normalFont);
        }
        
        document.Add(detailsTable);
        document.Add(new Paragraph("\n"));
        
        // Description
        document.Add(new Paragraph("Description:", headerFont));
        document.Add(new Paragraph(ticket.Description ?? "No description provided.", normalFont));
        document.Add(new Paragraph("\n"));
        
        // Comments
        if (comments != null && comments.Any())
        {
            document.Add(new Paragraph("Comments:", headerFont));
            document.Add(new Paragraph("\n"));
            
            foreach (var comment in comments.OrderBy(c => c.CreatedAt))
            {
                var commentTable = new PdfPTable(1);
                commentTable.WidthPercentage = 100;
                
                var cell = new PdfPCell();
                cell.Border = Rectangle.BOX;
                cell.Padding = 10;
                
                var commentPara = new Paragraph();
                commentPara.Add(new Chunk($"{comment.User?.FullName ?? "Unknown"} - {comment.CreatedAt:yyyy-MM-dd HH:mm}\n", headerFont));
                commentPara.Add(new Chunk(comment.Comment, normalFont));
                
                cell.AddElement(commentPara);
                commentTable.AddCell(cell);
                
                document.Add(commentTable);
                document.Add(new Paragraph("\n"));
            }
        }
        
        // Footer
        document.Add(new Paragraph($"\nGenerated on: {DateTime.Now:yyyy-MM-dd HH:mm}", 
            FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 8)));
        
        document.Close();
        return ms.ToArray();
    }
    
    public byte[] GenerateTicketListReport(List<Ticket> tickets, string title = "Ticket Report")
    {
        using var ms = new MemoryStream();
        var document = new Document(PageSize.A4.Rotate(), 30, 30, 30, 30);
        var writer = PdfWriter.GetInstance(document, ms);
        
        document.Open();
        
        // Fonts
        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
        var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
        var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
        
        // Title
        document.Add(new Paragraph(title, titleFont));
        document.Add(new Paragraph($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}\n\n", normalFont));
        
        // Summary
        var summaryTable = new PdfPTable(4);
        summaryTable.WidthPercentage = 50;
        summaryTable.HorizontalAlignment = Element.ALIGN_LEFT;
        
        AddSummaryCell(summaryTable, "Total Tickets", tickets.Count.ToString(), headerFont, normalFont);
        AddSummaryCell(summaryTable, "Open", tickets.Count(t => t.Status == TicketStatus.Open).ToString(), headerFont, normalFont);
        AddSummaryCell(summaryTable, "In Progress", tickets.Count(t => t.Status == TicketStatus.InProgress).ToString(), headerFont, normalFont);
        AddSummaryCell(summaryTable, "Resolved", tickets.Count(t => t.Status == TicketStatus.Resolved).ToString(), headerFont, normalFont);
        
        document.Add(summaryTable);
        document.Add(new Paragraph("\n"));
        
        // Tickets Table
        var table = new PdfPTable(7);
        table.WidthPercentage = 100;
        table.SetWidths(new float[] { 15f, 25f, 15f, 12f, 12f, 12f, 9f });
        
        // Headers
        AddHeaderCell(table, "Ticket #", headerFont);
        AddHeaderCell(table, "Title", headerFont);
        AddHeaderCell(table, "Customer", headerFont);
        AddHeaderCell(table, "Status", headerFont);
        AddHeaderCell(table, "Priority", headerFont);
        AddHeaderCell(table, "Assigned To", headerFont);
        AddHeaderCell(table, "Created", headerFont);
        
        // Data
        foreach (var ticket in tickets)
        {
            table.AddCell(new PdfPCell(new Phrase(ticket.TicketNumber, normalFont)));
            table.AddCell(new PdfPCell(new Phrase(TruncateString(ticket.Title, 40), normalFont)));
            table.AddCell(new PdfPCell(new Phrase(ticket.Customer?.FullName ?? "N/A", normalFont)));
            
            // Status with color
            var statusCell = new PdfPCell(new Phrase(ticket.Status.ToString(), normalFont));
            statusCell.BackgroundColor = GetStatusColor(ticket.Status);
            table.AddCell(statusCell);
            
            // Priority with color
            var priorityCell = new PdfPCell(new Phrase(ticket.Priority.ToString(), normalFont));
            priorityCell.BackgroundColor = GetPriorityColor(ticket.Priority);
            table.AddCell(priorityCell);
            
            table.AddCell(new PdfPCell(new Phrase(ticket.AssignedTo?.FullName ?? "Unassigned", normalFont)));
            table.AddCell(new PdfPCell(new Phrase(ticket.CreatedAt.ToString("MM/dd"), normalFont)));
        }
        
        document.Add(table);
        
        document.Close();
        return ms.ToArray();
    }
    
    public byte[] GenerateStatisticsReport(dynamic statistics)
    {
        using var ms = new MemoryStream();
        var document = new Document(PageSize.A4, 50, 50, 50, 50);
        var writer = PdfWriter.GetInstance(document, ms);
        
        document.Open();
        
        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
        var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
        var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
        
        // Title
        document.Add(new Paragraph("Ticket Statistics Report", titleFont));
        document.Add(new Paragraph($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}\n\n", normalFont));
        
        // Overview Section
        document.Add(new Paragraph("Overview", headerFont));
        var overviewTable = new PdfPTable(4);
        overviewTable.WidthPercentage = 100;
        overviewTable.SpacingBefore = 10;
        overviewTable.SpacingAfter = 20;
        
        AddStatCell(overviewTable, "Total Tickets", statistics.TotalTickets.ToString(), headerFont, normalFont);
        AddStatCell(overviewTable, "Open Tickets", statistics.OpenTickets.ToString(), headerFont, normalFont);
        AddStatCell(overviewTable, "Resolved", statistics.ResolvedTickets.ToString(), headerFont, normalFont);
        AddStatCell(overviewTable, "Avg Resolution", $"{statistics.AverageResolutionHours:F1}h", headerFont, normalFont);
        
        document.Add(overviewTable);
        
        // Priority Distribution
        document.Add(new Paragraph("Tickets by Priority", headerFont));
        var priorityTable = new PdfPTable(3);
        priorityTable.WidthPercentage = 60;
        priorityTable.SpacingBefore = 10;
        priorityTable.SpacingAfter = 20;
        
        AddHeaderCell(priorityTable, "Priority", headerFont);
        AddHeaderCell(priorityTable, "Count", headerFont);
        AddHeaderCell(priorityTable, "Percentage", headerFont);
        
        foreach (var priority in statistics.TicketsByPriority)
        {
            var percentage = statistics.TotalTickets > 0 ? (priority.Value * 100.0 / statistics.TotalTickets) : 0;
            priorityTable.AddCell(new Phrase(priority.Key.ToString(), normalFont));
            priorityTable.AddCell(new Phrase(priority.Value.ToString(), normalFont));
            priorityTable.AddCell(new Phrase($"{percentage:F1}%", normalFont));
        }
        
        document.Add(priorityTable);
        
        // Category Distribution
        document.Add(new Paragraph("Tickets by Category", headerFont));
        var categoryTable = new PdfPTable(3);
        categoryTable.WidthPercentage = 60;
        categoryTable.SpacingBefore = 10;
        
        AddHeaderCell(categoryTable, "Category", headerFont);
        AddHeaderCell(categoryTable, "Count", headerFont);
        AddHeaderCell(categoryTable, "Percentage", headerFont);
        
        foreach (var category in statistics.TicketsByCategory)
        {
            var percentage = statistics.TotalTickets > 0 ? (category.Value * 100.0 / statistics.TotalTickets) : 0;
            categoryTable.AddCell(new Phrase(category.Key, normalFont));
            categoryTable.AddCell(new Phrase(category.Value.ToString(), normalFont));
            categoryTable.AddCell(new Phrase($"{percentage:F1}%", normalFont));
        }
        
        document.Add(categoryTable);
        
        document.Close();
        return ms.ToArray();
    }
    
    public byte[] GenerateUserReport(List<ApplicationUser> users)
    {
        using var ms = new MemoryStream();
        var document = new Document(PageSize.A4, 50, 50, 50, 50);
        var writer = PdfWriter.GetInstance(document, ms);
        
        document.Open();
        
        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
        var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
        var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);
        
        document.Add(new Paragraph("User Report", titleFont));
        document.Add(new Paragraph($"Total Users: {users.Count}\n\n", normalFont));
        
        var table = new PdfPTable(5);
        table.WidthPercentage = 100;
        table.SetWidths(new float[] { 20f, 25f, 25f, 15f, 15f });
        
        AddHeaderCell(table, "Username", headerFont);
        AddHeaderCell(table, "Full Name", headerFont);
        AddHeaderCell(table, "Email", headerFont);
        AddHeaderCell(table, "Status", headerFont);
        AddHeaderCell(table, "Created", headerFont);
        
        foreach (var user in users)
        {
            table.AddCell(new Phrase(user.UserName, normalFont));
            table.AddCell(new Phrase(user.FullName ?? "N/A", normalFont));
            table.AddCell(new Phrase(user.Email, normalFont));
            table.AddCell(new Phrase(user.IsActive ? "Active" : "Inactive", normalFont));
            table.AddCell(new Phrase(user.CreatedAt.ToString("yyyy-MM-dd"), normalFont));
        }
        
        document.Add(table);
        document.Close();
        return ms.ToArray();
    }
    
    public byte[] GenerateBlogPostPdf(BlogPost post)
    {
        using var ms = new MemoryStream();
        var document = new Document(PageSize.A4, 50, 50, 50, 50);
        var writer = PdfWriter.GetInstance(document, ms);
        
        document.Open();
        
        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
        var metaFont = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 10);
        var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);
        
        // Title
        document.Add(new Paragraph(post.Title, titleFont));
        
        // Meta information
        document.Add(new Paragraph($"By {post.Author?.FullName ?? "Unknown"} | {post.PublishedDate:yyyy-MM-dd}", metaFont));
        document.Add(new Paragraph($"Category: {post.Category?.Name ?? "Uncategorized"}\n\n", metaFont));
        
        // Summary
        if (!string.IsNullOrEmpty(post.Summary))
        {
            var summaryPara = new Paragraph(post.Summary, normalFont);
            summaryPara.SpacingAfter = 15;
            document.Add(summaryPara);
        }
        
        // Content - Strip HTML tags
        var content = System.Text.RegularExpressions.Regex.Replace(post.Content ?? "", "<.*?>", "");
        document.Add(new Paragraph(content, normalFont));
        
        // Footer
        document.Add(new Paragraph($"\n\nGenerated from blog on {DateTime.Now:yyyy-MM-dd HH:mm}", 
            FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 8)));
        
        document.Close();
        return ms.ToArray();
    }
    
    // Helper methods
    private void AddTableRow(PdfPTable table, string label, string value, Font labelFont, Font valueFont)
    {
        table.AddCell(new PdfPCell(new Phrase(label, labelFont)) { Border = Rectangle.NO_BORDER });
        table.AddCell(new PdfPCell(new Phrase(value, valueFont)) { Border = Rectangle.NO_BORDER });
    }
    
    private void AddHeaderCell(PdfPTable table, string text, Font font)
    {
        var cell = new PdfPCell(new Phrase(text, font));
        cell.BackgroundColor = BaseColor.LightGray;
        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        table.AddCell(cell);
    }
    
    private void AddSummaryCell(PdfPTable table, string label, string value, Font labelFont, Font valueFont)
    {
        var cell = new PdfPCell();
        cell.AddElement(new Paragraph(label, labelFont));
        cell.AddElement(new Paragraph(value, valueFont));
        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        table.AddCell(cell);
    }
    
    private void AddStatCell(PdfPTable table, string label, string value, Font labelFont, Font valueFont)
    {
        var cell = new PdfPCell();
        cell.Border = Rectangle.BOX;
        cell.Padding = 10;
        cell.AddElement(new Paragraph(label, labelFont));
        cell.AddElement(new Paragraph(value, valueFont));
        cell.HorizontalAlignment = Element.ALIGN_CENTER;
        table.AddCell(cell);
    }
    
    private BaseColor GetStatusColor(TicketStatus status)
    {
        return status switch
        {
            TicketStatus.Open => new BaseColor(255, 243, 224),
            TicketStatus.InProgress => new BaseColor(224, 242, 255),
            TicketStatus.Resolved => new BaseColor(224, 255, 224),
            TicketStatus.Closed => new BaseColor(240, 240, 240),
            _ => BaseColor.White
        };
    }
    
    private BaseColor GetPriorityColor(TicketPriority priority)
    {
        return priority switch
        {
            TicketPriority.Low => new BaseColor(224, 255, 224),
            TicketPriority.Medium => new BaseColor(255, 243, 224),
            TicketPriority.High => new BaseColor(255, 224, 224),
            TicketPriority.Urgent => new BaseColor(255, 200, 200),
            _ => BaseColor.White
        };
    }
    
    private string TruncateString(string value, int maxLength)
    {
        if (string.IsNullOrEmpty(value)) return value;
        return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
    }
}