using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace VNVTStore.Application.Common.Helpers;

public static class ExcelExportHelper
{
    public static byte[] GenerateTemplate<T>() where T : class
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Template");

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        for (int i = 0; i < properties.Length; i++)
        {
            worksheet.Cells[1, i + 1].Value = properties[i].Name;
        }

        worksheet.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }

    public static byte[] ExportToExcel<T>(IEnumerable<T> data, string sheetName = "Data") where T : class
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add(sheetName);

        var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
        
        // Headers
        for (int i = 0; i < properties.Length; i++)
        {
            worksheet.Cells[1, i + 1].Value = properties[i].Name;
            worksheet.Cells[1, i + 1].Style.Font.Bold = true;
        }

        // Data
        var dataList = data.ToList();
        for (int row = 0; row < dataList.Count; row++)
        {
            for (int col = 0; col < properties.Length; col++)
            {
                worksheet.Cells[row + 2, col + 1].Value = properties[col].GetValue(dataList[row]);
            }
        }

        worksheet.Cells.AutoFitColumns();
        return package.GetAsByteArray();
    }
}
