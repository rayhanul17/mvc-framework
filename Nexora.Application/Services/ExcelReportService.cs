using ClosedXML.Excel;
using Nexora.Application.Helpers;
using System.Drawing;

namespace Nexora.Application.Services;

public class ExcelReportService
{
    /// <summary>
    /// Create an Excel file with multiple sheets
    /// </summary>
    public byte[] CreateMultiSheetReport()
    {
        // Method 1: Using CreateMultiSheetWorkbook with builders
        var sheetBuilders = new Dictionary<string, Action<IXLWorksheet>>
        {
            ["Summary"] = BuildSummarySheet,
            ["Sales Data"] = BuildSalesSheet,
            ["Charts"] = BuildChartsSheet,
            ["Settings"] = BuildSettingsSheet
        };
        
        var workbook = ExcelHelper.CreateMultiSheetWorkbook(sheetBuilders);
        
        // Set tab colors for better organization
        ExcelHelper.SetTabColor(ExcelHelper.GetWorksheet(workbook, "Summary")!, XLColor.Blue);
        ExcelHelper.SetTabColor(ExcelHelper.GetWorksheet(workbook, "Sales Data")!, XLColor.Green);
        ExcelHelper.SetTabColor(ExcelHelper.GetWorksheet(workbook, "Charts")!, XLColor.Orange);
        ExcelHelper.SetTabColor(ExcelHelper.GetWorksheet(workbook, "Settings")!, XLColor.Gray);
        
        return ExcelHelper.ExportToBytes(workbook);
    }
    
    /// <summary>
    /// Alternative method: Create multi-sheet report manually
    /// </summary>
    public byte[] CreateComplexMultiSheetReport()
    {
        // Create workbook without initial sheet
        var workbook = ExcelHelper.CreateWorkbook();
        
        // Add multiple sheets
        var summarySheet = ExcelHelper.AddWorksheet(workbook, "Executive Summary");
        var dataSheet = ExcelHelper.AddWorksheet(workbook, "Raw Data");
        var analysisSheet = ExcelHelper.AddWorksheet(workbook, "Analysis");
        var pivotSheet = ExcelHelper.AddWorksheet(workbook, "Pivot Tables");
        
        // Build Executive Summary Sheet
        BuildExecutiveSummary(summarySheet);
        
        // Build Raw Data Sheet
        BuildRawDataSheet(dataSheet);
        
        // Build Analysis Sheet
        BuildAnalysisSheet(analysisSheet);
        
        // Build Pivot Sheet
        BuildPivotSheet(pivotSheet);
        
        // Add a hidden configuration sheet
        var configSheet = ExcelHelper.AddWorksheet(workbook, "Config");
        BuildConfigSheet(configSheet);
        ExcelHelper.SetWorksheetVisibility(configSheet, XLWorksheetVisibility.Hidden);
        
        // Protect sensitive sheets
        ExcelHelper.ProtectWorksheet(configSheet, "admin123");
        
        // Set the active sheet to Summary
        summarySheet.SetTabActive();
        
        return ExcelHelper.ExportToBytes(workbook);
    }
    
