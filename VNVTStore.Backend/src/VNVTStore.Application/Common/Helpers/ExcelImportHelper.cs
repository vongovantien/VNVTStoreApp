using System.IO;
using System.Collections.Generic;
using OfficeOpenXml;
using System.Linq;
using System.Reflection;
using System;

namespace VNVTStore.Application.Common.Helpers;

public static class ExcelImportHelper
{
    public static IEnumerable<T> Import<T>(Stream stream) where T : class, new()
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        using (var package = new ExcelPackage(stream))
        {
            var worksheet = package.Workbook.Worksheets[0];
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var headerMap = new Dictionary<string, int>();

            var rowCount = worksheet.Dimension?.Rows ?? 0;
            if (rowCount == 0) yield break;

            // Map headers
            for (int col = 1; col <= worksheet.Dimension.Columns; col++)
            {
                var header = worksheet.Cells[1, col].Text;
                if (!string.IsNullOrEmpty(header))
                {
                    headerMap[header.Trim()] = col;
                }
            }

            for (int row = 2; row <= rowCount; row++)
            {
                var obj = new T();
                foreach (var prop in properties)
                {
                    // Assume property name matches header name, or could use an attribute
                    if (headerMap.TryGetValue(prop.Name, out int colIndex))
                    {
                        var val = worksheet.Cells[row, colIndex].Value;
                        if (val != null)
                        {
                            try {
                                if (prop.PropertyType == typeof(string))
                                    prop.SetValue(obj, val?.ToString());
                                else if (prop.PropertyType == typeof(int) || prop.PropertyType == typeof(int?))
                                    prop.SetValue(obj, Convert.ToInt32(val));
                                else if (prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?))
                                    prop.SetValue(obj, Convert.ToDecimal(val));
                                else if (prop.PropertyType == typeof(double) || prop.PropertyType == typeof(double?))
                                    prop.SetValue(obj, Convert.ToDouble(val));
                                else if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                                {
                                     // Handle Date/Double conversion if needed, EPPlus usually helps but safe cast
                                     if(val is DateTime dt) prop.SetValue(obj, dt);
                                     else if(val is double d) prop.SetValue(obj, DateTime.FromOADate(d));
                                     else if(DateTime.TryParse(val.ToString(), out var parsedDt)) prop.SetValue(obj, parsedDt);
                                }
                                else if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?))
                                    prop.SetValue(obj, Convert.ToBoolean(val));
                            } catch {} // Simple mapping, ignore failures
                        }
                    }
                }
                yield return obj;
            }
        }
    }
}
