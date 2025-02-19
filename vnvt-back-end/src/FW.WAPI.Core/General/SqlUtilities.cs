using FW.WAPI.Core.DAL.DTO;
using FW.WAPI.Core.DAL.Model.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FW.WAPI.Core.General
{
    /// <summary>
    /// Author: ThuanND
    /// Created Date:
    /// Description:
    /// </summary>

    public static class SqlUtilities
    {
        //public static string QUOTE = "\"";
        /// <summary>
        ///
        /// </summary>
        /// <param name="SearchFieldList"></param>
        /// <returns></returns>
        /*
        public static string BuildRawQueryCondition(List<SearchDTO> SearchFieldList, string tableName = null)
        {
            string rawQuery = "";

            List<SearchDTO> searchGroup = SearchFieldList.Where(x => x.GroupID.HasValue).ToList();

            if (searchGroup.Count > 0)
            {
                var groups = searchGroup.GroupBy(x => x.GroupID).Select(x => x.Key);

                foreach (var group in groups)
                {
                    var searchConditionGroup = searchGroup.Where(x => x.GroupID == group);

                    foreach (var itemGroup in searchConditionGroup)
                    {

                        if ((!string.IsNullOrEmpty(rawQuery)) && (string.IsNullOrEmpty(itemGroup.CombineCondition)))
                        {
                            rawQuery += " AND ";
                        }
                        else
                        {
                            rawQuery += " " + itemGroup.CombineCondition + " ";
                        }

                        bool isArray = JsonUtilities.CheckJsonArray(itemGroup.SearchValue);

                        switch (itemGroup.SearchCondition)
                        {
                            case SearchCondition.IsNull:
                                rawQuery += itemGroup.SearchField + " IS NULL ";
                                break;

                            case SearchCondition.In:

                                if (isArray)
                                {
                                    var arrayValueEqual = ((JArray)itemGroup.SearchValue).ToObject<object[]>();

                                    var inVal = "";

                                    for (int i = 0; i < arrayValueEqual.Length; i++)
                                    {
                                        inVal += "'" + arrayValueEqual[i] + "',";
                                    }

                                    inVal = inVal.Substring(0, inVal.Length - 1);

                                    rawQuery += string.IsNullOrEmpty(tableName) ? "" + itemGroup.SearchField + " IN (" + inVal + ") " :
                                        tableName + "." + itemGroup.SearchField + " IN (" + inVal + ") ";
                                }
                                else
                                {
                                    rawQuery += String.IsNullOrEmpty(tableName) ? "" + itemGroup.SearchField + " IN ('" + itemGroup.SearchValue + "') " :
                                        tableName + "." + itemGroup.SearchField + " IN ('" + itemGroup.SearchValue + "') ";
                                }

                                break;

                            case SearchCondition.IsNotNull:
                                rawQuery += itemGroup.SearchField + " IS NOT NULL ";
                                break;

                            case SearchCondition.Equal:

                                if (isArray)
                                {
                                    var arrayValueEqual = ((JArray)itemGroup.SearchValue).ToObject<object[]>();
                                    var subQueryEqual = "";

                                    for (int i = 0; i < arrayValueEqual.Length; i++)
                                    {
                                        var type = arrayValueEqual[i].GetType().FullName;
                                        switch (type)
                                        {
                                            case "System.String":

                                                if (i > 0)
                                                {
                                                    subQueryEqual += tableName == null ? " OR " + itemGroup.SearchField + " = " + "N'" + arrayValueEqual[i] + "' " :
                                                        " OR " + tableName + "." + itemGroup.SearchField + " = " + "N'" + arrayValueEqual[i] + "' ";
                                                }
                                                else
                                                {
                                                    if (string.IsNullOrEmpty(tableName))
                                                    {
                                                        subQueryEqual += itemGroup.SearchField + " = " + "N'" + arrayValueEqual[i] + "'";
                                                    }
                                                    else
                                                    {
                                                        subQueryEqual += tableName + "." + itemGroup.SearchField + " = " + "N'" + arrayValueEqual[i] + "'";
                                                    }
                                                }

                                                break;

                                            default:
                                                break;
                                        }
                                    }

                                    subQueryEqual = "( " + subQueryEqual + " )";
                                    rawQuery = rawQuery + subQueryEqual;
                                }
                                else
                                {
                                    var type = itemGroup.SearchValue.GetType().FullName;

                                    if (type == "System.DateTime")
                                    {
                                        if (string.IsNullOrEmpty(tableName))
                                        {
                                            rawQuery += itemGroup.SearchField + " = " + "convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                                System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                        }
                                        else
                                        {
                                            rawQuery += tableName + "." + itemGroup.SearchField + " = " + "convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                                System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                        }
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(tableName))
                                        {
                                            rawQuery += itemGroup.SearchField + " = " + "'" + itemGroup.SearchValue + "'";
                                        }
                                        else
                                        {
                                            rawQuery += tableName + "." + itemGroup.SearchField + " = " + "'" + itemGroup.SearchValue + "'";
                                        }
                                    }
                                }

                                break;

                            case SearchCondition.NotEqual:

                                if (isArray)
                                {
                                    var arrayValueEqual = ((JArray)itemGroup.SearchValue).ToObject<object[]>();
                                    var subQueryEqual = "";

                                    for (int i = 0; i < arrayValueEqual.Length; i++)
                                    {
                                        var type = arrayValueEqual[i].GetType().FullName;
                                        switch (type)
                                        {
                                            case "System.String":

                                                if (i > 0)
                                                {
                                                    if (string.IsNullOrEmpty(tableName))
                                                    {
                                                        subQueryEqual += " OR " + itemGroup.SearchField + " != " + "N'" + arrayValueEqual[i] + "' ";
                                                    }
                                                    else
                                                    {
                                                        subQueryEqual += " OR " + tableName + "." + itemGroup.SearchField + " != " + "N'" + arrayValueEqual[i] + "' ";
                                                    }
                                                }
                                                else
                                                {
                                                    if (string.IsNullOrEmpty(tableName))
                                                    {
                                                        subQueryEqual += itemGroup.SearchField + " != " + "N'" + arrayValueEqual[i] + "'";
                                                    }
                                                    else
                                                    {
                                                        subQueryEqual += tableName + "." + itemGroup.SearchField + " != " + "N'" + arrayValueEqual[i] + "'";
                                                    }
                                                }

                                                break;

                                            default:
                                                break;
                                        }
                                    }

                                    subQueryEqual = "( " + subQueryEqual + " )";
                                    rawQuery = rawQuery + subQueryEqual;
                                }
                                else
                                {
                                    var type = itemGroup.SearchValue.GetType().FullName;

                                    if (type == "System.DateTime")
                                    {
                                        if (string.IsNullOrEmpty(tableName))
                                        {
                                            rawQuery += itemGroup.SearchField + " != " + "convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                                System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                        }
                                        else
                                        {
                                            rawQuery += tableName + "." + itemGroup.SearchField + " != " + "convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                                System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                        }
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(tableName))
                                        {
                                            rawQuery += itemGroup.SearchField + " != " + "'" + itemGroup.SearchValue + "'";
                                        }
                                        else
                                        {
                                            rawQuery += tableName + "." + itemGroup.SearchField + " != " + "'" + itemGroup.SearchValue + "'";
                                        }
                                    }
                                }

                                break;

                            case SearchCondition.Contains:

                                if (isArray)
                                {
                                    var arrayValueEqual = ((JArray)itemGroup.SearchValue).ToObject<object[]>();
                                    var subQueryEqual = "";

                                    for (int i = 0; i < arrayValueEqual.Length; i++)
                                    {
                                        if (i > 0)
                                        {
                                            if (string.IsNullOrEmpty(tableName))
                                            {
                                                subQueryEqual += " OR " + itemGroup.SearchField + " LIKE N'%" + arrayValueEqual[i] + "%' ";
                                            }
                                            else
                                            {
                                                subQueryEqual += " OR " + tableName + "." + itemGroup.SearchField + " LIKE N'%" + arrayValueEqual[i] + "%' ";
                                            }
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(tableName))
                                            {
                                                subQueryEqual += itemGroup.SearchField + " LIKE N'%" + arrayValueEqual[i] + "%'";
                                            }
                                            else
                                            {
                                                subQueryEqual += tableName + "." + itemGroup.SearchField + " LIKE N'%" + arrayValueEqual[i] + "%'";
                                            }
                                        }
                                    }

                                    subQueryEqual = "( " + subQueryEqual + " )";
                                    rawQuery = rawQuery + subQueryEqual;
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        rawQuery += itemGroup.SearchField + " LIKE N'%" + itemGroup.SearchValue + "%'";
                                    }
                                    else
                                    {
                                        rawQuery += tableName + "." + itemGroup.SearchField + " LIKE N'%" + itemGroup.SearchValue + "%'";
                                    }
                                }

                                break;

                            case SearchCondition.GreaterThan:

                                var typeGreaterThan = itemGroup.SearchValue.GetType().FullName;

                                if (typeGreaterThan == "System.DateTime")
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        rawQuery += itemGroup.SearchField + " > " + "convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }
                                    else
                                    {
                                        rawQuery += tableName + "." + itemGroup.SearchField + " > " + "convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        rawQuery += itemGroup.SearchField + " > " + itemGroup.SearchValue;
                                    }
                                    else
                                    {
                                        rawQuery += tableName + "." + itemGroup.SearchField + " > " + itemGroup.SearchValue;
                                    }
                                }

                                break;

                            case SearchCondition.GreaterThanEqual:

                                var typeGreaterThanEqual = itemGroup.SearchValue.GetType().FullName;

                                if (typeGreaterThanEqual == "System.DateTime")
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        rawQuery += itemGroup.SearchField + " >= " + "convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }
                                    else
                                    {
                                        rawQuery += tableName + "." + itemGroup.SearchField + " >= " + "convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        rawQuery += itemGroup.SearchField + " >= " + itemGroup.SearchValue;
                                    }
                                    else
                                    {
                                        rawQuery += tableName + "." + itemGroup.SearchField + " >= " + itemGroup.SearchValue;
                                    }
                                }

                                break;

                            case SearchCondition.LessThan:

                                var typeLessThan = itemGroup.SearchValue.GetType().FullName;

                                if (typeLessThan == "System.DateTime")
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        rawQuery += itemGroup.SearchField + " < " + "convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }
                                    else
                                    {
                                        rawQuery += tableName + "." + itemGroup.SearchField + " < " + "convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        rawQuery += itemGroup.SearchField + " < " + itemGroup.SearchValue;
                                    }
                                    else
                                    {
                                        rawQuery += tableName + "." + itemGroup.SearchField + " < " + itemGroup.SearchValue;
                                    }
                                }

                                break;

                            case SearchCondition.LessThanEqual:

                                var typeLessThanEqual = itemGroup.SearchValue.GetType().FullName;

                                if (typeLessThanEqual == "System.DateTime")
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        rawQuery += itemGroup.SearchField + " <= " + "convert(datetime, '" +
                                            itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }
                                    else
                                    {
                                        rawQuery += tableName + "." + itemGroup.SearchField + " <= " +
                                            "convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        rawQuery += itemGroup.SearchField + " <= " + itemGroup.SearchValue;
                                    }
                                    else
                                    {
                                        rawQuery += tableName + "." + itemGroup.SearchField + " <= " + itemGroup.SearchValue;
                                    }
                                }

                                break;

                            case SearchCondition.DateTimeRange:

                                var arrayValue = ((JArray)itemGroup.SearchValue).ToObject<object[]>();
                                var fromDate = DateTime.Parse(arrayValue[0].ToString());

                                if (arrayValue.Length <= 1)
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        rawQuery += itemGroup.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                        //CAST( '" + fromDate + "' as datetime)";
                                    }
                                    else
                                    {
                                        rawQuery += tableName + "." + itemGroup.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                        //CAST( '" + fromDate + "' as datetime)";
                                    }
                                }
                                else
                                {
                                    if (arrayValue[1] != null)
                                    {
                                        var toDate = DateTime.Parse(arrayValue[1].ToString());
                                        if (string.IsNullOrEmpty(tableName))
                                        {
                                            rawQuery += "( " + itemGroup.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)" + " AND " +
                                                itemGroup.SearchField + " <= " + "convert(datetime, '" + toDate.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101))";
                                        }
                                        else
                                        {
                                            rawQuery += "( " + tableName + "." + itemGroup.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)" + " AND "
                                                + tableName + "." + itemGroup.SearchField + " <= " + "convert(datetime, '" + toDate.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101))";
                                        }
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(tableName))
                                        {
                                            rawQuery += itemGroup.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                        }
                                        else
                                        {
                                            rawQuery += tableName + "." + itemGroup.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                        }
                                    }
                                }

                                break;

                            default:
                                break;
                        }
                    }
                }

                rawQuery = "( " + rawQuery + " )";
            }

            foreach (var search in SearchFieldList)
            {
                if (search.GroupID.HasValue)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(rawQuery))
                {
                    if (string.IsNullOrEmpty(search.CombineCondition))
                    {
                        rawQuery += " AND ";
                    }
                    else
                    {
                        rawQuery += " " + search.CombineCondition + " ";
                    }
                }

                bool isArray = JsonUtilities.CheckJsonArray(search.SearchValue);

                switch (search.SearchCondition)
                {
                    case SearchCondition.IsNull:
                        rawQuery += search.SearchField + " IS NULL ";
                        break;

                    case SearchCondition.In:

                        if (isArray)
                        {
                            var arrayValueEqual = ((JArray)search.SearchValue).ToObject<object[]>();

                            var inVal = "";

                            for (int i = 0; i < arrayValueEqual.Length; i++)
                            {
                                inVal += "'" + arrayValueEqual[i] + "',";
                            }

                            inVal = inVal.Substring(0, inVal.Length - 1);

                            rawQuery += string.IsNullOrEmpty(tableName) ? "" + search.SearchField + " IN (" + inVal + ") " :
                                tableName + "." + search.SearchField + " IN (" + inVal + ") ";
                        }
                        else
                        {
                            rawQuery += String.IsNullOrEmpty(tableName) ? "" + search.SearchField + " IN ('" + search.SearchValue + "') " :
                                tableName + "." + search.SearchField + " IN ('" + search.SearchValue + "') ";
                        }

                        break;

                    case SearchCondition.IsNotNull:
                        rawQuery += search.SearchField + " IS NOT NULL ";
                        break;

                    case SearchCondition.Equal:

                        if (isArray)
                        {
                            var arrayValueEqual = ((JArray)search.SearchValue).ToObject<object[]>();
                            var subQueryEqual = "";

                            for (int i = 0; i < arrayValueEqual.Length; i++)
                            {
                                var type = arrayValueEqual[i].GetType().FullName;
                                switch (type)
                                {
                                    case "System.String":

                                        if (i > 0)
                                        {
                                            subQueryEqual += tableName == null ? " OR " + search.SearchField + " = " + "N'" + arrayValueEqual[i] + "' " :
                                                " OR " + tableName + "." + search.SearchField + " = " + "N'" + arrayValueEqual[i] + "' ";
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(tableName))
                                            {
                                                subQueryEqual += search.SearchField + " = " + "N'" + arrayValueEqual[i] + "'";
                                            }
                                            else
                                            {
                                                subQueryEqual += tableName + "." + search.SearchField + " = " + "N'" + arrayValueEqual[i] + "'";
                                            }
                                        }

                                        break;

                                    default:
                                        break;
                                }
                            }

                            subQueryEqual = "( " + subQueryEqual + " )";
                            rawQuery = rawQuery + subQueryEqual;
                        }
                        else
                        {
                            var type = search.SearchValue.GetType().FullName;

                            if (type == "System.DateTime")
                            {
                                if (string.IsNullOrEmpty(tableName))
                                {
                                    rawQuery += search.SearchField + " = " + "convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                        System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                }
                                else
                                {
                                    rawQuery += tableName + "." + search.SearchField + " = " + "convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                        System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(tableName))
                                {
                                    rawQuery += search.SearchField + " = " + "'" + search.SearchValue + "'";
                                }
                                else
                                {
                                    rawQuery += tableName + "." + search.SearchField + " = " + "'" + search.SearchValue + "'";
                                }
                            }
                        }

                        break;

                    case SearchCondition.NotEqual:

                        if (isArray)
                        {
                            var arrayValueEqual = ((JArray)search.SearchValue).ToObject<object[]>();
                            var subQueryEqual = "";

                            for (int i = 0; i < arrayValueEqual.Length; i++)
                            {
                                var type = arrayValueEqual[i].GetType().FullName;
                                switch (type)
                                {
                                    case "System.String":

                                        if (i > 0)
                                        {
                                            if (string.IsNullOrEmpty(tableName))
                                            {
                                                subQueryEqual += " OR " + search.SearchField + " != " + "N'" + arrayValueEqual[i] + "' ";
                                            }
                                            else
                                            {
                                                subQueryEqual += " OR " + tableName + "." + search.SearchField + " != " + "N'" + arrayValueEqual[i] + "' ";
                                            }
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(tableName))
                                            {
                                                subQueryEqual += search.SearchField + " != " + "N'" + arrayValueEqual[i] + "'";
                                            }
                                            else
                                            {
                                                subQueryEqual += tableName + "." + search.SearchField + " != " + "N'" + arrayValueEqual[i] + "'";
                                            }
                                        }

                                        break;

                                    default:
                                        break;
                                }
                            }

                            subQueryEqual = "( " + subQueryEqual + " )";
                            rawQuery = rawQuery + subQueryEqual;
                        }
                        else
                        {
                            var type = search.SearchValue.GetType().FullName;

                            if (type == "System.DateTime")
                            {
                                if (string.IsNullOrEmpty(tableName))
                                {
                                    rawQuery += search.SearchField + " != " + "convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                        System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                }
                                else
                                {
                                    rawQuery += tableName + "." + search.SearchField + " != " + "convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                        System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(tableName))
                                {
                                    rawQuery += search.SearchField + " != " + "'" + search.SearchValue + "'";
                                }
                                else
                                {
                                    rawQuery += tableName + "." + search.SearchField + " != " + "'" + search.SearchValue + "'";
                                }
                            }
                        }

                        break;

                    case SearchCondition.Contains:

                        if (isArray)
                        {
                            var arrayValueEqual = ((JArray)search.SearchValue).ToObject<object[]>();
                            var subQueryEqual = "";

                            for (int i = 0; i < arrayValueEqual.Length; i++)
                            {
                                if (i > 0)
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        subQueryEqual += " OR " + search.SearchField + " LIKE N'%" + arrayValueEqual[i] + "%' ";
                                    }
                                    else
                                    {
                                        subQueryEqual += " OR " + tableName + "." + search.SearchField + " LIKE N'%" + arrayValueEqual[i] + "%' ";
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        subQueryEqual += search.SearchField + " LIKE N'%" + arrayValueEqual[i] + "%'";
                                    }
                                    else
                                    {
                                        subQueryEqual += tableName + "." + search.SearchField + " LIKE N'%" + arrayValueEqual[i] + "%'";
                                    }
                                }
                            }

                            subQueryEqual = "( " + subQueryEqual + " )";
                            rawQuery = rawQuery + subQueryEqual;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += search.SearchField + " LIKE N'%" + search.SearchValue + "%'";
                            }
                            else
                            {
                                rawQuery += tableName + "." + search.SearchField + " LIKE N'%" + search.SearchValue + "%'";
                            }
                        }

                        break;

                    case SearchCondition.GreaterThan:

                        var typeGreaterThan = search.SearchValue.GetType().FullName;

                        if (typeGreaterThan == "System.DateTime")
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += search.SearchField + " > " + "convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                            }
                            else
                            {
                                rawQuery += tableName + "." + search.SearchField + " > " + "convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += search.SearchField + " > " + search.SearchValue;
                            }
                            else
                            {
                                rawQuery += tableName + "." + search.SearchField + " > " + search.SearchValue;
                            }
                        }

                        break;

                    case SearchCondition.GreaterThanEqual:

                        var typeGreaterThanEqual = search.SearchValue.GetType().FullName;

                        if (typeGreaterThanEqual == "System.DateTime")
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += search.SearchField + " >= " + "convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                            }
                            else
                            {
                                rawQuery += tableName + "." + search.SearchField + " >= " + "convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += search.SearchField + " >= " + search.SearchValue;
                            }
                            else
                            {
                                rawQuery += tableName + "." + search.SearchField + " >= " + search.SearchValue;
                            }
                        }

                        break;

                    case SearchCondition.LessThan:

                        var typeLessThan = search.SearchValue.GetType().FullName;

                        if (typeLessThan == "System.DateTime")
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += search.SearchField + " < " + "convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                            }
                            else
                            {
                                rawQuery += tableName + "." + search.SearchField + " < " + "convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += search.SearchField + " < " + search.SearchValue;
                            }
                            else
                            {
                                rawQuery += tableName + "." + search.SearchField + " < " + search.SearchValue;
                            }
                        }

                        break;

                    case SearchCondition.LessThanEqual:

                        var typeLessThanEqual = search.SearchValue.GetType().FullName;

                        if (typeLessThanEqual == "System.DateTime")
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += search.SearchField + " <= " + "convert(datetime, '" +
                                    search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                            }
                            else
                            {
                                rawQuery += tableName + "." + search.SearchField + " <= " +
                                    "convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += search.SearchField + " <= " + search.SearchValue;
                            }
                            else
                            {
                                rawQuery += tableName + "." + search.SearchField + " <= " + search.SearchValue;
                            }
                        }

                        break;

                    case SearchCondition.DateTimeRange:

                        var arrayValue = ((JArray)search.SearchValue).ToObject<object[]>();
                        var fromDate = DateTime.Parse(arrayValue[0].ToString());

                        if (arrayValue.Length <= 1)
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += search.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                //CAST( '" + fromDate + "' as datetime)";
                            }
                            else
                            {
                                rawQuery += tableName + "." + search.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                //CAST( '" + fromDate + "' as datetime)";
                            }
                        }
                        else
                        {
                            if (arrayValue[1] != null)
                            {
                                var toDate = DateTime.Parse(arrayValue[1].ToString());
                                if (string.IsNullOrEmpty(tableName))
                                {
                                    rawQuery += "( " + search.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)" + " AND " +
                                        search.SearchField + " <= " + "convert(datetime, '" + toDate.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101))";
                                }
                                else
                                {
                                    rawQuery += "( " + tableName + "." + search.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)" + " AND "
                                        + tableName + "." + search.SearchField + " <= " + "convert(datetime, '" + toDate.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101))";
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(tableName))
                                {
                                    rawQuery += search.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                }
                                else
                                {
                                    rawQuery += tableName + "." + search.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                }
                            }
                        }

                        break;

                    default:
                        break;
                }
            }

            return rawQuery;
        }
        */

        /// <summary>
        /// 
        /// </summary>
        /// <param name="SearchFieldList"></param>
        /// <returns></returns>
        public static string BuildRawQueryCondition(List<SearchDTO> SearchFieldList, string tableName = null)
        {
            string rawQuery = "";

            List<SearchDTO> searchGroup = SearchFieldList.Where(x => x.GroupID.HasValue).ToList();

            if (searchGroup.Count > 0)
            {
                var groups = searchGroup.GroupBy(x => x.GroupID).Select(x => x.Key);

                foreach (var group in groups)
                {
                    var searchConditionGroup = searchGroup.Where(x => x.GroupID == group);

                    foreach (var itemGroup in searchConditionGroup)
                    {
                        if (!string.IsNullOrEmpty(rawQuery))
                        {
                            if (string.IsNullOrEmpty(itemGroup.CombineCondition))
                            {
                                rawQuery += " AND ";
                            }
                            else
                            {
                                rawQuery += " " + itemGroup.CombineCondition + " ";
                            }
                        }

                        bool isArray = JsonUtilities.CheckJsonArray(itemGroup.SearchValue);

                        switch (itemGroup.SearchCondition)
                        {
                            case SearchCondition.IsNull:
                                rawQuery += string.IsNullOrEmpty(tableName) ? "" +
                                    itemGroup.SearchField + " IS NULL " : tableName + "." + itemGroup.SearchField + " IS NULL ";
                                break;
                            case SearchCondition.In:

                                if (isArray)
                                {
                                    var arrayValueEqual = ((JArray)itemGroup.SearchValue).ToObject<object[]>();

                                    var inVal = "";

                                    for (int i = 0; i < arrayValueEqual.Length; i++)
                                    {
                                        inVal += "'" + arrayValueEqual[i] + "',";
                                    }

                                    inVal = inVal.Substring(0, inVal.Length - 1);

                                    rawQuery += string.IsNullOrEmpty(tableName) ? "" + itemGroup.SearchField + " IN (" + inVal + ") " :
                                        tableName + "." + itemGroup.SearchField + " IN (" + inVal + ") ";
                                }
                                else
                                {
                                    rawQuery += string.IsNullOrEmpty(tableName) ? "" + itemGroup.SearchField + " IN ('" + itemGroup.SearchValue + "') " :
                                        tableName + "." + itemGroup.SearchField + " IN ('" + itemGroup.SearchValue + "') ";
                                }

                                break;
                            case SearchCondition.NotIn:

                                if (isArray)
                                {
                                    var arrayValueEqual = ((JArray)itemGroup.SearchValue).ToObject<object[]>();

                                    var inVal = "";

                                    for (int i = 0; i < arrayValueEqual.Length; i++)
                                    {
                                        inVal += "'" + arrayValueEqual[i] + "',";
                                    }

                                    inVal = inVal.Substring(0, inVal.Length - 1);

                                    rawQuery += string.IsNullOrEmpty(tableName) ? "" + itemGroup.SearchField + " NOT IN (" + inVal + ") " :
                                        tableName + "." + itemGroup.SearchField + " NOT IN (" + inVal + ") ";
                                }
                                else
                                {
                                    rawQuery += string.IsNullOrEmpty(tableName) ? "" + itemGroup.SearchField + " NOT IN ('" + itemGroup.SearchValue + "') " :
                                        tableName + "." + itemGroup.SearchField + " NOT IN ('" + itemGroup.SearchValue + "') ";
                                }

                                break;
                            case SearchCondition.IsNotNull:
                                rawQuery += itemGroup.SearchField + " IS NOT NULL ";
                                break;

                            case SearchCondition.Equal:

                                if (isArray)
                                {
                                    var arrayValueEqual = ((JArray)itemGroup.SearchValue).ToObject<object[]>();
                                    var subQueryEqual = "";

                                    for (int i = 0; i < arrayValueEqual.Length; i++)
                                    {
                                        var type = arrayValueEqual[i].GetType().FullName;
                                        switch (type)
                                        {
                                            case "System.String":

                                                if (i > 0)
                                                {
                                                    subQueryEqual += tableName == null ? " OR " + itemGroup.SearchField + " = " + "N'" + arrayValueEqual[i] + "' " :
                                                        " OR " + tableName + "." + itemGroup.SearchField + " = " + "N'" + arrayValueEqual[i] + "' ";
                                                }
                                                else
                                                {
                                                    if (string.IsNullOrEmpty(tableName))
                                                    {
                                                        subQueryEqual += itemGroup.SearchField + " = " + "N'" + arrayValueEqual[i] + "'";
                                                    }
                                                    else
                                                    {
                                                        subQueryEqual += tableName + "." + itemGroup.SearchField + " = " + "N'" + arrayValueEqual[i] + "'";
                                                    }
                                                }

                                                break;
                                            default:
                                                break;
                                        }
                                    }

                                    subQueryEqual = "( " + subQueryEqual + " )";
                                    rawQuery = rawQuery + subQueryEqual;
                                }
                                else
                                {
                                    var type = itemGroup.SearchValue.GetType().FullName;

                                    if (type == "System.DateTime")
                                    {
                                        if (string.IsNullOrEmpty(tableName))
                                        {
                                            rawQuery += itemGroup.SearchField + " = " + "convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                                System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                        }
                                        else
                                        {
                                            rawQuery += tableName + "." + itemGroup.SearchField + " = " + "convert(datetime, '"
                                                + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                                System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                        }
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(tableName))
                                        {
                                            rawQuery += itemGroup.SearchField + " = " + "'" + itemGroup.SearchValue + "'";
                                        }
                                        else
                                        {
                                            rawQuery += tableName + "." + itemGroup.SearchField + " = " + "'" + itemGroup.SearchValue + "'";
                                        }
                                    }
                                }

                                break;
                            case SearchCondition.NotEqual:

                                if (isArray)
                                {
                                    var arrayValueEqual = ((JArray)itemGroup.SearchValue).ToObject<object[]>();
                                    var subQueryEqual = "";

                                    for (int i = 0; i < arrayValueEqual.Length; i++)
                                    {
                                        var type = arrayValueEqual[i].GetType().FullName;
                                        switch (type)
                                        {
                                            case "System.String":

                                                if (i > 0)
                                                {
                                                    if (string.IsNullOrEmpty(tableName))
                                                    {
                                                        subQueryEqual += " OR " + itemGroup.SearchField + " != " + "N'" + arrayValueEqual[i] + "' ";
                                                    }
                                                    else
                                                    {
                                                        subQueryEqual += " OR " + tableName + "." + itemGroup.SearchField + " != " + "N'" + arrayValueEqual[i] + "' ";
                                                    }
                                                }
                                                else
                                                {
                                                    if (string.IsNullOrEmpty(tableName))
                                                    {
                                                        subQueryEqual += itemGroup.SearchField + " != " + "N'" + arrayValueEqual[i] + "'";
                                                    }
                                                    else
                                                    {
                                                        subQueryEqual += tableName + "." + itemGroup.SearchField + " != " + "N'" + arrayValueEqual[i] + "'";
                                                    }
                                                }

                                                break;
                                            default:
                                                break;
                                        }
                                    }

                                    subQueryEqual = "( " + subQueryEqual + " )";
                                    rawQuery = rawQuery + subQueryEqual;
                                }
                                else
                                {
                                    var type = itemGroup.SearchValue.GetType().FullName;

                                    if (type == "System.DateTime")
                                    {
                                        if (string.IsNullOrEmpty(tableName))
                                        {
                                            rawQuery += itemGroup.SearchField + " != " + "convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                                System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                        }
                                        else
                                        {
                                            rawQuery += tableName + "." + itemGroup.SearchField + " != " + "convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                                System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                        }
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(tableName))
                                        {
                                            rawQuery += itemGroup.SearchField + " != " + "'" + itemGroup.SearchValue + "'";
                                        }
                                        else
                                        {
                                            rawQuery += tableName + "." + itemGroup.SearchField + " != " + "'" + itemGroup.SearchValue + "'";
                                        }
                                    }
                                }

                                break;
                            case SearchCondition.Contains:

                                if (isArray)
                                {
                                    var arrayValueEqual = ((JArray)itemGroup.SearchValue).ToObject<object[]>();
                                    var subQueryEqual = "";

                                    for (int i = 0; i < arrayValueEqual.Length; i++)
                                    {
                                        if (i > 0)
                                        {
                                            if (string.IsNullOrEmpty(tableName))
                                            {
                                                subQueryEqual += " OR " + itemGroup.SearchField + " LIKE N'%" + arrayValueEqual[i] + "%' ";
                                            }
                                            else
                                            {
                                                subQueryEqual += " OR " + tableName + "." + itemGroup.SearchField + " LIKE N'%" + arrayValueEqual[i] + "%' ";
                                            }
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(tableName))
                                            {
                                                subQueryEqual += itemGroup.SearchField + " LIKE N'%" + arrayValueEqual[i] + "%'";
                                            }
                                            else
                                            {
                                                subQueryEqual += tableName + "." + itemGroup.SearchField + " LIKE N'%" + arrayValueEqual[i] + "%'";
                                            }

                                        }
                                    }

                                    subQueryEqual = "( " + subQueryEqual + " )";
                                    rawQuery = rawQuery + subQueryEqual;
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        rawQuery += itemGroup.SearchField + " LIKE N'%" + itemGroup.SearchValue + "%'";
                                    }
                                    else
                                    {
                                        rawQuery += tableName + "." + itemGroup.SearchField + " LIKE N'%" + itemGroup.SearchValue + "%'";
                                    }
                                }

                                break;
                            case SearchCondition.GreaterThan:

                                var typeGreaterThan = itemGroup.SearchValue.GetType().FullName;

                                if (typeGreaterThan == "System.DateTime")
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        rawQuery += itemGroup.SearchField + " > " + "convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }
                                    else
                                    {
                                        rawQuery += tableName + "." + itemGroup.SearchField + " > " + "convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        rawQuery += itemGroup.SearchField + " > " + itemGroup.SearchValue;
                                    }
                                    else
                                    {
                                        rawQuery += tableName + "." + itemGroup.SearchField + " > " + itemGroup.SearchValue;
                                    }
                                }


                                break;
                            case SearchCondition.GreaterThanEqual:

                                var typeGreaterThanEqual = itemGroup.SearchValue.GetType().FullName;

                                if (typeGreaterThanEqual == "System.DateTime")
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        rawQuery += itemGroup.SearchField + " >= " + "convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }
                                    else
                                    {
                                        rawQuery += tableName + "." + itemGroup.SearchField + " >= " + "convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }

                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        rawQuery += itemGroup.SearchField + " >= " + itemGroup.SearchValue;
                                    }
                                    else
                                    {
                                        rawQuery += tableName + "." + itemGroup.SearchField + " >= " + itemGroup.SearchValue;
                                    }

                                }

                                break;
                            case SearchCondition.LessThan:

                                var typeLessThan = itemGroup.SearchValue.GetType().FullName;

                                if (typeLessThan == "System.DateTime")
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        rawQuery += itemGroup.SearchField + " < " + "convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }
                                    else
                                    {
                                        rawQuery += tableName + "." + itemGroup.SearchField + " < " + "convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        rawQuery += itemGroup.SearchField + " < " + itemGroup.SearchValue;
                                    }
                                    else
                                    {
                                        rawQuery += tableName + "." + itemGroup.SearchField + " < " + itemGroup.SearchValue;
                                    }
                                }

                                break;
                            case SearchCondition.LessThanEqual:

                                var typeLessThanEqual = itemGroup.SearchValue.GetType().FullName;

                                if (typeLessThanEqual == "System.DateTime")
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        rawQuery += itemGroup.SearchField + " <= " + "convert(datetime, '" +
                                            itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }
                                    else
                                    {
                                        rawQuery += tableName + "." + itemGroup.SearchField + " <= " +
                                            "convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        rawQuery += itemGroup.SearchField + " <= " + itemGroup.SearchValue;
                                    }
                                    else
                                    {
                                        rawQuery += tableName + "." + itemGroup.SearchField + " <= " + itemGroup.SearchValue;
                                    }
                                }

                                break;
                            case SearchCondition.DateTimeRange:

                                var arrayValue = ((JArray)itemGroup.SearchValue).ToObject<object[]>();
                                var fromDate = DateTime.Parse(arrayValue[0].ToString());

                                if (arrayValue.Length <= 1)
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        rawQuery += itemGroup.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                        //CAST( '" + fromDate + "' as datetime)";
                                    }
                                    else
                                    {
                                        rawQuery += tableName + "." + itemGroup.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                        //CAST( '" + fromDate + "' as datetime)";
                                    }

                                }
                                else
                                {
                                    if (arrayValue[1] != null)
                                    {
                                        var toDate = DateTime.Parse(arrayValue[1].ToString());
                                        if (string.IsNullOrEmpty(tableName))
                                        {
                                            rawQuery += "( " + itemGroup.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)" + " AND " +
                                                itemGroup.SearchField + " <= " + "convert(datetime, '" + toDate.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101))";
                                        }
                                        else
                                        {
                                            rawQuery += "( " + tableName + "." + itemGroup.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)" + " AND "
                                                + tableName + "." + itemGroup.SearchField + " <= " + "convert(datetime, '" + toDate.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101))";
                                        }
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(tableName))
                                        {
                                            rawQuery += itemGroup.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                        }
                                        else
                                        {
                                            rawQuery += tableName + "." + itemGroup.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                            System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                        }
                                    }
                                }

                                break;
                            default:
                                break;
                        }
                    }
                }

                rawQuery = "( " + rawQuery + " )";
            }

            foreach (var search in SearchFieldList)
            {
                if (search.GroupID.HasValue)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(rawQuery))
                {
                    if (string.IsNullOrEmpty(search.CombineCondition))
                    {
                        rawQuery += " AND ";
                    }
                    else
                    {
                        rawQuery += " " + search.CombineCondition + " ";
                    }
                }

                bool isArray = JsonUtilities.CheckJsonArray(search.SearchValue);

                switch (search.SearchCondition)
                {
                    case SearchCondition.IsNull:
                        rawQuery += string.IsNullOrEmpty(tableName) ? "" + search.SearchField + " IS NULL " : tableName + "." + search.SearchField + " IS NULL ";
                        break;
                    case SearchCondition.In:

                        if (isArray)
                        {
                            var arrayValueEqual = ((JArray)search.SearchValue).ToObject<object[]>();

                            var inVal = "";

                            for (int i = 0; i < arrayValueEqual.Length; i++)
                            {
                                inVal += "'" + arrayValueEqual[i] + "',";
                            }

                            inVal = inVal.Substring(0, inVal.Length - 1);

                            rawQuery += string.IsNullOrEmpty(tableName) ? "" + search.SearchField + " IN (" + inVal + ") " :
                                tableName + "." + search.SearchField + " IN (" + inVal + ") ";
                        }
                        else
                        {
                            rawQuery += string.IsNullOrEmpty(tableName) ? "" + search.SearchField + " IN ('" + search.SearchValue + "') " :
                                tableName + "." + search.SearchField + " IN ('" + search.SearchValue + "') ";
                        }

                        break;
                    case SearchCondition.NotIn:

                        if (isArray)
                        {
                            var arrayValueEqual = ((JArray)search.SearchValue).ToObject<object[]>();

                            var inVal = "";

                            for (int i = 0; i < arrayValueEqual.Length; i++)
                            {
                                inVal += "'" + arrayValueEqual[i] + "',";
                            }

                            inVal = inVal.Substring(0, inVal.Length - 1);

                            rawQuery += string.IsNullOrEmpty(tableName) ? "" + search.SearchField + " NOT IN (" + inVal + ") " :
                                tableName + "." + search.SearchField + " NOT IN (" + inVal + ") ";
                        }
                        else
                        {
                            rawQuery += string.IsNullOrEmpty(tableName) ? "" + search.SearchField + " NOT IN ('" + search.SearchValue + "') " :
                                tableName + "." + search.SearchField + " NOT IN ('" + search.SearchValue + "') ";
                        }

                        break;
                    case SearchCondition.IsNotNull:
                        rawQuery += search.SearchField + " IS NOT NULL ";
                        break;

                    case SearchCondition.Equal:

                        if (isArray)
                        {
                            var arrayValueEqual = ((JArray)search.SearchValue).ToObject<object[]>();
                            var subQueryEqual = "";

                            for (int i = 0; i < arrayValueEqual.Length; i++)
                            {
                                var type = arrayValueEqual[i].GetType().FullName;
                                switch (type)
                                {
                                    case "System.String":

                                        if (i > 0)
                                        {
                                            subQueryEqual += tableName == null ? " OR " + search.SearchField + " = " + "N'" + arrayValueEqual[i] + "' " :
                                                " OR " + tableName + "." + search.SearchField + " = " + "N'" + arrayValueEqual[i] + "' ";
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(tableName))
                                            {
                                                subQueryEqual += search.SearchField + " = " + "N'" + arrayValueEqual[i] + "'";
                                            }
                                            else
                                            {
                                                subQueryEqual += tableName + "." + search.SearchField + " = " + "N'" + arrayValueEqual[i] + "'";
                                            }
                                        }

                                        break;
                                    default:
                                        break;
                                }
                            }

                            subQueryEqual = "( " + subQueryEqual + " )";
                            rawQuery = rawQuery + subQueryEqual;
                        }
                        else
                        {
                            var type = search.SearchValue == null ? "" : search.SearchValue.GetType().FullName;

                            if (type == "System.DateTime")
                            {
                                if (string.IsNullOrEmpty(tableName))
                                {
                                    rawQuery += search.SearchField + " = " + "convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                        System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                }
                                else
                                {
                                    rawQuery += tableName + "." + search.SearchField + " = " + "convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                        System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(tableName))
                                {
                                    rawQuery += search.SearchField + " = " + "N'" + search.SearchValue + "'";
                                }
                                else
                                {
                                    if (search.SearchValue is null)
                                    {
                                        rawQuery += tableName + "." + search.SearchField + " IS NULL ";
                                    }
                                    else
                                    {
                                        rawQuery += tableName + "." + search.SearchField + " = " + "N'" + search.SearchValue + "'";
                                    }

                                }
                            }
                        }

                        break;
                    case SearchCondition.NotEqual:

                        if (isArray)
                        {
                            var arrayValueEqual = ((JArray)search.SearchValue).ToObject<object[]>();
                            var subQueryEqual = "";

                            for (int i = 0; i < arrayValueEqual.Length; i++)
                            {
                                var type = arrayValueEqual[i].GetType().FullName;
                                switch (type)
                                {
                                    case "System.String":

                                        if (i > 0)
                                        {
                                            if (string.IsNullOrEmpty(tableName))
                                            {
                                                subQueryEqual += " OR " + search.SearchField + " != " + "N'" + arrayValueEqual[i] + "' ";
                                            }
                                            else
                                            {
                                                subQueryEqual += " OR " + tableName + "." + search.SearchField + " != " + "N'" + arrayValueEqual[i] + "' ";
                                            }
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(tableName))
                                            {
                                                subQueryEqual += search.SearchField + " != " + "N'" + arrayValueEqual[i] + "'";
                                            }
                                            else
                                            {
                                                subQueryEqual += tableName + "." + search.SearchField + " != " + "N'" + arrayValueEqual[i] + "'";
                                            }
                                        }

                                        break;
                                    default:
                                        break;
                                }
                            }

                            subQueryEqual = "( " + subQueryEqual + " )";
                            rawQuery = rawQuery + subQueryEqual;
                        }
                        else
                        {
                            var type = search.SearchValue.GetType().FullName;

                            if (type == "System.DateTime")
                            {
                                if (string.IsNullOrEmpty(tableName))
                                {
                                    rawQuery += search.SearchField + " != " + "convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                        System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                }
                                else
                                {
                                    rawQuery += tableName + "." + search.SearchField + " != " + "convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                        System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(tableName))
                                {
                                    rawQuery += search.SearchField + " != " + "N'" + search.SearchValue + "'";
                                }
                                else
                                {
                                    rawQuery += tableName + "." + search.SearchField + " != " + "N'" + search.SearchValue + "'";
                                }
                            }
                        }

                        break;
                    case SearchCondition.Contains:

                        if (isArray)
                        {
                            var arrayValueEqual = ((JArray)search.SearchValue).ToObject<object[]>();
                            var subQueryEqual = "";

                            for (int i = 0; i < arrayValueEqual.Length; i++)
                            {
                                if (i > 0)
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        subQueryEqual += " OR " + search.SearchField + " LIKE N'%" + arrayValueEqual[i] + "%' ";
                                    }
                                    else
                                    {
                                        subQueryEqual += " OR " + tableName + "." + search.SearchField + " LIKE N'%" + arrayValueEqual[i] + "%' ";
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        subQueryEqual += search.SearchField + " LIKE N'%" + arrayValueEqual[i] + "%'";
                                    }
                                    else
                                    {
                                        subQueryEqual += tableName + "." + search.SearchField + " LIKE N'%" + arrayValueEqual[i] + "%'";
                                    }

                                }
                            }

                            subQueryEqual = "( " + subQueryEqual + " )";
                            rawQuery = rawQuery + subQueryEqual;
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += search.SearchField + " LIKE N'%" + search.SearchValue + "%'";
                            }
                            else
                            {
                                rawQuery += tableName + "." + search.SearchField + " LIKE N'%" + search.SearchValue + "%'";
                            }
                        }

                        break;
                    case SearchCondition.GreaterThan:

                        var typeGreaterThan = search.SearchValue.GetType().FullName;

                        if (typeGreaterThan == "System.DateTime")
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += search.SearchField + " > " + "convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                            }
                            else
                            {
                                rawQuery += tableName + "." + search.SearchField + " > " + "convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += search.SearchField + " > " + search.SearchValue;
                            }
                            else
                            {
                                rawQuery += tableName + "." + search.SearchField + " > " + search.SearchValue;
                            }
                        }


                        break;
                    case SearchCondition.GreaterThanEqual:

                        var typeGreaterThanEqual = search.SearchValue.GetType().FullName;

                        if (typeGreaterThanEqual == "System.DateTime")
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += search.SearchField + " >= " + "convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                            }
                            else
                            {
                                rawQuery += tableName + "." + search.SearchField + " >= " + "convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                            }

                        }
                        else
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += search.SearchField + " >= " + search.SearchValue;
                            }
                            else
                            {
                                rawQuery += tableName + "." + search.SearchField + " >= " + search.SearchValue;
                            }

                        }

                        break;
                    case SearchCondition.LessThan:

                        var typeLessThan = search.SearchValue.GetType().FullName;

                        if (typeLessThan == "System.DateTime")
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += search.SearchField + " < " + "convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                            }
                            else
                            {
                                rawQuery += tableName + "." + search.SearchField + " < " + "convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += search.SearchField + " < " + search.SearchValue;
                            }
                            else
                            {
                                rawQuery += tableName + "." + search.SearchField + " < " + search.SearchValue;
                            }

                        }

                        break;
                    case SearchCondition.LessThanEqual:

                        var typeLessThanEqual = search.SearchValue.GetType().FullName;

                        if (typeLessThanEqual == "System.DateTime")
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += search.SearchField + " <= " + "convert(datetime, '" +
                                    search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                            }
                            else
                            {
                                rawQuery += tableName + "." + search.SearchField + " <= " +
                                    "convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += search.SearchField + " <= " + search.SearchValue;
                            }
                            else
                            {
                                rawQuery += tableName + "." + search.SearchField + " <= " + search.SearchValue;
                            }
                        }

                        break;
                    case SearchCondition.DateTimeRange:

                        var arrayValue = ((JArray)search.SearchValue).ToObject<object[]>();
                        var fromDate = DateTime.Parse(arrayValue[0].ToString());

                        if (arrayValue.Length <= 1)
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += search.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                //CAST( '" + fromDate + "' as datetime)";
                            }
                            else
                            {
                                rawQuery += tableName + "." + search.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                //CAST( '" + fromDate + "' as datetime)";
                            }

                        }
                        else
                        {
                            if (arrayValue[1] != null)
                            {
                                var toDate = DateTime.Parse(arrayValue[1].ToString());
                                if (string.IsNullOrEmpty(tableName))
                                {
                                    rawQuery += "( " + search.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)" + " AND " +
                                        search.SearchField + " <= " + "convert(datetime, '" + toDate.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101))";
                                }
                                else
                                {
                                    rawQuery += "( " + tableName + "." + search.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)" + " AND "
                                        + tableName + "." + search.SearchField + " <= " + "convert(datetime, '" + toDate.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101))";
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(tableName))
                                {
                                    rawQuery += search.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                }
                                else
                                {
                                    rawQuery += tableName + "." + search.SearchField + " >= " + "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                }
                            }
                        }

                        break;
                    default:
                        break;
                }
            }

            return rawQuery;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="tableName"></param>
        /// <param name="sort"></param>
        /// <param name="colsRef"></param>
        /// <param name="joinRefTbl"></param>
        /// <returns></returns>
        public static string BuildRawQueryPaging(int pageSize, int pageIndex, string rootTbl, List<ReferenceTable> refTblList,
            List<SearchDTO> SearchFieldList, SortDTO sortDTO, List<string> fields = null)
        {

            var rawQueryPaging = @"DECLARE @PageSize INT = {0}, @PageNum  INT = {1};
                            WITH TempResult AS (
                                {2}
                            ), TempCount AS(
                                SELECT COUNT(*) AS TotalRow FROM TempResult
                            )
                            SELECT *
                            FROM TempResult, TempCount
                            ORDER BY TempResult.{3}
                                OFFSET(@PageNum - 1) * @PageSize ROWS
                                   FETCH NEXT @PageSize ROWS ONLY";

            var aliasRootTbl = "r";
            var selectedCols = "";
            //convert select field to jarray
            List<string> fieldSelects = null;
            if (fields != null)
            {               
                fieldSelects = fields == null ? null : fields.Where(x => !x.Contains(".")).ToList();

                foreach (var item in fieldSelects)
                {
                    var isExist = refTblList == null ? null : refTblList.FirstOrDefault(x => x.AliasName == item);
                    if (isExist == null)
                    {
                        selectedCols += aliasRootTbl + "." + PostgresConst.QUOTE + item.ToString() + PostgresConst.QUOTE + ",";
                    }
                }
                //check selected column doesn't have company code, add selected company code to compare others
                if (!fieldSelects.Any(x => x.ToString() == TableColumnConst.COMPANY_CODE_COL))
                {
                    selectedCols += aliasRootTbl + "." + PostgresConst.QUOTE +
                        TableColumnConst.COMPANY_CODE_COL.ToString() + PostgresConst.QUOTE + ",";
                }

                //remove "," in last index
                selectedCols = selectedCols.Substring(0, selectedCols.Length - 1);
            }

            var rawQuery = string.IsNullOrEmpty(selectedCols) ? "SELECT " + aliasRootTbl + ".* {0} FROM " + rootTbl + " AS " + aliasRootTbl :
                "SELECT " + selectedCols + " {0} FROM " + rootTbl + " AS " + aliasRootTbl;

            var conditionJoin = "";
            var selectCols = "";

            var count = 1;

            if (refTblList != null)
            {
                foreach (var refTbl in refTblList)
                {
                    //check reference column in fiedl
                    if (fieldSelects != null && !fieldSelects.Any(x => x.ToString() == refTbl.AliasName))
                    {
                        continue;
                    }

                    var aliasTbl = "t" + count;
                    var aliasName = refTbl.AliasName;

                    selectCols += ", " + aliasTbl + "." + refTbl.ColumnName + " AS " + aliasName;

                    conditionJoin += " LEFT JOIN " + refTbl.TableName + " AS " + aliasTbl + " ON " +
                        aliasRootTbl + "." + refTbl.ForeignKeyCol + "=" + aliasTbl + $".{TableColumnConst.CODE_COL} ";

                    if (refTbl.FilterType != "All" || string.IsNullOrEmpty(refTbl.FilterType))
                    {
                        conditionJoin += "AND " + aliasRootTbl + "." + TableColumnConst.COMPANY_CODE_COL + " = " +
                            aliasTbl + "." + TableColumnConst.COMPANY_CODE_COL + " ";
                    }

                    count++;
                }
            }

            sortDTO = sortDTO == null ? new SortDTO() { SortBy = TableColumnConst.CODE_COL, Sort = "desc" } : sortDTO;
            var sortQuery = sortDTO.SortBy + " " + sortDTO.Sort;

            var whereCondition = BuildRawQueryCondition(SearchFieldList, aliasRootTbl);

            rawQuery = string.Format(rawQuery, selectCols);
            rawQuery += conditionJoin;
            rawQuery += string.IsNullOrEmpty(whereCondition) ? " " : " WHERE " + whereCondition;

            rawQueryPaging = string.Format(rawQueryPaging, pageSize, pageIndex, rawQuery, sortQuery);

            return rawQueryPaging;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="rootTbl"></param>
        /// <param name="refTblList"></param>
        /// <param name="SearchFieldList"></param>
        /// <param name="sortDTO"></param>
        /// <returns></returns>
        public static string BuildRawQueryJoin(string rootTbl, List<ReferenceTable> refTblList,
            List<SearchDTO> SearchFieldList, SortDTO sortDTO, List<string> fields = null)
        {
            var aliasRootTbl = "r";
            var selectedCols = "";

            //convert select field to jarray
            List<string> fieldSelects = null;
            if (fields != null)
            {
                fieldSelects = fields == null ? null : fields.Where(x => !x.Contains(".")).ToList();

                foreach (var item in fieldSelects)
                {
                    var isExist = refTblList != null ? refTblList.FirstOrDefault(x => x.AliasName == item) : null;
                    if (isExist == null)
                    {
                        selectedCols += aliasRootTbl + "." + PostgresConst.QUOTE + item.ToString() + PostgresConst.QUOTE + ",";
                    }
                }
                //check selected column doesn't have company code, add selected company code to compare others
                if (!fieldSelects.Any(x => x.ToString() == TableColumnConst.COMPANY_CODE_COL))
                {
                    selectedCols += aliasRootTbl + "." + PostgresConst.QUOTE +
                        TableColumnConst.COMPANY_CODE_COL.ToString() + PostgresConst.QUOTE + ",";
                }

                //remove "," in last index
                selectedCols = selectedCols.Substring(0, selectedCols.Length - 1);
            }

            var rawQuery = string.IsNullOrEmpty(selectedCols) ?
                "SELECT " + aliasRootTbl + ".* {0} FROM " + rootTbl + " AS " + aliasRootTbl :
                 "SELECT " + selectedCols + " {0} FROM " + rootTbl + " AS " + aliasRootTbl;

            var conditionJoin = "";
            var selectCols = "";

            var count = 1;

            if (refTblList != null)
            {
                foreach (var refTbl in refTblList)
                {
                    //check reference column in fiedl
                    if (fieldSelects != null && !fieldSelects.Any(x => x.ToString() == refTbl.AliasName))
                    {
                        continue;
                    }

                    var aliasTbl = "t" + count;
                    var aliasName = refTbl.AliasName;

                    selectCols += ", " + aliasTbl + "." + refTbl.ColumnName + " AS " + aliasName;

                    conditionJoin += " LEFT JOIN " + refTbl.TableName + " AS " + aliasTbl + " ON "
                        + aliasRootTbl + "." + refTbl.ForeignKeyCol + "=" + aliasTbl + $".{TableColumnConst.CODE_COL} ";

                    if (refTbl.FilterType != "All" || string.IsNullOrEmpty(refTbl.FilterType))
                    {
                        conditionJoin += "AND " + aliasRootTbl + "." + TableColumnConst.COMPANY_CODE_COL + " = "
                            + aliasTbl + "." + TableColumnConst.COMPANY_CODE_COL + " ";
                    }

                    count++;
                }
            }


            sortDTO = sortDTO == null ? new SortDTO() { SortBy = TableColumnConst.CODE_COL, Sort = "desc" } : sortDTO;
            var sortQuery = "ORDER BY " + aliasRootTbl + "." + sortDTO.SortBy + " " + sortDTO.Sort;

            var whereCondition = BuildRawQueryCondition(SearchFieldList, aliasRootTbl);

            rawQuery = string.Format(rawQuery, selectCols);
            rawQuery += conditionJoin;
            rawQuery += string.IsNullOrEmpty(whereCondition) ? " " + sortQuery : " WHERE " + whereCondition + " " + sortQuery;

            return rawQuery;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootTbl"></param>
        /// <param name="refTblList"></param>
        /// <param name="SearchFieldList"></param>
        /// <param name="sortDTO"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static string BuildRawQueryJoinLimit(string rootTbl, List<ReferenceTable> refTblList,
           List<SearchDTO> SearchFieldList, SortDTO sortDTO, List<string> fields = null, int limit = 1)
        {
            var aliasRootTbl = "r";
            var selectedCols = "";

            //convert select field to jarray
            List<string> fieldSelects = null;

            if (fields != null)
            {
                fieldSelects = fields == null ? null : fields.Where(x => !x.Contains(".")).ToList();

                foreach (var item in fieldSelects)
                {
                    var isExist = refTblList != null ? refTblList.FirstOrDefault(x => x.AliasName == item.ToString()) : null;
                    if (isExist == null)
                    {
                        selectedCols += aliasRootTbl + "." + item.ToString() + ",";
                    }
                }
                //check selected column doesn't have company code, add selected company code to compare others
                if (!fieldSelects.Any(x => x.ToString() == TableColumnConst.COMPANY_CODE_COL))
                {
                    selectedCols += aliasRootTbl + "." + TableColumnConst.COMPANY_CODE_COL.ToString() + ",";
                }

                //remove "," in last index
                selectedCols = selectedCols.Substring(0, selectedCols.Length - 1);
            }

            var rawQuery = string.IsNullOrEmpty(selectedCols) ?
                $"SELECT TOP {limit} " + aliasRootTbl + ".* {0} FROM " + rootTbl + " AS " + aliasRootTbl :
                  $"SELECT TOP {limit} " + selectedCols + " {0} FROM " + rootTbl + " AS " + aliasRootTbl;

            var conditionJoin = "";
            var selectCols = "";

            var count = 1;

            foreach (var refTbl in refTblList)
            {
                //check reference column in fiedl
                if (fieldSelects != null && !fieldSelects.Any(x => x.ToString() == refTbl.AliasName))
                {
                    continue;
                }

                var aliasTbl = "t" + count;
                var aliasName = refTbl.AliasName;

                selectCols += ", " + aliasTbl + "." + refTbl.ColumnName + " AS " + aliasName;

                conditionJoin += " LEFT JOIN " + refTbl.TableName + " AS " + aliasTbl + " ON "
                    + aliasRootTbl + "." + refTbl.ForeignKeyCol + " = " + aliasTbl + $".{TableColumnConst.CODE_COL} ";

                if (refTbl.FilterType != "All" || string.IsNullOrEmpty(refTbl.FilterType))
                {
                    conditionJoin += "AND " + aliasRootTbl + "." + TableColumnConst.COMPANY_CODE_COL + " = "
                        + aliasTbl + "." + TableColumnConst.COMPANY_CODE_COL + " ";
                }

                count++;
            }

            sortDTO = sortDTO == null ? new SortDTO() { SortBy = TableColumnConst.CODE_COL, Sort = "desc" } : sortDTO;
            var sortQuery = "ORDER BY " + aliasRootTbl + "." + sortDTO.SortBy + " " + sortDTO.Sort;

            var whereCondition = BuildRawQueryCondition(SearchFieldList, aliasRootTbl);

            rawQuery = string.Format(rawQuery, selectCols);
            rawQuery += conditionJoin;
            rawQuery += string.IsNullOrEmpty(whereCondition) ? " " + sortQuery : " WHERE " + whereCondition + " " + sortQuery;
            return rawQuery;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="rootTbl"></param>
        /// <param name="SearchFieldList"></param>
        /// <param name="sortDTO"></param>
        /// <returns></returns>
        public static string BuildRawQuery(string rootTbl, List<SearchDTO> SearchFieldList, SortDTO sortDTO,
            List<string> fields = null)
        {
            var aliasRootTbl = "r";
            string rawQuery;

            if (fields == null)
            {
                rawQuery = "SELECT " + aliasRootTbl + ".* {0} FROM " + rootTbl + " AS " + aliasRootTbl;
            }
            else
            {
                //if (fields?.GetType().IsArray)
                //{

                //    string selects = "";

                //    for (int i = 0; i < fields.Length; i++)
                //    {
                //        if (i == 0 || i == (fields.Length - 1))
                //        {
                //            selects += " " + aliasRootTbl + "." + fields[i];
                //        }
                //        else if (i < (fields.Length - 1) && i > 0)
                //        {
                //            selects += " " + aliasRootTbl + "." + fields[i] + " , ";
                //        }
                //    }

                //    rawQuery = "SELECT " + selects + " FROM " + rootTbl + " AS " + aliasRootTbl;
                //}
                //else
                //{
                //    //convert select field to jarray
                //    var fieldSelects = ((JArray)fields).Where(x => x.Type == JTokenType.String).ToList();
                //    var selectedCols = "";

                //    foreach (var item in fieldSelects)
                //    {
                //        selectedCols += aliasRootTbl + "." + item.ToString() + ",";
                //    }

                //    //remove "," in last index
                //    selectedCols = selectedCols.Substring(0, selectedCols.Length - 1);

                //    rawQuery = "SELECT " + selectedCols + " FROM " + rootTbl + " AS " + aliasRootTbl;
                //}
                var selectedCols = "";
                foreach (var item in fields)
                {

                    selectedCols += aliasRootTbl + "." + item.ToString() + ",";

                }
                //check selected column doesn't have company code, add selected company code to compare others
                if (!fields.Any(x => x.ToString() == TableColumnConst.COMPANY_CODE_COL))
                {
                    selectedCols += aliasRootTbl + "." + TableColumnConst.COMPANY_CODE_COL.ToString() + ",";
                }

                //remove "," in last index
                selectedCols = selectedCols.Substring(0, selectedCols.Length - 1);
                rawQuery = "SELECT " + selectedCols + " FROM " + PostgresConst.QUOTE + rootTbl + PostgresConst.QUOTE + " AS " + aliasRootTbl;
            }

            var conditionJoin = "";
            var selectCols = "";

            sortDTO = sortDTO == null ? new SortDTO() { SortBy = TableColumnConst.CODE_COL, Sort = "desc" } : sortDTO;
            var sortQuery = "ORDER BY " + aliasRootTbl + "." + sortDTO.SortBy + " " + sortDTO.Sort;

            var whereCondition = BuildRawQueryCondition(SearchFieldList, aliasRootTbl);

            rawQuery = string.Format(rawQuery, selectCols);
            rawQuery += conditionJoin;
            rawQuery += string.IsNullOrEmpty(whereCondition) ? " " + sortQuery : " WHERE " + whereCondition + " " + sortQuery;

            return rawQuery;
        }
    }
}