    private void BuildSummarySheet(IXLWorksheet worksheet)
    {
        // Title
        var titleRange = ExcelHelper.MergeCells(worksheet, 1, 1, 2, 8);
        titleRange.Value = "Executive Dashboard - Q4 2024";
        ExcelHelper.CenterAlign(titleRange);
        ExcelHelper.SetFontSize(titleRange, 20);
        ExcelHelper.SetBackgroundColor(titleRange, Color.Navy);
        ExcelHelper.SetFontColor(titleRange, Color.White);
        
        // KPI Section
        int row = 4;
        string[] kpis = { "Total Revenue", "Growth Rate", "Customer Count", "Profit Margin" };
        string[] values = { "$1,250,000", "+15.3%", "8,542", "23.5%" };
        Color[] colors = { Color.Green, Color.Blue, Color.Orange, Color.Purple };
        
        for (int i = 0; i < kpis.Length; i++)
        {
            var kpiCell = worksheet.Cell(row, (i * 2) + 1);
            var valueCell = worksheet.Cell(row + 1, (i * 2) + 1);
            
            kpiCell.Value = kpis[i];
            ExcelHelper.SetFontStyle(worksheet.Range(row, (i * 2) + 1, row, (i * 2) + 1), bold: true);
            
            valueCell.Value = values[i];
            ExcelHelper.SetFontSize(worksheet.Range(row + 1, (i * 2) + 1, row + 1, (i * 2) + 1), 14);
            ExcelHelper.SetFontColor(worksheet.Range(row + 1, (i * 2) + 1, row + 1, (i * 2) + 1), colors[i]);
        }
        
        // Add notes section
        var notesHeader = worksheet.Cell(7, 1);
        notesHeader.Value = "Key Insights:";
        ExcelHelper.SetFontStyle(worksheet.Range(7, 1, 7, 1), bold: true);
        
        var notes = new[]
        {
            "• Revenue exceeded target by 12%",
            "• New customer acquisition up 25% YoY",
            "• Operating efficiency improved by 8%"
        };
        
        for (int i = 0; i < notes.Length; i++)
        {
            worksheet.Cell(8 + i, 1).Value = notes[i];
        }
        
        ExcelHelper.AutoFitColumns(worksheet);
    }
    
    private void BuildSalesSheet(IXLWorksheet worksheet)
    {
        // Create a detailed sales table
        var headers = new[] { "Date", "Product", "Quantity", "Unit Price", "Total", "Region", "Sales Rep" };
        ExcelHelper.CreateStyledTable(worksheet, headers, 1, 1);
        
        // Add sample data
        Random rand = new Random();
        string[] products = { "Widget A", "Widget B", "Gadget X", "Gadget Y" };
        string[] regions = { "North", "South", "East", "West" };
        string[] reps = { "John", "Sarah", "Mike", "Lisa" };
        
        for (int i = 0; i < 20; i++)
        {
            int row = i + 2;
            worksheet.Cell(row, 1).Value = DateTime.Now.AddDays(-rand.Next(1, 30));
            worksheet.Cell(row, 2).Value = products[rand.Next(products.Length)];
            worksheet.Cell(row, 3).Value = rand.Next(1, 100);
            worksheet.Cell(row, 4).Value = rand.Next(10, 200);
            worksheet.Cell(row, 5).FormulaA1 = $"=C{row}*D{row}";
            worksheet.Cell(row, 6).Value = regions[rand.Next(regions.Length)];
            worksheet.Cell(row, 7).Value = reps[rand.Next(reps.Length)];
        }
        
        // Format columns
        ExcelHelper.FormatAsDate(worksheet.Range(2, 1, 21, 1));
        ExcelHelper.FormatAsCurrency(worksheet.Range(2, 4, 21, 5));
        
        // Add subtotal row
        var totalRow = worksheet.Cell(23, 1);
        totalRow.Value = "TOTAL";
        ExcelHelper.SetFontStyle(worksheet.Range(23, 1, 23, 1), bold: true);
        worksheet.Cell(23, 5).FormulaA1 = "=SUM(E2:E21)";
        ExcelHelper.SetBackgroundColor(worksheet.Range(23, 1, 23, 7), Color.LightGray);
        
        // Apply alternating colors
        ExcelHelper.ApplyAlternatingRowColors(worksheet, 2, 21, 1, 7);
        
        ExcelHelper.AutoFitColumns(worksheet);
    }
    
