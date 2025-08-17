using ClosedXML.Excel;
using System.Drawing;

namespace Nexora.Application.Helpers;

public static class ExcelHelper
{
    #region Workbook and Worksheet Management
    
    /// <summary>
    /// Create a new workbook with optional initial worksheet
    /// </summary>
    public static XLWorkbook CreateWorkbook(string? initialSheetName = null)
    {
        var workbook = new XLWorkbook();
        if (!string.IsNullOrEmpty(initialSheetName))
        {
            workbook.Worksheets.Add(initialSheetName);
        }
        return workbook;
    }
    
    /// <summary>
    /// Add a new worksheet to existing workbook
    /// </summary>
    public static IXLWorksheet AddWorksheet(XLWorkbook workbook, string sheetName)
    {
        return workbook.Worksheets.Add(sheetName);
    }
    
    /// <summary>
    /// Add multiple worksheets to a workbook
    /// </summary>
    public static void AddWorksheets(XLWorkbook workbook, params string[] sheetNames)
    {
        foreach (var sheetName in sheetNames)
        {
            workbook.Worksheets.Add(sheetName);
        }
    }
    
    /// <summary>
    /// Get worksheet by name or index
    /// </summary>
    public static IXLWorksheet? GetWorksheet(XLWorkbook workbook, string sheetName)
    {
        return workbook.Worksheets.FirstOrDefault(ws => ws.Name == sheetName);
    }
    
    /// <summary>
    /// Get worksheet by index (1-based)
    /// </summary>
    public static IXLWorksheet? GetWorksheet(XLWorkbook workbook, int index)
    {
        return workbook.Worksheets.ElementAtOrDefault(index - 1);
    }
    
    /// <summary>
    /// Rename a worksheet
    /// </summary>
    public static void RenameWorksheet(IXLWorksheet worksheet, string newName)
    {
        worksheet.Name = newName;
    }
    
