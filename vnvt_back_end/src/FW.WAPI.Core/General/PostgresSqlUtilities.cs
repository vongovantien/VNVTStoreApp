using FW.WAPI.Core.DAL.DTO;
using FW.WAPI.Core.DAL.Model.Configuration;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FW.WAPI.Core.General
{
    public static class PostgresSqlUtilities
    {
        /// <summary>
        /// Build search query for postgres
        /// </summary>
        /// <param name="SearchFieldList"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static string BuildRawQueryCondition(List<SearchDTO> SearchFieldList, string tableName = null)
        {
            string rawQuery = "";

            List<SearchDTO> searchGroup = SearchFieldList.Where(x => x.GroupID.HasValue).ToList();

            if (searchGroup.Count > 0)
            {
                var groups = searchGroup.GroupBy(x => x.GroupID).Select(x => x.Key);

                int index = 0;

                foreach (var group in groups)
                {
                    var groupQuery = "";
                    var searchConditionGroup = searchGroup.Where(x => x.GroupID == group);

                    foreach (var itemGroup in searchConditionGroup)
                    {
                        if (!string.IsNullOrEmpty(groupQuery))
                        {
                            if (string.IsNullOrEmpty(itemGroup.CombineCondition))
                            {
                                groupQuery += " AND ";
                            }
                            else
                            {
                                groupQuery += " " + itemGroup.CombineCondition + " ";
                            }
                        }

                        bool isArray = JsonUtilities.CheckJsonArray(itemGroup.SearchValue);

                        switch (itemGroup.SearchCondition)
                        {
                            case SearchCondition.IsNull:
                                groupQuery += string.IsNullOrEmpty(tableName) ? " " + PostgresConst.QUOTE +
                                    itemGroup.SearchField + PostgresConst.QUOTE + " IS NULL " :
                                    PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." + PostgresConst.QUOTE +
                                    itemGroup.SearchField + PostgresConst.QUOTE + " IS NULL ";
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

                                    groupQuery += string.IsNullOrEmpty(tableName) ? "" + PostgresConst.QUOTE +
                                        itemGroup.SearchField + PostgresConst.QUOTE + " IN (" + inVal + ") " :
                                        PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                        PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " IN (" + inVal + ") ";
                                }
                                else
                                {
                                    groupQuery += string.IsNullOrEmpty(tableName) ? "" + PostgresConst.QUOTE +
                                        itemGroup.SearchField + PostgresConst.QUOTE + " IN ('" + itemGroup.SearchValue + "') " :
                                       PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                       PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " IN ('" + itemGroup.SearchValue + "') ";
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

                                    groupQuery += string.IsNullOrEmpty(tableName) ? "" + PostgresConst.QUOTE +
                                        itemGroup.SearchField + PostgresConst.QUOTE + " NOT IN (" + inVal + ") " :
                                        PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                        PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " NOT IN (" + inVal + ") ";
                                }
                                else
                                {
                                    groupQuery += string.IsNullOrEmpty(tableName) ? "" + PostgresConst.QUOTE +
                                        itemGroup.SearchField + PostgresConst.QUOTE + " NOT IN ('" + itemGroup.SearchValue + "') " :
                                       PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                       PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " NOT IN ('" + itemGroup.SearchValue + "') ";
                                }

                                break;

                            case SearchCondition.IsNotNull:
                                groupQuery += PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " IS NOT NULL ";
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
                                                    subQueryEqual += tableName == null ? " OR " + PostgresConst.QUOTE + itemGroup.SearchField +
                                                        PostgresConst.QUOTE + $" {PostgresConst.ILIKE} " + "N'" + arrayValueEqual[i] + "' " :
                                                        " OR " + PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                                        PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + $" {PostgresConst.ILIKE} " +
                                                        "N'" + arrayValueEqual[i] + "' ";
                                                }
                                                else
                                                {
                                                    if (string.IsNullOrEmpty(tableName))
                                                    {
                                                        subQueryEqual += PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE +
                                                            $" {PostgresConst.ILIKE} " + "N'" + arrayValueEqual[i] + "'";
                                                    }
                                                    else
                                                    {
                                                        subQueryEqual += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                                            PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE
                                                            + $" {PostgresConst.ILIKE} " + " N'" + arrayValueEqual[i] + "'";
                                                    }
                                                }

                                                break;
                                            case "System.Boolean":
                                                if (i > 0)
                                                {
                                                    subQueryEqual += tableName == null ? " OR " + PostgresConst.QUOTE + itemGroup.SearchField +
                                                        PostgresConst.QUOTE + " = " + arrayValueEqual[i] + " " :
                                                        " OR " + PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                                        PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " = " + arrayValueEqual[i] + " ";
                                                }
                                                else
                                                {
                                                    if (string.IsNullOrEmpty(tableName))
                                                    {
                                                        subQueryEqual += PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE +
                                                            " = " + arrayValueEqual[i] + "";
                                                    }
                                                    else
                                                    {
                                                        subQueryEqual += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                                            PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE
                                                            + " = " + arrayValueEqual[i];
                                                    }
                                                }
                                                break;
                                            default:
                                                break;
                                        }
                                    }

                                    subQueryEqual = "( " + subQueryEqual + " )";
                                    groupQuery = groupQuery + subQueryEqual;
                                }
                                else
                                {
                                    var type = itemGroup.SearchValue.GetType().FullName;

                                    if (type == "System.DateTime")
                                    {
                                        if (string.IsNullOrEmpty(tableName))
                                        {
                                            groupQuery += PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE +
                                                " = " + $"to_timestamp('{itemGroup.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                                            //"convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                            //System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                        }
                                        else
                                        {
                                            groupQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                                PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " = " +
                                                $"to_timestamp('{itemGroup.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                                            //"convert(datetime, '"
                                            //+ itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                            //System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                        }
                                    }
                                    else if (type == "System.String")
                                    {
                                        if (string.IsNullOrEmpty(tableName))
                                        {
                                            groupQuery += PostgresConst.QUOTE + itemGroup.SearchField +
                                                PostgresConst.QUOTE + $" {PostgresConst.ILIKE} " + "'" + itemGroup.SearchValue + "'";
                                        }
                                        else
                                        {
                                            groupQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE +
                                                "." + PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + $" {PostgresConst.ILIKE} "
                                                + "'" + itemGroup.SearchValue + "'";
                                        }
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(tableName))
                                        {
                                            groupQuery += PostgresConst.QUOTE + itemGroup.SearchField +
                                                PostgresConst.QUOTE + " = " + "'" + itemGroup.SearchValue + "'";
                                        }
                                        else
                                        {
                                            groupQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE +
                                                "." + PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " = " + "'" + itemGroup.SearchValue + "'";
                                        }
                                    }
                                }

                                break;

                            case SearchCondition.EqualExact:
                                var typeEqualExact = itemGroup.SearchValue.GetType().FullName;

                                if (typeEqualExact == "System.DateTime")
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        groupQuery += PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE +
                                            " = " + $"to_timestamp('{itemGroup.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";

                                    }
                                    else
                                    {
                                        groupQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                            PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " = " +
                                            $"to_timestamp('{itemGroup.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                                    }
                                }
                                else if (typeEqualExact == "System.String")
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        groupQuery += PostgresConst.QUOTE + itemGroup.SearchField +
                                            PostgresConst.QUOTE + " = " + "'" + itemGroup.SearchValue + "'";
                                    }
                                    else
                                    {
                                        groupQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE +
                                            "." + PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + "="
                                            + "'" + itemGroup.SearchValue + "'";
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        groupQuery += PostgresConst.QUOTE + itemGroup.SearchField +
                                            PostgresConst.QUOTE + " = " + "'" + itemGroup.SearchValue + "'";
                                    }
                                    else
                                    {
                                        groupQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE +
                                            "." + PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " = " + "'" + itemGroup.SearchValue + "'";
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
                                                        subQueryEqual += " OR " + PostgresConst.QUOTE + itemGroup.SearchField +
                                                            PostgresConst.QUOTE + " != " + "N'" + arrayValueEqual[i] + "' ";
                                                    }
                                                    else
                                                    {
                                                        subQueryEqual += " OR " + PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." + PostgresConst.QUOTE +
                                                            itemGroup.SearchField + PostgresConst.QUOTE + " != " + "N'" + arrayValueEqual[i] + "' ";
                                                    }
                                                }
                                                else
                                                {
                                                    if (string.IsNullOrEmpty(tableName))
                                                    {
                                                        subQueryEqual += PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " != " + "N'" + arrayValueEqual[i] + "'";
                                                    }
                                                    else
                                                    {
                                                        subQueryEqual += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." + PostgresConst.QUOTE +
                                                            itemGroup.SearchField + PostgresConst.QUOTE +
                                                            " != " + "N'" + arrayValueEqual[i] + "'";
                                                    }
                                                }

                                                break;
                                            default:
                                                break;
                                        }
                                    }

                                    subQueryEqual = "( " + subQueryEqual + " )";
                                    groupQuery = groupQuery + subQueryEqual;
                                }
                                else
                                {
                                    var type = itemGroup.SearchValue.GetType().FullName;

                                    if (type == "System.DateTime")
                                    {
                                        if (string.IsNullOrEmpty(tableName))
                                        {
                                            groupQuery += PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " != " +
                                                $"to_timestamp('{itemGroup.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                                            //"convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                            //System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                        }
                                        else
                                        {
                                            groupQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." + PostgresConst.QUOTE +
                                                itemGroup.SearchField + PostgresConst.QUOTE + " != " +
                                                $"to_timestamp('{itemGroup.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                                            //"convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                            //System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                        }
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(tableName))
                                        {
                                            groupQuery += PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " != " + "'" + itemGroup.SearchValue + "'";
                                        }
                                        else
                                        {
                                            groupQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." + PostgresConst.QUOTE +
                                                itemGroup.SearchField + PostgresConst.QUOTE + " != " + "'" + itemGroup.SearchValue + "'";
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
                                                subQueryEqual += " OR " + PostgresConst.QUOTE + itemGroup.SearchField +
                                                    PostgresConst.QUOTE + "::text" + $" {PostgresConst.ILIKE} N'%" + arrayValueEqual[i] + "%' ";
                                            }
                                            else
                                            {
                                                subQueryEqual += " OR " + PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "."
                                                    + PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + "::text" +
                                                    $" {PostgresConst.ILIKE} N'%" + arrayValueEqual[i] + "%' ";
                                            }
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(tableName))
                                            {
                                                subQueryEqual += PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + "::text" +
                                                    $" {PostgresConst.ILIKE} N'%" + arrayValueEqual[i] + "%'";
                                            }
                                            else
                                            {
                                                subQueryEqual += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                                    PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + "::text" +
                                                    $" {PostgresConst.ILIKE} N'%" + arrayValueEqual[i] + "%'";
                                            }
                                        }
                                    }

                                    subQueryEqual = "( " + subQueryEqual + " )";
                                    groupQuery = groupQuery + subQueryEqual;
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        groupQuery += PostgresConst.QUOTE + itemGroup.SearchField +
                                            PostgresConst.QUOTE + "::text" + $" {PostgresConst.ILIKE} N'%" + itemGroup.SearchValue + "%'";
                                    }
                                    else
                                    {
                                        groupQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                            PostgresConst.QUOTE + itemGroup.SearchField +
                                            PostgresConst.QUOTE + "::text" + $" {PostgresConst.ILIKE} N'%" + itemGroup.SearchValue + "%'";
                                    }
                                }

                                break;
                            case SearchCondition.GreaterThan:

                                var typeGreaterThan = itemGroup.SearchValue.GetType().FullName;

                                if (typeGreaterThan == "System.DateTime")
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        groupQuery += PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " > " +
                                            $"to_timestamp('{itemGroup.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                                        //"convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                        //System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }
                                    else
                                    {
                                        groupQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." + PostgresConst.QUOTE +
                                            itemGroup.SearchField + PostgresConst.QUOTE + " > " +
                                            $"to_timestamp('{itemGroup.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                                        //"convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                        //System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        groupQuery += PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " > " + itemGroup.SearchValue;
                                    }
                                    else
                                    {
                                        groupQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                            PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " > " + itemGroup.SearchValue;
                                    }
                                }


                                break;
                            case SearchCondition.GreaterThanEqual:

                                var typeGreaterThanEqual = itemGroup.SearchValue.GetType().FullName;

                                if (typeGreaterThanEqual == "System.DateTime")
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        groupQuery += PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " >= " +
                                            $"to_timestamp('{itemGroup.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                                        //"convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                        //System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }
                                    else
                                    {
                                        groupQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." + PostgresConst.QUOTE +
                                            itemGroup.SearchField + PostgresConst.QUOTE + " >= " +
                                            $"to_timestamp('{itemGroup.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                                        //"convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                        //System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        groupQuery += PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " >= " + itemGroup.SearchValue;
                                    }
                                    else
                                    {
                                        groupQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                            PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " >= " + itemGroup.SearchValue;
                                    }

                                }

                                break;
                            case SearchCondition.LessThan:

                                var typeLessThan = itemGroup.SearchValue.GetType().FullName;

                                if (typeLessThan == "System.DateTime")
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        groupQuery += PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " < " +
                                            $"to_timestamp('{itemGroup.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                                        //"convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                        //System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }
                                    else
                                    {
                                        groupQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                            PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " < " +
                                            $"to_timestamp('{itemGroup.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                                        //"convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                        //System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        groupQuery += PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " < " + itemGroup.SearchValue;
                                    }
                                    else
                                    {
                                        groupQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                            PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " < " + itemGroup.SearchValue;
                                    }
                                }

                                break;
                            case SearchCondition.LessThanEqual:

                                var typeLessThanEqual = itemGroup.SearchValue.GetType().FullName;

                                if (typeLessThanEqual == "System.DateTime")
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        groupQuery += PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " <= " +
                                            $"to_timestamp('{itemGroup.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                                        //"convert(datetime, '" +
                                        //itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }
                                    else
                                    {
                                        groupQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                            PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " <= " +
                                           $"to_timestamp('{itemGroup.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                                        //"convert(datetime, '" + itemGroup.SearchValue.ToString("MM/dd/yyyy HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        groupQuery += PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " <= " + itemGroup.SearchValue;
                                    }
                                    else
                                    {
                                        groupQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                            PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " <= " + itemGroup.SearchValue;
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
                                        groupQuery += PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " >= " +
                                            $"to_timestamp('{fromDate.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                                        //"convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                        //System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                        //CAST( '" + fromDate + "' as datetime)";
                                    }
                                    else
                                    {
                                        groupQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                            PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " >= " +
                                            $"to_timestamp('{fromDate.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                                        //"convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                        //    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
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
                                            groupQuery += "( " + PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " >= " +
                                            //    "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                            //System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)" 
                                            $"to_timestamp('{fromDate.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')"
                                            + " AND " +
                                               PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " <= " +
                                               $"to_timestamp('{toDate.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')" +
                                               $")";
                                            //   "convert(datetime, '" + toDate.ToString("MM/dd/yyyy HH:mm:ss",
                                            //System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101))";
                                        }
                                        else
                                        {
                                            groupQuery += "( " + PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                                PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " >= " +
                                                $"to_timestamp('{fromDate.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')"
                                            //"convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                            //System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)" 
                                            + " AND "
                                                + PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                                PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " <= " +
                                                $"to_timestamp('{toDate.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')" +
                                                $")";
                                            //    "convert(datetime, '" + toDate.ToString("MM/dd/yyyy HH:mm:ss",
                                            //System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101))";
                                        }
                                    }
                                    else
                                    {
                                        if (string.IsNullOrEmpty(tableName))
                                        {
                                            groupQuery += PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " >= " +
                                                $"to_timestamp('{fromDate.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                                            //    "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                            //System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                        }
                                        else
                                        {
                                            groupQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                                PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " >= " +
                                            $"to_timestamp('{fromDate.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                                            //    "convert(datetime, '" + fromDate.ToString("MM/dd/yyyy HH:mm:ss",
                                            //System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                        }
                                    }
                                }

                                break;

                            default:
                                break;
                        }
                    }

                    groupQuery = "( " + groupQuery + " )";
                    if (index > 0)
                    {
                        rawQuery += " OR " + groupQuery;
                    }
                    else
                    {
                        rawQuery += groupQuery;
                    }

                    index++;
                }

                //rawQuery = "( " + rawQuery + " )";
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
                        rawQuery += string.IsNullOrEmpty(tableName) ? " " + PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " IS NULL " :
                            PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                            PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " IS NULL ";
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

                            rawQuery += string.IsNullOrEmpty(tableName) ? " " + PostgresConst.QUOTE +
                                search.SearchField + PostgresConst.QUOTE + " IN (" + inVal + ") " :
                                PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " IN (" + inVal + ") ";
                        }
                        else
                        {
                            rawQuery += string.IsNullOrEmpty(tableName) ? " " + PostgresConst.QUOTE +
                                search.SearchField + PostgresConst.QUOTE + " IN ('" + search.SearchValue + "') " :
                               PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                               PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " IN ('" + search.SearchValue + "') ";
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

                            rawQuery += string.IsNullOrEmpty(tableName) ? " " + PostgresConst.QUOTE +
                                search.SearchField + PostgresConst.QUOTE + " NOT IN (" + inVal + ") " :
                                PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " NOT IN (" + inVal + ") ";
                        }
                        else
                        {
                            rawQuery += string.IsNullOrEmpty(tableName) ? " " + PostgresConst.QUOTE +
                                search.SearchField + PostgresConst.QUOTE + " NOT IN ('" + search.SearchValue + "') " :
                               PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                               PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " NOT IN ('" + search.SearchValue + "') ";
                        }

                        break;
                    case SearchCondition.IsNotNull:
                        rawQuery += string.IsNullOrEmpty(tableName) ? " " + PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " IS NOT NULL " :
                            PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                            PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " IS NOT NULL ";
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
                                            subQueryEqual += tableName == null ? " OR " + PostgresConst.QUOTE + search.SearchField +
                                                PostgresConst.QUOTE + $" {PostgresConst.ILIKE} " + "N'" + arrayValueEqual[i] + "' " :
                                                " OR " + PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." + PostgresConst.QUOTE +
                                                search.SearchField + PostgresConst.QUOTE + $" {PostgresConst.ILIKE} " + "N'" + arrayValueEqual[i] + "' ";
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(tableName))
                                            {
                                                subQueryEqual += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + $" {PostgresConst.ILIKE} " + "N'" + arrayValueEqual[i] + "'";
                                            }
                                            else
                                            {
                                                subQueryEqual += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                                    PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + $" {PostgresConst.ILIKE} " + "N'" + arrayValueEqual[i] + "'";
                                            }
                                        }

                                        break;

                                    default:
                                        if (i > 0)
                                        {
                                            subQueryEqual += tableName == null ? " OR " + PostgresConst.QUOTE + search.SearchField +
                                                PostgresConst.QUOTE + " = " + arrayValueEqual[i] :
                                                " OR " + PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." + PostgresConst.QUOTE +
                                                search.SearchField + PostgresConst.QUOTE + " = " + arrayValueEqual[i];
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(tableName))
                                            {
                                                subQueryEqual += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " = " + arrayValueEqual[i];
                                            }
                                            else
                                            {
                                                subQueryEqual += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                                    PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " = " + arrayValueEqual[i];
                                            }
                                        }
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
                                    rawQuery += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " = " +
                                        $"to_timestamp('{search.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                                }
                                else
                                {
                                    rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." + PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " = " +
                                         $"to_timestamp('{search.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                                }
                            }
                            else if (type == "System.Boolean")
                            {
                                if (string.IsNullOrEmpty(tableName))
                                {
                                    rawQuery += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " = " + search.SearchValue;
                                }
                                else
                                {
                                    if (search.SearchValue is null)
                                    {
                                        rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                            PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " IS NULL ";
                                    }
                                    else
                                    {
                                        rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." + PostgresConst.QUOTE +
                                            search.SearchField + PostgresConst.QUOTE + " = " + search.SearchValue;
                                    }
                                }
                            }
                            else if (type == "System.String")
                            {
                                if (string.IsNullOrEmpty(tableName))
                                {
                                    rawQuery += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE +
                                        $" {PostgresConst.ILIKE} " + "N'" + search.SearchValue + "'";
                                }
                                else
                                {
                                    if (search.SearchValue is null)
                                    {
                                        rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                            PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " IS NULL ";
                                    }
                                    else
                                    {
                                        rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                            PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + $" {PostgresConst.ILIKE} " + "N'" + search.SearchValue + "'";
                                    }
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(tableName))
                                {
                                    rawQuery += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE +
                                        " = " + "N'" + search.SearchValue + "'";
                                }
                                else
                                {
                                    if (search.SearchValue is null)
                                    {
                                        rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                            PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " IS NULL ";
                                    }
                                    else
                                    {
                                        double checkumber;
                                        var isNumber = double.TryParse(search.SearchValue.ToString(), out checkumber);

                                        if (isNumber)
                                        {
                                            rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                                PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " = " + search.SearchValue;
                                        }
                                        else
                                        {
                                            rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                           PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " = " + "N'" + search.SearchValue + "'";
                                        }
                                    }
                                }
                            }
                        }

                        break;
                    case SearchCondition.EqualExact:

                        var typeEqualExact = search.SearchValue == null ? "" : search.SearchValue.GetType().FullName;

                        if (typeEqualExact == "System.DateTime")
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " = " +
                                    $"to_timestamp('{search.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                            }
                            else
                            {
                                rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." + PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " = " +
                                     $"to_timestamp('{search.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                            }
                        }
                        else if (typeEqualExact == "System.Boolean")
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " = " + search.SearchValue;
                            }
                            else
                            {
                                if (search.SearchValue is null)
                                {
                                    rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                        PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " IS NULL ";
                                }
                                else
                                {
                                    rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." + PostgresConst.QUOTE +
                                        search.SearchField + PostgresConst.QUOTE + " = " + search.SearchValue;
                                }
                            }
                        }
                        else if (typeEqualExact == "System.String")
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE +
                                    "='" + search.SearchValue + "'";
                            }
                            else
                            {
                                if (search.SearchValue is null)
                                {
                                    rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                        PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " IS NULL ";
                                }
                                else
                                {
                                    rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                        PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + "='" + search.SearchValue + "'";
                                }
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE +
                                    " = " + "N'" + search.SearchValue + "'";
                            }
                            else
                            {
                                if (search.SearchValue is null)
                                {
                                    rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                        PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " IS NULL ";
                                }
                                else
                                {
                                    double checkumber;
                                    var isNumber = double.TryParse(search.SearchValue.ToString(), out checkumber);

                                    if (isNumber)
                                    {
                                        rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                            PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " = " + search.SearchValue;
                                    }
                                    else
                                    {
                                        rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                       PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " = " + "N'" + search.SearchValue + "'";
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
                                                subQueryEqual += " OR " + PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " != " + "N'" + arrayValueEqual[i] + "' ";
                                            }
                                            else
                                            {
                                                subQueryEqual += " OR " + PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." + PostgresConst.QUOTE +
                                                    search.SearchField + PostgresConst.QUOTE + " != " + "N'" + arrayValueEqual[i] + "' ";
                                            }
                                        }
                                        else
                                        {
                                            if (string.IsNullOrEmpty(tableName))
                                            {
                                                subQueryEqual += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " != " + "N'" + arrayValueEqual[i] + "'";
                                            }
                                            else
                                            {
                                                subQueryEqual += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                                    PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " != " + "N'" + arrayValueEqual[i] + "'";
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
                                    rawQuery += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " != " +
                                         $"to_timestamp('{search.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                                    //"convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                    //    System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                }
                                else
                                {
                                    rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." + PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " != " +
                                         $"to_timestamp('{search.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                                    //"convert(datetime, '" + search.SearchValue.ToString("MM/dd/yyyy HH:mm:ss",
                                    //System.Globalization.CultureInfo.GetCultureInfo("en-US")) + "', 101)";
                                }
                            }
                            else if (type == "System.Boolean")
                            {
                                if (string.IsNullOrEmpty(tableName))
                                {
                                    rawQuery += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " != " + search.SearchValue;
                                }
                                else
                                {
                                    if (search.SearchValue is null)
                                    {
                                        rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                            PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " IS NULL ";
                                    }
                                    else
                                    {
                                        rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." + PostgresConst.QUOTE +
                                            search.SearchField + PostgresConst.QUOTE + " != " + search.SearchValue;
                                    }
                                }
                            }
                            else if (type == "System.String")
                            {
                                if (string.IsNullOrEmpty(tableName))
                                {
                                    rawQuery += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE +
                                        " != '" + search.SearchValue + "'";
                                }
                                else
                                {
                                    if (search.SearchValue is null)
                                    {
                                        rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                            PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " IS NULL ";
                                    }
                                    else
                                    {
                                        rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                            PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " != '" + search.SearchValue + "'";
                                    }
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(tableName))
                                {
                                    rawQuery += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " != N'" + search.SearchValue + "'";
                                }
                                else
                                {
                                    double checkumber;
                                    var isNumber = double.TryParse(search.SearchValue.ToString(), out checkumber);

                                    if (isNumber)
                                    {
                                        rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                            PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " = " + search.SearchValue;
                                    }
                                    else
                                    {
                                        rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                        PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " != N'" + search.SearchValue + "'";
                                    }
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
                                        subQueryEqual += " OR " + PostgresConst.QUOTE + search.SearchField +
                                            PostgresConst.QUOTE + "::text" + $" {PostgresConst.ILIKE} N'%" + arrayValueEqual[i] + "%' ";
                                    }
                                    else
                                    {
                                        subQueryEqual += " OR " + PostgresConst.QUOTE + tableName + PostgresConst.QUOTE +
                                            "." + PostgresConst.QUOTE + search.SearchField +
                                            PostgresConst.QUOTE + "::text" + $" {PostgresConst.ILIKE} N'%" + arrayValueEqual[i] + "%' ";
                                    }
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(tableName))
                                    {
                                        subQueryEqual += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + "::text" +
                                            $" {PostgresConst.ILIKE} N'%" + arrayValueEqual[i] + "%'";
                                    }
                                    else
                                    {
                                        subQueryEqual += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                            PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + "::text" +
                                            $" {PostgresConst.ILIKE} N'%" + arrayValueEqual[i] + "%'";
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
                                rawQuery += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + "::text" +
                                    $" {PostgresConst.ILIKE} N'%" + search.SearchValue + "%'";
                            }
                            else
                            {
                                rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                    PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + "::text" +
                                    $" {PostgresConst.ILIKE} N'%" + search.SearchValue + "%'";
                            }
                        }

                        break;
                    case SearchCondition.GreaterThan:

                        var typeGreaterThan = search.SearchValue.GetType().FullName;

                        if (typeGreaterThan == "System.DateTime")
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " > " +
                                     $"to_timestamp('{search.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                            }
                            else
                            {
                                rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                    PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " > " +
                                     $"to_timestamp('{search.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " > " + search.SearchValue;
                            }
                            else
                            {
                                rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                    PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " > " + search.SearchValue;
                            }
                        }


                        break;
                    case SearchCondition.GreaterThanEqual:

                        var typeGreaterThanEqual = search.SearchValue.GetType().FullName;

                        if (typeGreaterThanEqual == "System.DateTime")
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " >= " +
                                     $"to_timestamp('{search.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                            }
                            else
                            {
                                rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                    PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " >= " +
                                     $"to_timestamp('{search.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                            }

                        }
                        else
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " >= " + search.SearchValue;
                            }
                            else
                            {
                                rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." + PostgresConst.QUOTE +
                                    search.SearchField + PostgresConst.QUOTE + " >= " + search.SearchValue;
                            }
                        }

                        break;
                    case SearchCondition.LessThan:

                        var typeLessThan = search.SearchValue.GetType().FullName;

                        if (typeLessThan == "System.DateTime")
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " < " +
                                     $"to_timestamp('{search.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                            }
                            else
                            {
                                rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                    PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " < " +
                                     $"to_timestamp('{search.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " < " + search.SearchValue;
                            }
                            else
                            {
                                rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                    PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " < " + search.SearchValue;
                            }
                        }

                        break;
                    case SearchCondition.LessThanEqual:

                        var typeLessThanEqual = search.SearchValue.GetType().FullName;

                        if (typeLessThanEqual == "System.DateTime")
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " <= " +
                                     $"to_timestamp('{search.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                            }
                            else
                            {
                                rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                    PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " <= " +
                                     $"to_timestamp('{search.SearchValue.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                            }
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(tableName))
                            {
                                rawQuery += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " <= " + search.SearchValue;
                            }
                            else
                            {
                                rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                    PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " <= " + search.SearchValue;
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
                                rawQuery += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " >= " +
                                     $"to_timestamp('{fromDate.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                            }
                            else
                            {
                                rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                                    PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " >= " +
                                     $"to_timestamp('{fromDate.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                            }
                        }
                        else
                        {
                            if (arrayValue[1] != null)
                            {
                                var toDate = DateTime.Parse(arrayValue[1].ToString());
                                if (string.IsNullOrEmpty(tableName))
                                {
                                    rawQuery += "( " + PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " >= " +
                                         $"to_timestamp('{fromDate.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')"
                                    + " AND " +
                                       PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " <= " +
                                        $"to_timestamp('{toDate.ToString()}', '{PostgresConst.TIME_STAMP_FORMAT}')" +
                                        $")";
                                }
                                else
                                {
                                    rawQuery += "( " + PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." + PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " >= " +
                                         $"to_timestamp('{fromDate.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')"
                                    + " AND " + PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." + PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " <= " +
                                     $"to_timestamp('{toDate.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')" +
                                        $")";
                                }
                            }
                            else
                            {
                                if (string.IsNullOrEmpty(tableName))
                                {
                                    rawQuery += PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " >= " +
                                         $"to_timestamp('{fromDate.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                                }
                                else
                                {
                                    rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." + PostgresConst.QUOTE + search.SearchField + PostgresConst.QUOTE + " >= " +
                                         $"to_timestamp('{fromDate.ToString("dd/MM/yyyy HH:mm:ss")}', '{PostgresConst.TIME_STAMP_FORMAT}')";
                                }
                            }
                        }

                        break;

                    case SearchCondition.DayPart:
                        if (string.IsNullOrEmpty(tableName))
                        {
                            rawQuery += $" date_part('day', {PostgresConst.QUOTE}{search.SearchField}{PostgresConst.QUOTE})  = {search.SearchValue}";
                            //+" = " + itemGroup.SearchValue;
                            //$"to_timestamp({itemGroup.SearchValue.ToString()}, {PostgresConst.TIME_STAMP_FORMAT})";
                        }
                        else
                        {
                            rawQuery += $" date_part('day',{PostgresConst.QUOTE}{tableName}{PostgresConst.QUOTE}.{PostgresConst.QUOTE}" +
                                $"{search.SearchField}{PostgresConst.QUOTE})  = {search.SearchValue}";
                            //rawQuery += PostgresConst.QUOTE + tableName + PostgresConst.QUOTE + "." +
                            //          PostgresConst.QUOTE + itemGroup.SearchField + PostgresConst.QUOTE + " >= " +
                            //      $"to_timestamp({itemGroup.SearchValue.ToString()}, {PostgresConst.TIME_STAMP_FORMAT})";
                        }
                        break;
                    case SearchCondition.MonthPart:
                        if (string.IsNullOrEmpty(tableName))
                        {
                            rawQuery += $" date_part('month', {PostgresConst.QUOTE}{search.SearchField}{PostgresConst.QUOTE})  = {search.SearchValue}";
                        }
                        else
                        {
                            rawQuery += $" date_part('month',{PostgresConst.QUOTE}{tableName}{PostgresConst.QUOTE}.{PostgresConst.QUOTE}" +
                                $"{search.SearchField}{PostgresConst.QUOTE})  = {search.SearchValue}";
                        }
                        break;
                    case SearchCondition.DatePart:
                        if (string.IsNullOrEmpty(tableName))
                        {
                            rawQuery += $" {PostgresConst.QUOTE}{search.SearchField}{PostgresConst.QUOTE}::date  = {search.SearchValue}";
                        }
                        else
                        {
                            rawQuery += $"{PostgresConst.QUOTE}{tableName}{PostgresConst.QUOTE}.{PostgresConst.QUOTE}" +
                                $"{search.SearchField}{PostgresConst.QUOTE}::date  = to_date('{search.SearchValue}', '{PostgresConst.DATE_FORMAT}')";
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
            var rawQueryPaging = @"WITH TempResult AS(
                                {2}
                            ), TempCount AS(
                                SELECT COUNT(*) AS " + PostgresConst.QUOTE + "TotalRow" + PostgresConst.QUOTE + @"FROM TempResult
                            )
                            SELECT * FROM TempResult, TempCount
                            ORDER BY TempResult.{3}
                                OFFSET({1} - 1) * {0}
                                   FETCH NEXT {0} ROWS ONLY";


            var aliasRootTbl = "r";
            var selectedCols = "";
            //convert select field to jarray
            List<string> fieldSelects = null;
            if (fields != null)
            {
                fieldSelects = fields.Where(x => !x.Contains(".")).ToList();

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

            var rawQuery = string.IsNullOrEmpty(selectedCols) ? "SELECT " + aliasRootTbl + ".* {0} FROM " +
                PostgresConst.QUOTE + rootTbl + PostgresConst.QUOTE + " AS " + aliasRootTbl :
                "SELECT " + selectedCols + " {0} FROM " + PostgresConst.QUOTE + rootTbl + PostgresConst.QUOTE + " AS " + aliasRootTbl;

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

                    selectCols += ", " + aliasTbl + "." + PostgresConst.QUOTE + refTbl.ColumnName + PostgresConst.QUOTE + " AS " +
                        PostgresConst.QUOTE + aliasName + PostgresConst.QUOTE;

                    conditionJoin += " LEFT JOIN " + PostgresConst.QUOTE + refTbl.TableName + PostgresConst.QUOTE + " AS " + aliasTbl + " ON " +
                        aliasRootTbl + "." + PostgresConst.QUOTE + refTbl.ForeignKeyCol + PostgresConst.QUOTE + "=" + aliasTbl + $".\"{TableColumnConst.CODE_COL}\"";

                    if (refTbl.FilterType != "All" || string.IsNullOrEmpty(refTbl.FilterType))
                    {
                        conditionJoin += "AND " + aliasRootTbl + "." + PostgresConst.QUOTE + TableColumnConst.COMPANY_CODE_COL + PostgresConst.QUOTE + " = " +
                            aliasTbl + "." + PostgresConst.QUOTE + TableColumnConst.COMPANY_CODE_COL + PostgresConst.QUOTE + " ";
                    }

                    count++;
                }
            }

            sortDTO = sortDTO == null ? new SortDTO() { SortBy = TableColumnConst.CODE_COL, Sort = "desc" } : sortDTO;
            var sortQuery = PostgresConst.QUOTE + sortDTO.SortBy + PostgresConst.QUOTE + " " + sortDTO.Sort;

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
        /// <param name="size"></param>
        /// <param name="offset"></param>
        /// <param name="rootTbl"></param>
        /// <param name="refTblList"></param>
        /// <param name="SearchFieldList"></param>
        /// <param name="sortDTO"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static string BuildRawCountQueryPagingV3(int size, int offset, string rootTbl, List<ReferenceTable> refTblList,
            List<SearchDTO> SearchFieldList, SortDTO sortDTO, List<string> fields = null)
        {
            var rawQueryPaging = @"WITH TempResult AS(
                                {2}
                                ORDER BY {4}.{3}
                                OFFSET {1}
                                   FETCH NEXT {0} ROWS ONLY
                            ), TempCount AS(
                                SELECT COUNT(*) AS " + PostgresConst.QUOTE + "TotalRow" + PostgresConst.QUOTE + @"FROM TempResult
                            )
                            SELECT * FROM TempCount";


            var aliasRootTbl = "r";
            var selectedCols = "";
            //convert select field to jarray
            List<string> fieldSelects = null;
            if (fields != null)
            {
                fieldSelects = fields.Where(x => !x.Contains(".")).ToList();

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

            var rawQuery = string.IsNullOrEmpty(selectedCols) ? "SELECT " + aliasRootTbl + ".* {0} FROM " +
                PostgresConst.QUOTE + rootTbl + PostgresConst.QUOTE + " AS " + aliasRootTbl :
                "SELECT " + selectedCols + " FROM " + PostgresConst.QUOTE + rootTbl + PostgresConst.QUOTE + " AS " + aliasRootTbl;

            sortDTO = sortDTO == null ? new SortDTO() { SortBy = TableColumnConst.CODE_COL, Sort = "desc" } : sortDTO;
            var sortQuery = PostgresConst.QUOTE + sortDTO.SortBy + PostgresConst.QUOTE + " " + sortDTO.Sort;

            var whereCondition = BuildRawQueryCondition(SearchFieldList, aliasRootTbl);

            rawQuery += string.IsNullOrEmpty(whereCondition) ? " " : " WHERE " + whereCondition;

            rawQueryPaging = string.Format(rawQueryPaging, size, offset, rawQuery, sortQuery, aliasRootTbl);

            return rawQueryPaging;
        }

        /// <summary>
        /// Build Raw Query Paging V2 (Not Return Total)
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="rootTbl"></param>
        /// <param name="refTblList"></param>
        /// <param name="SearchFieldList"></param>
        /// <param name="sortDTO"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static string BuildRawQueryPagingV2(int pageSize, int pageIndex, string rootTbl, List<ReferenceTable> refTblList,
            List<SearchDTO> SearchFieldList, SortDTO sortDTO, List<string> fields = null)
        {
            var aliasRootTbl = "r";
            var selectedCols = "";
            //convert select field to jarray
            List<string> fieldSelects = null;
            if (fields != null)
            {
                fieldSelects = fields.Where(x => !x.Contains(".")).ToList();

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

            var rawQuery = string.IsNullOrEmpty(selectedCols)
                ? "SELECT " + aliasRootTbl + ".* {0} FROM " +
                PostgresConst.QUOTE + rootTbl + PostgresConst.QUOTE + " AS " + aliasRootTbl
                : "SELECT " + selectedCols + " {0} FROM " +
                PostgresConst.QUOTE + rootTbl + PostgresConst.QUOTE + " AS " + aliasRootTbl;

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

                    selectCols += ", " + aliasTbl + "." + PostgresConst.QUOTE + refTbl.ColumnName + PostgresConst.QUOTE + " AS " +
                        PostgresConst.QUOTE + aliasName + PostgresConst.QUOTE;

                    conditionJoin += " LEFT JOIN " + PostgresConst.QUOTE + refTbl.TableName + PostgresConst.QUOTE + " AS " + aliasTbl + " ON " +
                        aliasRootTbl + "." + PostgresConst.QUOTE + refTbl.ForeignKeyCol + PostgresConst.QUOTE + "=" + aliasTbl + $".\"{TableColumnConst.CODE_COL}\"";

                    if (refTbl.FilterType != "All" || string.IsNullOrEmpty(refTbl.FilterType))
                    {
                        conditionJoin += "AND " + aliasRootTbl + "." + PostgresConst.QUOTE + TableColumnConst.COMPANY_CODE_COL + PostgresConst.QUOTE + " = " +
                            aliasTbl + "." + PostgresConst.QUOTE + TableColumnConst.COMPANY_CODE_COL + PostgresConst.QUOTE + " ";
                    }

                    count++;
                }
            }

            sortDTO = sortDTO ?? new SortDTO() { SortBy = TableColumnConst.CODE_COL, Sort = "desc" };
            var sortQuery = PostgresConst.QUOTE + sortDTO.SortBy + PostgresConst.QUOTE + " " + sortDTO.Sort;

            var whereCondition = BuildRawQueryCondition(SearchFieldList, aliasRootTbl);

            rawQuery = string.Format(rawQuery, selectCols);
            rawQuery += conditionJoin;
            rawQuery += string.IsNullOrEmpty(whereCondition) ? " " : " WHERE " + whereCondition;
            rawQuery += $" ORDER BY {aliasRootTbl}.{sortQuery}";
            rawQuery += $" OFFSET({pageIndex} - 1) * {pageSize} FETCH NEXT {pageSize} ROWS ONLY";

            return rawQuery;
        }

        /// <summary>
        /// Build Raw Query Paging V3 (Not Return Total)
        /// </summary>
        /// <param name="pageSize"></param>
        /// <param name="pageIndex"></param>
        /// <param name="rootTbl"></param>
        /// <param name="refTblList"></param>
        /// <param name="SearchFieldList"></param>
        /// <param name="sortDTO"></param>
        /// <param name="fields"></param>
        /// <returns></returns>
        public static string BuildRawQueryPagingV3(int pageSize, int pageIndex, int offset, string rootTbl, List<ReferenceTable> refTblList,
            List<SearchDTO> SearchFieldList, SortDTO sortDTO, List<string> fields = null)
        {
            var aliasRootTbl = "r";
            var selectedCols = "";
            //convert select field to jarray
            List<string> fieldSelects = null;
            if (fields != null)
            {
                fieldSelects = fields.Where(x => !x.Contains(".")).ToList();

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

            var rawQuery = string.IsNullOrEmpty(selectedCols)
                ? "SELECT " + aliasRootTbl + ".* {0} FROM " +
                PostgresConst.QUOTE + rootTbl + PostgresConst.QUOTE + " AS " + aliasRootTbl
                : "SELECT " + selectedCols + " {0} FROM " +
                PostgresConst.QUOTE + rootTbl + PostgresConst.QUOTE + " AS " + aliasRootTbl;

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

                    selectCols += ", " + aliasTbl + "." + PostgresConst.QUOTE + refTbl.ColumnName + PostgresConst.QUOTE + " AS " +
                        PostgresConst.QUOTE + aliasName + PostgresConst.QUOTE;

                    conditionJoin += " LEFT JOIN " + PostgresConst.QUOTE + refTbl.TableName + PostgresConst.QUOTE + " AS " + aliasTbl + " ON " +
                        aliasRootTbl + "." + PostgresConst.QUOTE + refTbl.ForeignKeyCol + PostgresConst.QUOTE + "=" + aliasTbl + $".\"{TableColumnConst.CODE_COL}\"";

                    if (refTbl.FilterType != "All" || string.IsNullOrEmpty(refTbl.FilterType))
                    {
                        conditionJoin += "AND " + aliasRootTbl + "." + PostgresConst.QUOTE + TableColumnConst.COMPANY_CODE_COL + PostgresConst.QUOTE + " = " +
                            aliasTbl + "." + PostgresConst.QUOTE + TableColumnConst.COMPANY_CODE_COL + PostgresConst.QUOTE + " ";
                    }

                    count++;
                }
            }

            sortDTO = sortDTO ?? new SortDTO() { SortBy = TableColumnConst.CODE_COL, Sort = "desc" };
            var sortQuery = PostgresConst.QUOTE + sortDTO.SortBy + PostgresConst.QUOTE + " " + sortDTO.Sort;

            var whereCondition = BuildRawQueryCondition(SearchFieldList, aliasRootTbl);

            rawQuery = string.Format(rawQuery, selectCols);
            rawQuery += conditionJoin;
            rawQuery += string.IsNullOrEmpty(whereCondition) ? " " : " WHERE " + whereCondition;
            rawQuery += $" ORDER BY {aliasRootTbl}.{sortQuery}";
            rawQuery += $" OFFSET({pageIndex} - 1) * {pageSize} + {offset} FETCH NEXT {pageSize} ROWS ONLY";

            return rawQuery;
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
            List<SearchDTO> SearchFieldList, SortDTO sortDTO, List<string> fields = null, int limit = 0)
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
                "SELECT " + aliasRootTbl + ".* {0} FROM " + PostgresConst.QUOTE + rootTbl + PostgresConst.QUOTE + " AS " +
                aliasRootTbl :
                 "SELECT " + selectedCols + " {0} FROM " + PostgresConst.QUOTE + rootTbl +
                 PostgresConst.QUOTE + " AS " + aliasRootTbl;

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

                    selectCols += ", " + aliasTbl + "." + PostgresConst.QUOTE + refTbl.ColumnName +
                        PostgresConst.QUOTE + " AS " + PostgresConst.QUOTE + aliasName + PostgresConst.QUOTE;

                    conditionJoin += " LEFT JOIN " + PostgresConst.QUOTE + refTbl.TableName + PostgresConst.QUOTE + " AS " + aliasTbl + " ON " +
                        aliasRootTbl + "." + PostgresConst.QUOTE + refTbl.ForeignKeyCol + PostgresConst.QUOTE + "=" + aliasTbl + ".\"Code\"";

                    if (refTbl.OptionJoin != null && refTbl.OptionJoin.Length == 2)
                    {
                        conditionJoin += "AND " + aliasRootTbl + "." + PostgresConst.QUOTE + refTbl.OptionJoin[0] + PostgresConst.QUOTE + " = " +
                           aliasTbl + "." + PostgresConst.QUOTE + refTbl.OptionJoin[1] + PostgresConst.QUOTE + " ";
                    }

                    if (refTbl.FilterType != "All" || string.IsNullOrEmpty(refTbl.FilterType))
                    {
                        conditionJoin += " AND " + aliasRootTbl + "." + PostgresConst.QUOTE + TableColumnConst.COMPANY_CODE_COL + PostgresConst.QUOTE + " = " +
                            aliasTbl + "." + PostgresConst.QUOTE + TableColumnConst.COMPANY_CODE_COL + PostgresConst.QUOTE + " ";
                    }

                    count++;
                }
            }

            if (fields == null && sortDTO == null)
            {
                sortDTO = new SortDTO() { SortBy = TableColumnConst.CODE_COL, Sort = "desc" };
            }

            //sortDTO = sortDTO == null ? new SortDTO() { SortBy = TableColumnConst.CODE_COL, Sort = "desc" } : sortDTO;
            var sortQuery = sortDTO == null ? "" : "ORDER BY " + aliasRootTbl + "." + PostgresConst.QUOTE + sortDTO.SortBy + PostgresConst.QUOTE + " " + sortDTO.Sort;
            var limitQuery = limit > 0 ? $"limit {limit}" : "";

            var whereCondition = BuildRawQueryCondition(SearchFieldList, aliasRootTbl);

            rawQuery = string.Format(rawQuery, selectCols);
            rawQuery += conditionJoin;
            rawQuery += string.IsNullOrEmpty(whereCondition) ? " " + sortQuery : " WHERE " + whereCondition + " " + sortQuery + " " + limitQuery;

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
        /// <param name="limit"></param>
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
                        selectedCols += aliasRootTbl + "." + PostgresConst.QUOTE + item.ToString() + PostgresConst.QUOTE + ",";
                    }
                }
                //check selected column doesn't have company code, add selected company code to compare others
                if (!fieldSelects.Any(x => x.ToString() == TableColumnConst.COMPANY_CODE_COL))
                {
                    selectedCols += aliasRootTbl + "." + PostgresConst.QUOTE + TableColumnConst.COMPANY_CODE_COL.ToString() + PostgresConst.QUOTE + ",";
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

                    selectCols += ", " + aliasTbl + "." + PostgresConst.QUOTE + refTbl.ColumnName + PostgresConst.QUOTE + " AS " +
                        PostgresConst.QUOTE + aliasName + PostgresConst.QUOTE;

                    conditionJoin += " LEFT JOIN " + refTbl.TableName + " AS " + aliasTbl + " ON "
                        + aliasRootTbl + "." + PostgresConst.QUOTE + refTbl.ForeignKeyCol + PostgresConst.QUOTE + "=" + aliasTbl + $".\"{TableColumnConst.CODE_COL}\" ";

                    if (refTbl.FilterType != "All" || string.IsNullOrEmpty(refTbl.FilterType))
                    {
                        conditionJoin += "AND " + aliasRootTbl + "." + PostgresConst.QUOTE +
                            TableColumnConst.COMPANY_CODE_COL + PostgresConst.QUOTE + " = "
                            + aliasTbl + "." + PostgresConst.QUOTE + TableColumnConst.COMPANY_CODE_COL + PostgresConst.QUOTE + " ";
                    }

                    count++;
                }
            }

            sortDTO = sortDTO == null ? new SortDTO() { SortBy = TableColumnConst.CODE_COL, Sort = "desc" } : sortDTO;
            var sortQuery = "ORDER BY " + aliasRootTbl + "." + PostgresConst.QUOTE + sortDTO.SortBy + PostgresConst.QUOTE + " " + sortDTO.Sort;

            var whereCondition = BuildRawQueryCondition(SearchFieldList, aliasRootTbl);

            rawQuery = string.Format(rawQuery, selectCols);
            rawQuery += conditionJoin;
            rawQuery += string.IsNullOrEmpty(whereCondition) ? " " + sortQuery : " WHERE " + whereCondition + " " + sortQuery;
            rawQuery += $" LIMIT {limit}";

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
                rawQuery = "SELECT " + aliasRootTbl + ".* {0} FROM " + PostgresConst.QUOTE + rootTbl +
                    PostgresConst.QUOTE + " AS " + aliasRootTbl;
            }
            else
            {
                string selects = "";
                for (int i = 0; i < fields.Count; i++)
                {
                    if (i == 0 || i == (fields.Count - 1))
                    {
                        selects += " " + aliasRootTbl + "." + PostgresConst.QUOTE + fields[i] + PostgresConst.QUOTE;
                    }
                    else if (i < (fields.Count - 1) && i > 0)
                    {
                        selects += " " + aliasRootTbl + "." + PostgresConst.QUOTE + fields[i] + PostgresConst.QUOTE + " , ";
                    }
                }

                rawQuery = "SELECT " + selects + " FROM " + PostgresConst.QUOTE + rootTbl + PostgresConst.QUOTE + " AS " + aliasRootTbl;
            }

            var conditionJoin = "";
            var selectCols = "";

            if (fields == null && sortDTO == null)
            {
                sortDTO = new SortDTO() { SortBy = TableColumnConst.CODE_COL, Sort = "desc" };
            }

            //sortDTO = sortDTO == null ? new SortDTO() { SortBy = TableColumnConst.CODE_COL, Sort = "desc" } : sortDTO;
            var sortQuery = sortDTO == null ? "" :
                "ORDER BY " + aliasRootTbl + "." + PostgresConst.QUOTE + sortDTO.SortBy + PostgresConst.QUOTE + " " + sortDTO.Sort;

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
        /// <returns></returns>
        public static string BuildRawQueryCount(string rootTbl, List<SearchDTO> SearchFieldList)
        {
            var aliasRootTbl = "r";
            string rawQuery = "SELECT Count(*) FROM " + PostgresConst.QUOTE + rootTbl + PostgresConst.QUOTE + " AS " + aliasRootTbl;
            var selectCols = "";

            var whereCondition = BuildRawQueryCondition(SearchFieldList, aliasRootTbl);

            rawQuery = string.Format(rawQuery, selectCols);
            rawQuery += string.IsNullOrEmpty(whereCondition) ? " " : " WHERE " + whereCondition;

            return rawQuery;
        }
    }
}