    private void BuildChartsSheet(IXLWorksheet worksheet)
    {
        // Chart placeholder with instructions
        var titleRange = ExcelHelper.MergeCells(worksheet, 1, 1, 1, 6);
        titleRange.Value = "Charts & Visualizations";
        ExcelHelper.ApplyHeaderStyle(titleRange);
        
        // Data for charts
        worksheet.Cell(3, 1).Value = "Month";
        worksheet.Cell(3, 2).Value = "Revenue";
        worksheet.Cell(3, 3).Value = "Expenses";
        worksheet.Cell(3, 4).Value = "Profit";
        
        string[] months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };
        for (int i = 0; i < months.Length; i++)
        {
            worksheet.Cell(4 + i, 1).Value = months[i];
            worksheet.Cell(4 + i, 2).Value = 100000 + (i * 10000);
            worksheet.Cell(4 + i, 3).Value = 70000 + (i * 5000);
            worksheet.Cell(4 + i, 4).FormulaA1 = $"=B{4 + i}-C{4 + i}";
        }
        
        ExcelHelper.FormatAsCurrency(worksheet.Range(4, 2, 9, 4));
        
        // Note about charts
        var noteRange = ExcelHelper.MergeCells(worksheet, 11, 1, 12, 6);
        noteRange.Value = "Note: Charts can be added programmatically or manually in Excel after export.";
        ExcelHelper.CenterAlign(noteRange);
        ExcelHelper.SetBackgroundColor(noteRange, Color.LightYellow);
        ExcelHelper.WrapText(noteRange);
        