    /// <summary>
    /// Delete a worksheet by name
    /// </summary>
    public static bool DeleteWorksheet(XLWorkbook workbook, string sheetName)
    {
        var worksheet = workbook.Worksheets.FirstOrDefault(ws => ws.Name == sheetName);
        if (worksheet != null)
        {
            worksheet.Delete();
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// Copy worksheet to same or different workbook
    /// </summary>
    public static IXLWorksheet CopyWorksheet(IXLWorksheet sourceWorksheet, XLWorkbook targetWorkbook, string newName)
    {
        return sourceWorksheet.CopyTo(targetWorkbook, newName);
    }
    
    /// <summary>
    /// Move worksheet to a specific position
    /// </summary>
    public static void MoveWorksheet(IXLWorksheet worksheet, int position)
    {
        worksheet.Position = position;
    }
    
    /// <summary>
    /// Set worksheet tab color
    /// </summary>
    public static void SetTabColor(IXLWorksheet worksheet, XLColor color)
    {
        worksheet.TabColor = color;
    }
    
    /// <summary>
    /// Hide or show a worksheet
    /// </summary>
    public static void SetWorksheetVisibility(IXLWorksheet worksheet, XLWorksheetVisibility visibility)
    {
        worksheet.Visibility = visibility;
    }
    
    /// <summary>
    /// Protect worksheet with optional password
    /// </summary>
    public static void ProtectWorksheet(IXLWorksheet worksheet, string? password = null)
    {
        if (!string.IsNullOrEmpty(password))
        {
            worksheet.Protect(password);
        }
        else
        {
            worksheet.Protect();
        }
    }
    
    /// <summary>
    /// Create a workbook with multiple styled sheets from data
    /// </summary>
    public static XLWorkbook CreateMultiSheetWorkbook(Dictionary<string, Action<IXLWorksheet>> sheetBuilders)
    {
        var workbook = new XLWorkbook();
        
        foreach (var sheetBuilder in sheetBuilders)
        {
            var worksheet = workbook.Worksheets.Add(sheetBuilder.Key);
            sheetBuilder.Value(worksheet);
        }
        
        return workbook;
    }
    
    /// <summary>
    /// Export workbook to byte array
    /// </summary>
    public static byte[] ExportToBytes(XLWorkbook workbook)
    {
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
    
    /// <summary>
    /// Save workbook to file
    /// </summary>
    public static void SaveToFile(XLWorkbook workbook, string filePath)
    {
        workbook.SaveAs(filePath);
    }
    
    #endregion
    
    #region Cell Merging Methods
    
    /// <summary>
    /// Merge cells in a range
    /// </summary>
    public static IXLRange MergeCells(IXLWorksheet worksheet, int startRow, int startCol, int endRow, int endCol)
    {
        var range = worksheet.Range(startRow, startCol, endRow, endCol);
        range.Merge();
        return range;
    }
    
    /// <summary>
    /// Merge cells using cell addresses (e.g., "A1:C3")
    /// </summary>
    public static IXLRange MergeCells(IXLWorksheet worksheet, string rangeAddress)
    {
        var range = worksheet.Range(rangeAddress);
        range.Merge();
        return range;
    }
    
    /// <summary>
    /// Merge cells horizontally in a row
    /// </summary>
    public static IXLRange MergeRow(IXLWorksheet worksheet, int row, int startCol, int endCol)
    {
        return MergeCells(worksheet, row, startCol, row, endCol);
    }
    
    /// <summary>
    /// Merge cells vertically in a column
    /// </summary>
    public static IXLRange MergeColumn(IXLWorksheet worksheet, int col, int startRow, int endRow)
    {
        return MergeCells(worksheet, startRow, col, endRow, col);
    }
    
    #endregion
    
    #region Alignment Methods
    
    /// <summary>
    /// Set horizontal alignment for a cell or range
    /// </summary>
    public static void SetHorizontalAlignment(IXLRange range, XLAlignmentHorizontalValues alignment)
    {
        range.Style.Alignment.Horizontal = alignment;
    }
    
    /// <summary>
    /// Set vertical alignment for a cell or range
    /// </summary>
    public static void SetVerticalAlignment(IXLRange range, XLAlignmentVerticalValues alignment)
    {
        range.Style.Alignment.Vertical = alignment;
    }
    
    /// <summary>
    /// Center align text both horizontally and vertically
    /// </summary>
    public static void CenterAlign(IXLRange range)
    {
        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
    }
    
    /// <summary>
    /// Left align text
    /// </summary>
    public static void LeftAlign(IXLRange range)
    {
        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
    }
    
    /// <summary>
    /// Right align text
    /// </summary>
    public static void RightAlign(IXLRange range)
    {
        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
    }
    
    /// <summary>
    /// Top align text
    /// </summary>
    public static void TopAlign(IXLRange range)
    {
        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
    }
    
    /// <summary>
    /// Bottom align text
    /// </summary>
    public static void BottomAlign(IXLRange range)
    {
        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
    }
    
    #endregion
    
    #region Color and Styling Methods
    
    /// <summary>
    /// Set background color for a cell or range
    /// </summary>
    public static void SetBackgroundColor(IXLRange range, XLColor color)
    {
        range.Style.Fill.BackgroundColor = color;
    }
    
    /// <summary>
    /// Set background color using RGB values
    /// </summary>
    public static void SetBackgroundColor(IXLRange range, int red, int green, int blue)
    {
        range.Style.Fill.BackgroundColor = XLColor.FromArgb(red, green, blue);
    }
    
    /// <summary>
    /// Set background color using System.Drawing.Color
    /// </summary>
    public static void SetBackgroundColor(IXLRange range, Color color)
    {
        range.Style.Fill.BackgroundColor = XLColor.FromColor(color);
    }
    
    /// <summary>
    /// Set font color for a cell or range
    /// </summary>
    public static void SetFontColor(IXLRange range, XLColor color)
    {
        range.Style.Font.FontColor = color;
    }
    
    /// <summary>
    /// Set font color using RGB values
    /// </summary>
    public static void SetFontColor(IXLRange range, int red, int green, int blue)
    {
        range.Style.Font.FontColor = XLColor.FromArgb(red, green, blue);
    }
    
    /// <summary>
    /// Set font color using System.Drawing.Color
    /// </summary>
    public static void SetFontColor(IXLRange range, Color color)
    {
        range.Style.Font.FontColor = XLColor.FromColor(color);
    }
    
    /// <summary>
    /// Set font style (bold, italic, underline)
    /// </summary>
    public static void SetFontStyle(IXLRange range, bool bold = false, bool italic = false, bool underline = false)
    {
        if (bold) range.Style.Font.Bold = true;
        if (italic) range.Style.Font.Italic = true;
        if (underline) range.Style.Font.Underline = XLFontUnderlineValues.Single;
    }
    
    /// <summary>
    /// Set font size
    /// </summary>
    public static void SetFontSize(IXLRange range, double size)
    {
        range.Style.Font.FontSize = size;
    }
    
    /// <summary>
    /// Set font family
    /// </summary>
    public static void SetFontFamily(IXLRange range, string fontFamily)
    {
        range.Style.Font.FontName = fontFamily;
    }
    
    #endregion
    
    #region Border Methods
    
    /// <summary>
    /// Add borders to a range
    /// </summary>
    public static void AddBorders(IXLRange range, XLBorderStyleValues style = XLBorderStyleValues.Thin)
    {
        range.Style.Border.OutsideBorder = style;
        range.Style.Border.InsideBorder = style;
    }
    
    /// <summary>
    /// Add outside border only
    /// </summary>
    public static void AddOutsideBorder(IXLRange range, XLBorderStyleValues style = XLBorderStyleValues.Thin)
    {
        range.Style.Border.OutsideBorder = style;
    }
    
    /// <summary>
    /// Add inside borders only
    /// </summary>
    public static void AddInsideBorders(IXLRange range, XLBorderStyleValues style = XLBorderStyleValues.Thin)
    {
        range.Style.Border.InsideBorder = style;
    }
    
    /// <summary>
    /// Set border color
    /// </summary>
    public static void SetBorderColor(IXLRange range, XLColor color)
    {
        range.Style.Border.TopBorderColor = color;
        range.Style.Border.BottomBorderColor = color;
        range.Style.Border.LeftBorderColor = color;
        range.Style.Border.RightBorderColor = color;
    }
    
    #endregion
    
    #region Formatting Methods
    
    /// <summary>
    /// Format cells as currency
    /// </summary>
    public static void FormatAsCurrency(IXLRange range, string currencySymbol = "$")
    {
        range.Style.NumberFormat.Format = $"{currencySymbol}#,##0.00";
    }
    
    /// <summary>
    /// Format cells as percentage
    /// </summary>
    public static void FormatAsPercentage(IXLRange range, int decimalPlaces = 2)
    {
        range.Style.NumberFormat.Format = $"0.{new string('0', decimalPlaces)}%";
    }
    
    /// <summary>
    /// Format cells as date
    /// </summary>
    public static void FormatAsDate(IXLRange range, string format = "MM/dd/yyyy")
    {
        range.Style.NumberFormat.Format = format;
    }
    
    /// <summary>
    /// Format cells as number with decimal places
    /// </summary>
    public static void FormatAsNumber(IXLRange range, int decimalPlaces = 2)
    {
        range.Style.NumberFormat.Format = decimalPlaces > 0 
            ? $"#,##0.{new string('0', decimalPlaces)}" 
            : "#,##0";
    }
    
    /// <summary>
    /// Wrap text in cells
    /// </summary>
    public static void WrapText(IXLRange range)
    {
        range.Style.Alignment.WrapText = true;
    }
    
    #endregion
    
    #region Column and Row Operations
    
    /// <summary>
    /// Auto-fit column width
    /// </summary>
    public static void AutoFitColumns(IXLWorksheet worksheet)
    {
        worksheet.Columns().AdjustToContents();
    }
    
    /// <summary>
    /// Auto-fit specific columns
    /// </summary>
    public static void AutoFitColumns(IXLWorksheet worksheet, int startCol, int endCol)
    {
        worksheet.Columns(startCol, endCol).AdjustToContents();
    }
    
    /// <summary>
    /// Set column width
    /// </summary>
    public static void SetColumnWidth(IXLWorksheet worksheet, int column, double width)
    {
        worksheet.Column(column).Width = width;
    }
    
    /// <summary>
    /// Set row height
    /// </summary>
    public static void SetRowHeight(IXLWorksheet worksheet, int row, double height)
    {
        worksheet.Row(row).Height = height;
    }
    
    /// <summary>
    /// Hide column
    /// </summary>
    public static void HideColumn(IXLWorksheet worksheet, int column)
    {
        worksheet.Column(column).Hide();
    }
    
    /// <summary>
    /// Hide row
    /// </summary>
    public static void HideRow(IXLWorksheet worksheet, int row)
    {
        worksheet.Row(row).Hide();
    }
    
    #endregion
    
    #region Advanced Styling Methods
    
    /// <summary>
    /// Apply header style to a range
    /// </summary>
    public static void ApplyHeaderStyle(IXLRange range)
    {
        range.Style.Font.Bold = true;
        range.Style.Fill.BackgroundColor = XLColor.FromArgb(70, 130, 180); // Steel Blue
        range.Style.Font.FontColor = XLColor.White;
        range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
        range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    }
    
    /// <summary>
    /// Apply alternating row colors (zebra striping)
    /// </summary>
    public static void ApplyAlternatingRowColors(IXLWorksheet worksheet, int startRow, int endRow, int startCol, int endCol)
    {
        for (int row = startRow; row <= endRow; row++)
        {
            if (row % 2 == 0)
            {
                var range = worksheet.Range(row, startCol, row, endCol);
                range.Style.Fill.BackgroundColor = XLColor.FromArgb(240, 240, 240); // Light Gray
            }
        }
    }
    
    /// <summary>
    /// Create a styled table with headers
    /// </summary>
    public static void CreateStyledTable(IXLWorksheet worksheet, string[] headers, int startRow = 1, int startCol = 1)
    {
        // Add headers
        for (int i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(startRow, startCol + i);
            cell.Value = headers[i];
        }
        
        // Style header row
        var headerRange = worksheet.Range(startRow, startCol, startRow, startCol + headers.Length - 1);
        ApplyHeaderStyle(headerRange);
        
        // Auto-fit columns
        worksheet.Columns(startCol, startCol + headers.Length - 1).AdjustToContents();
    }
    
    /// <summary>
    /// Highlight cells based on condition
    /// </summary>
    public static void HighlightCellsWithValue(IXLWorksheet worksheet, string value, XLColor highlightColor)
    {
        var cells = worksheet.CellsUsed();
        foreach (var cell in cells)
        {
            if (cell.GetValue<string>() == value)
            {
                cell.Style.Fill.BackgroundColor = highlightColor;
            }
        }
    }
    
    /// <summary>
    /// Add conditional formatting for numeric values
    /// </summary>
    public static void AddConditionalFormatting(IXLRange range, double threshold, XLColor colorAbove, XLColor colorBelow)
    {
        foreach (var cell in range.Cells())
        {
            if (cell.TryGetValue<double>(out double value))
            {
                cell.Style.Fill.BackgroundColor = value >= threshold ? colorAbove : colorBelow;
            }
        }
    }
    
    #endregion
    
    #region Utility Methods
    
    /// <summary>
    /// Get cell reference (e.g., "A1") from row and column numbers
    /// </summary>
    public static string GetCellReference(int row, int column)
    {
        return XLHelper.GetColumnLetterFromNumber(column) + row;
    }
    
    /// <summary>
    /// Clear formatting from a range
    /// </summary>
    public static void ClearFormatting(IXLRange range)
    {
        range.Style = XLWorkbook.DefaultStyle;
    }
    
    /// <summary>
    /// Copy formatting from one range to another
    /// </summary>
    public static void CopyFormatting(IXLRange sourceRange, IXLRange targetRange)
    {
        targetRange.Style = sourceRange.Style;
    }
    
    /// <summary>
    /// Freeze panes at specific cell
    /// </summary>
    public static void FreezePanes(IXLWorksheet worksheet, int row, int column)
    {
        worksheet.SheetView.FreezeRows(row - 1);
        worksheet.SheetView.FreezeColumns(column - 1);
    }
    
    /// <summary>
    /// Add dropdown list validation to a cell
    /// </summary>
    public static void AddDropdownList(IXLCell cell, params string[] options)
    {
        cell.CreateDataValidation().List(string.Join(",", options));
    }
    
    /// <summary>
    /// Add a comment to a cell
    /// </summary>
    public static void AddComment(IXLCell cell, string comment, string author = "System")
    {
        cell.CreateComment().AddText($"{author}: {comment}");
    }
    
    #endregion
}