        ExcelHelper.AutoFitColumns(worksheet);
    }
    
    private void BuildSettingsSheet(IXLWorksheet worksheet)
    {
        // Configuration settings
        worksheet.Cell(1, 1).Value = "Report Configuration";
        ExcelHelper.SetFontStyle(worksheet.Range(1, 1, 1, 1), bold: true);
        
        worksheet.Cell(3, 1).Value = "Setting";
        worksheet.Cell(3, 2).Value = "Value";
        ExcelHelper.ApplyHeaderStyle(worksheet.Range(3, 1, 3, 2));
        
        var settings = new Dictionary<string, string>
        {
            ["Report Date"] = DateTime.Now.ToString("yyyy-MM-dd"),
            ["Generated By"] = "System Administrator",
            ["Data Source"] = "Production Database",
            ["Include Confidential"] = "No",
            ["Currency"] = "USD",
            ["Decimal Places"] = "2"
        };
        
        int row = 4;
        foreach (var setting in settings)
        {
            worksheet.Cell(row, 1).Value = setting.Key;
            worksheet.Cell(row, 2).Value = setting.Value;
            row++;
        }
        
        ExcelHelper.AddBorders(worksheet.Range(3, 1, row - 1, 2));
        ExcelHelper.AutoFitColumns(worksheet);
    }
    
    private void BuildExecutiveSummary(IXLWorksheet worksheet)
    {
        BuildSummarySheet(worksheet); // Reuse the summary builder
    }
    
    private void BuildRawDataSheet(IXLWorksheet worksheet)
    {
        // Create a large dataset
        var headers = new[] { "ID", "Date", "Category", "Amount", "Status", "Notes" };
        ExcelHelper.CreateStyledTable(worksheet, headers, 1, 1);
        
        // Add 100 rows of sample data
        Random rand = new Random();
        string[] categories = { "Sales", "Marketing", "Operations", "R&D" };
        string[] statuses = { "Completed", "Pending", "In Progress", "Cancelled" };
        
        for (int i = 1; i <= 100; i++)
        {
            int row = i + 1;
            worksheet.Cell(row, 1).Value = 1000 + i;
            worksheet.Cell(row, 2).Value = DateTime.Now.AddDays(-rand.Next(1, 365));
            worksheet.Cell(row, 3).Value = categories[rand.Next(categories.Length)];
            worksheet.Cell(row, 4).Value = rand.Next(1000, 50000);
            worksheet.Cell(row, 5).Value = statuses[rand.Next(statuses.Length)];
            worksheet.Cell(row, 6).Value = $"Transaction note {i}";
        }
        
        ExcelHelper.FormatAsDate(worksheet.Range(2, 2, 101, 2));
        ExcelHelper.FormatAsCurrency(worksheet.Range(2, 4, 101, 4));
        
        // Color code status column
        for (int i = 2; i <= 101; i++)
        {
            var statusCell = worksheet.Cell(i, 5);
            var status = statusCell.GetValue<string>();
            var color = status switch
            {
                "Completed" => Color.LightGreen,
                "Pending" => Color.LightYellow,
                "In Progress" => Color.LightBlue,
                "Cancelled" => Color.LightPink,
                _ => Color.White
            };
            ExcelHelper.SetBackgroundColor(worksheet.Range(i, 5, i, 5), color);
        }
        
        ExcelHelper.FreezePanes(worksheet, 2, 1);
        ExcelHelper.AutoFitColumns(worksheet);
    }
    
    private void BuildAnalysisSheet(IXLWorksheet worksheet)
    {
        // Analysis summary
        var title = ExcelHelper.MergeCells(worksheet, 1, 1, 1, 5);
        title.Value = "Data Analysis Summary";
        ExcelHelper.ApplyHeaderStyle(title);
        
        // Category breakdown
        worksheet.Cell(3, 1).Value = "Category Analysis";
        ExcelHelper.SetFontStyle(worksheet.Range(3, 1, 3, 1), bold: true);
        
        worksheet.Cell(4, 1).Value = "Category";
        worksheet.Cell(4, 2).Value = "Count";
        worksheet.Cell(4, 3).Value = "Total Amount";
        worksheet.Cell(4, 4).Value = "Average";
        worksheet.Cell(4, 5).Value = "Percentage";
        
        ExcelHelper.ApplyHeaderStyle(worksheet.Range(4, 1, 4, 5));
        
        // Add formulas referencing the Raw Data sheet
        string[] categories = { "Sales", "Marketing", "Operations", "R&D" };
        for (int i = 0; i < categories.Length; i++)
        {
            int row = 5 + i;
            worksheet.Cell(row, 1).Value = categories[i];
            worksheet.Cell(row, 2).FormulaA1 = $"=COUNTIF('Raw Data'!C:C,\"{categories[i]}\")";
            worksheet.Cell(row, 3).FormulaA1 = $"=SUMIF('Raw Data'!C:C,\"{categories[i]}\",'Raw Data'!D:D)";
            worksheet.Cell(row, 4).FormulaA1 = $"=C{row}/B{row}";
            worksheet.Cell(row, 5).FormulaA1 = $"=C{row}/SUM(C5:C8)";
        }
        
        ExcelHelper.FormatAsCurrency(worksheet.Range(5, 3, 8, 4));
        ExcelHelper.FormatAsPercentage(worksheet.Range(5, 5, 8, 5), 1);
        
        ExcelHelper.AddBorders(worksheet.Range(4, 1, 8, 5));
        ExcelHelper.AutoFitColumns(worksheet);
    }
    
    private void BuildPivotSheet(IXLWorksheet worksheet)
    {
        // Pivot table placeholder
        var title = ExcelHelper.MergeCells(worksheet, 1, 1, 1, 4);
        title.Value = "Pivot Table Analysis";
        ExcelHelper.ApplyHeaderStyle(title);
        
        worksheet.Cell(3, 1).Value = "Status";
        worksheet.Cell(3, 2).Value = "Q1";
        worksheet.Cell(3, 3).Value = "Q2";
        worksheet.Cell(3, 4).Value = "Q3";
        worksheet.Cell(3, 5).Value = "Q4";
        worksheet.Cell(3, 6).Value = "Total";
        
        ExcelHelper.ApplyHeaderStyle(worksheet.Range(3, 1, 3, 6));
        
        string[] statuses = { "Completed", "Pending", "In Progress", "Cancelled" };
        Random rand = new Random();
        
        for (int i = 0; i < statuses.Length; i++)
        {
            int row = 4 + i;
            worksheet.Cell(row, 1).Value = statuses[i];
            for (int q = 1; q <= 4; q++)
            {
                worksheet.Cell(row, q + 1).Value = rand.Next(10000, 50000);
            }
            worksheet.Cell(row, 6).FormulaA1 = $"=SUM(B{row}:E{row})";
        }
        
        // Add grand total row
        worksheet.Cell(8, 1).Value = "Grand Total";
        for (int col = 2; col <= 6; col++)
        {
            worksheet.Cell(8, col).FormulaA1 = $"=SUM({ExcelHelper.GetCellReference(4, col)}:{ExcelHelper.GetCellReference(7, col)})";
        }
        
        ExcelHelper.SetFontStyle(worksheet.Range(8, 1, 8, 6), bold: true);
        ExcelHelper.SetBackgroundColor(worksheet.Range(8, 1, 8, 6), Color.LightGray);
        
        ExcelHelper.FormatAsCurrency(worksheet.Range(4, 2, 8, 6));
        ExcelHelper.AddBorders(worksheet.Range(3, 1, 8, 6));
        ExcelHelper.AutoFitColumns(worksheet);
    }
    
    private void BuildConfigSheet(IXLWorksheet worksheet)
    {
        worksheet.Cell(1, 1).Value = "System Configuration (Hidden)";
        worksheet.Cell(2, 1).Value = "This sheet contains system settings and should remain hidden.";
        
        worksheet.Cell(4, 1).Value = "Parameter";
        worksheet.Cell(4, 2).Value = "Value";
        
        var configs = new Dictionary<string, string>
        {
            ["DatabaseConnection"] = "Server=localhost;Database=ReportDB",
            ["RefreshInterval"] = "3600",
            ["MaxRecords"] = "10000",
            ["EnableCache"] = "true"
        };
        
        int row = 5;
        foreach (var config in configs)
        {
            worksheet.Cell(row, 1).Value = config.Key;
            worksheet.Cell(row, 2).Value = config.Value;
            row++;
        }
        
        ExcelHelper.AddBorders(worksheet.Range(4, 1, row - 1, 2));
        ExcelHelper.AutoFitColumns(worksheet);
    }
    
    /// <summary>
    /// Example method showing how to use ExcelHelper to create a styled report
    /// </summary>
    public byte[] CreateSampleReport()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Sample Report");
        
        // 1. Create Title with merged cells
        var titleRange = ExcelHelper.MergeCells(worksheet, 1, 1, 2, 5);
        titleRange.Value = "Sales Report 2024";
        ExcelHelper.CenterAlign(titleRange);
        ExcelHelper.SetFontSize(titleRange, 18);
        ExcelHelper.SetFontStyle(titleRange, bold: true);
        ExcelHelper.SetBackgroundColor(titleRange, Color.Navy);
        ExcelHelper.SetFontColor(titleRange, Color.White);
        
        // 2. Create headers
        string[] headers = { "Product", "Q1 Sales", "Q2 Sales", "Q3 Sales", "Q4 Sales", "Total" };
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(4, i + 1);
            cell.Value = headers[i];
        }
        
        // Style headers
        var headerRange = worksheet.Range(4, 1, 4, headers.Length);
        ExcelHelper.ApplyHeaderStyle(headerRange);
        
        // 3. Add sample data
        var data = new[]
        {
            new { Product = "Product A", Q1 = 15000, Q2 = 18000, Q3 = 22000, Q4 = 25000 },
            new { Product = "Product B", Q1 = 12000, Q2 = 14000, Q3 = 16000, Q4 = 18000 },
            new { Product = "Product C", Q1 = 20000, Q2 = 22000, Q3 = 24000, Q4 = 26000 },
            new { Product = "Product D", Q1 = 8000, Q2 = 10000, Q3 = 12000, Q4 = 14000 }
        };
        
        int currentRow = 5;
        foreach (var item in data)
        {
            worksheet.Cell(currentRow, 1).Value = item.Product;
            worksheet.Cell(currentRow, 2).Value = item.Q1;
            worksheet.Cell(currentRow, 3).Value = item.Q2;
            worksheet.Cell(currentRow, 4).Value = item.Q3;
            worksheet.Cell(currentRow, 5).Value = item.Q4;
            worksheet.Cell(currentRow, 6).FormulaA1 = $"=SUM(B{currentRow}:E{currentRow})";
            currentRow++;
        }
        
        // 4. Format numbers as currency
        var dataRange = worksheet.Range(5, 2, currentRow - 1, 6);
        ExcelHelper.FormatAsCurrency(dataRange);
        
        // 5. Apply alternating row colors
        ExcelHelper.ApplyAlternatingRowColors(worksheet, 5, currentRow - 1, 1, 6);
        
        // 6. Add borders
        var tableRange = worksheet.Range(4, 1, currentRow - 1, 6);
        ExcelHelper.AddBorders(tableRange);
        
        // 7. Add totals row
        var totalRow = currentRow + 1;
        var totalLabelCell = worksheet.Cell(totalRow, 1);
        totalLabelCell.Value = "TOTAL";
        ExcelHelper.SetFontStyle(worksheet.Range(totalRow, 1, totalRow, 1), bold: true);
        
        for (int col = 2; col <= 6; col++)
        {
            var totalCell = worksheet.Cell(totalRow, col);
            totalCell.FormulaA1 = $"=SUM({ExcelHelper.GetCellReference(5, col)}:{ExcelHelper.GetCellReference(currentRow - 1, col)})";
        }
        
        var totalRange = worksheet.Range(totalRow, 1, totalRow, 6);
        ExcelHelper.SetBackgroundColor(totalRange, Color.LightGray);
        ExcelHelper.SetFontStyle(totalRange, bold: true);
        ExcelHelper.FormatAsCurrency(worksheet.Range(totalRow, 2, totalRow, 6));
        ExcelHelper.AddBorders(totalRange, XLBorderStyleValues.Medium);
        
        // 8. Auto-fit columns
        ExcelHelper.AutoFitColumns(worksheet);
        
        // 9. Freeze header row
        ExcelHelper.FreezePanes(worksheet, 5, 1);
        
        // Convert to byte array
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
    
    /// <summary>
    /// Example showing advanced features
    /// </summary>
    public byte[] CreateAdvancedReport()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Advanced Report");
        
        // 1. Complex merged header
        var mainHeader = ExcelHelper.MergeCells(worksheet, 1, 1, 1, 8);
        mainHeader.Value = "Company Performance Dashboard";
        ExcelHelper.CenterAlign(mainHeader);
        ExcelHelper.SetFontSize(mainHeader, 20);
        ExcelHelper.SetBackgroundColor(mainHeader, 0, 51, 102); // Dark Blue
        ExcelHelper.SetFontColor(mainHeader, Color.White);
        
        // 2. Sub-headers with different merges
        var subHeader1 = ExcelHelper.MergeCells(worksheet, 2, 1, 2, 4);
        subHeader1.Value = "Revenue Metrics";
        ExcelHelper.CenterAlign(subHeader1);
        ExcelHelper.SetBackgroundColor(subHeader1, Color.LightBlue);
        
        var subHeader2 = ExcelHelper.MergeCells(worksheet, 2, 5, 2, 8);
        subHeader2.Value = "Performance Indicators";
        ExcelHelper.CenterAlign(subHeader2);
        ExcelHelper.SetBackgroundColor(subHeader2, Color.LightGreen);
        
        // 3. Vertical merge example
        var verticalMerge = ExcelHelper.MergeColumn(worksheet, 1, 4, 8);
        verticalMerge.Value = "2024";
        ExcelHelper.CenterAlign(verticalMerge);
        ExcelHelper.SetFontStyle(verticalMerge, bold: true);
        worksheet.Cell(4, 1).Style.Alignment.TextRotation = 90; // Rotate text
        
        // 4. Create data with conditional formatting
        var headers = new[] { "Year", "Month", "Revenue", "Target", "Growth %", "Status", "Rating", "Notes" };
        ExcelHelper.CreateStyledTable(worksheet, headers, 3, 1);
        
        // 5. Add sample data with various formatting
        string[] months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun" };
        Random rand = new Random();
        
        for (int i = 0; i < months.Length; i++)
        {
            int row = 4 + i;
            worksheet.Cell(row, 1).Value = "2024";
            worksheet.Cell(row, 2).Value = months[i];
            
            double revenue = rand.Next(50000, 150000);
            double target = rand.Next(60000, 140000);
            double growth = (revenue - target) / target * 100;
            
            worksheet.Cell(row, 3).Value = revenue;
            worksheet.Cell(row, 4).Value = target;
            worksheet.Cell(row, 5).Value = growth;
            
            // Status with color coding
            var statusCell = worksheet.Cell(row, 6);
            if (revenue >= target)
            {
                statusCell.Value = "Achieved";
                ExcelHelper.SetBackgroundColor(worksheet.Range(row, 6, row, 6), Color.LightGreen);
            }
            else
            {
                statusCell.Value = "Missed";
                ExcelHelper.SetBackgroundColor(worksheet.Range(row, 6, row, 6), Color.LightPink);
            }
            
            // Rating with stars
            int rating = rand.Next(1, 6);
            worksheet.Cell(row, 7).Value = new string('★', rating) + new string('☆', 5 - rating);
            
            // Add dropdown for Notes
            ExcelHelper.AddDropdownList(worksheet.Cell(row, 8), "Good", "Average", "Needs Improvement");
        }
        
        // 6. Format columns
        ExcelHelper.FormatAsCurrency(worksheet.Range(4, 3, 9, 4));
        ExcelHelper.FormatAsPercentage(worksheet.Range(4, 5, 9, 5), 1);
        
        // 7. Add conditional formatting for growth percentage
        var growthRange = worksheet.Range(4, 5, 9, 5);
        ExcelHelper.AddConditionalFormatting(growthRange, 0, 
            XLColor.FromColor(Color.LightGreen), 
            XLColor.FromColor(Color.LightPink));
        
        // 8. Add comment to a cell
        ExcelHelper.AddComment(worksheet.Cell(4, 3), "Q1 Revenue Data", "Finance Team");
        
        // 9. Set column widths
        ExcelHelper.SetColumnWidth(worksheet, 1, 8);
        ExcelHelper.SetColumnWidth(worksheet, 2, 10);
        ExcelHelper.SetColumnWidth(worksheet, 3, 15);
        ExcelHelper.SetColumnWidth(worksheet, 4, 15);
        ExcelHelper.SetColumnWidth(worksheet, 5, 12);
        ExcelHelper.SetColumnWidth(worksheet, 6, 12);
        ExcelHelper.SetColumnWidth(worksheet, 7, 15);
        ExcelHelper.SetColumnWidth(worksheet, 8, 20);
        
        // 10. Add a summary section with different alignments
        int summaryRow = 11;
        var summaryHeader = ExcelHelper.MergeCells(worksheet, summaryRow, 1, summaryRow, 8);
        summaryHeader.Value = "Summary Statistics";
        ExcelHelper.CenterAlign(summaryHeader);
        ExcelHelper.SetBackgroundColor(summaryHeader, Color.Gray);
        ExcelHelper.SetFontColor(summaryHeader, Color.White);
        
        // Different alignment examples
        worksheet.Cell(summaryRow + 1, 1).Value = "Left Aligned";
        ExcelHelper.LeftAlign(worksheet.Range(summaryRow + 1, 1, summaryRow + 1, 1));
        
        worksheet.Cell(summaryRow + 1, 3).Value = "Center Aligned";
        ExcelHelper.CenterAlign(worksheet.Range(summaryRow + 1, 3, summaryRow + 1, 3));
        
        worksheet.Cell(summaryRow + 1, 5).Value = "Right Aligned";
        ExcelHelper.RightAlign(worksheet.Range(summaryRow + 1, 5, summaryRow + 1, 5));
        
        // Convert to byte array
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}