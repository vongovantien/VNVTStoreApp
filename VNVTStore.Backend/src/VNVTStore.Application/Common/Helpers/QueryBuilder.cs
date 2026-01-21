using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using VNVTStore.Application.DTOs;
using VNVTStore.Application.Common.Helpers;

namespace VNVTStore.Application.Common.Helpers;

public static class QueryBuilder
{
    public static string BuildRawQueryCondition(List<SearchDTO> SearchFieldList, string tableName = null)
    {
        // Normalize field names (Capitalize first letter to match DB)
        if (SearchFieldList != null)
        {
            foreach (var item in SearchFieldList)
            {
                if (!string.IsNullOrEmpty(item.SearchField) && char.IsLower(item.SearchField[0]))
                {
                    item.SearchField = char.ToUpper(item.SearchField[0]) + item.SearchField.Substring(1);
                }
            }
        }

        string text = "";
        List<SearchDTO> list = Enumerable.ToList(Enumerable.Where(SearchFieldList, (SearchDTO x) => x.GroupID.HasValue));
        if (list.Count > 0)
        {
            IEnumerable<short?> enumerable = Enumerable.Select(Enumerable.GroupBy(list, (SearchDTO x) => x.GroupID), (IGrouping<short?, SearchDTO> x) => x.Key);
            int num = 0;
            foreach (short? group in enumerable)
            {
                string text2 = "";
                foreach (SearchDTO item in Enumerable.Where(list, (SearchDTO x) => x.GroupID == group))
                {
                    // Skip invalid search items
                    if (string.IsNullOrEmpty(item.SearchField)) continue;

                    if (!string.IsNullOrEmpty(text2))
                    {
                        text2 = ((!string.IsNullOrEmpty(item.CombineCondition)) ? (text2 + " " + item.CombineCondition + " ") : (text2 + " AND "));
                    }

                    bool flag = JsonUtilities.CheckJsonArray(item.SearchValue);
                    switch (item.SearchCondition)
                    {
                        case SearchCondition.IsNull:
                            text2 += (string.IsNullOrEmpty(tableName) ? (" \"" + item.SearchField + "\" IS NULL ") : ("\"" + tableName + "\".\"" + item.SearchField + "\" IS NULL "));
                            break;
                        case SearchCondition.In:
                            if (flag)
                            {
                                object[] array6 = ((JArray)item.SearchValue).ToObject<object[]>();
                                string text7 = "";
                                for (int m = 0; m < array6.Length; m++)
                                {
                                    text7 = text7 + "'" + array6[m]?.ToString() + "',";
                                }

                                text7 = text7.Substring(0, text7.Length - 1);
                                text2 += (string.IsNullOrEmpty(tableName) ? ("\"" + item.SearchField + "\" IN (" + text7 + ") ") : ("\"" + tableName + "\".\"" + item.SearchField + "\" IN (" + text7 + ") "));
                            }
                            else
                            {
                                text2 += (string.IsNullOrEmpty(tableName) ? ("\"" + item.SearchField + "\" IN ('" + item.SearchValue + "') ") : ("\"" + tableName + "\".\"" + item.SearchField + "\" IN ('" + item.SearchValue + "') "));
                            }

                            break;
                        case SearchCondition.NotIn:
                            if (flag)
                            {
                                object[] array5 = ((JArray)item.SearchValue).ToObject<object[]>();
                                string text6 = "";
                                for (int l = 0; l < array5.Length; l++)
                                {
                                    text6 = text6 + "'" + array5[l]?.ToString() + "',";
                                }

                                text6 = text6.Substring(0, text6.Length - 1);
                                text2 += (string.IsNullOrEmpty(tableName) ? ("\"" + item.SearchField + "\" NOT IN (" + text6 + ") ") : ("\"" + tableName + "\".\"" + item.SearchField + "\" NOT IN (" + text6 + ") "));
                            }
                            else
                            {
                                text2 += (string.IsNullOrEmpty(tableName) ? ("\"" + item.SearchField + "\" NOT IN ('" + item.SearchValue + "') ") : ("\"" + tableName + "\".\"" + item.SearchField + "\" NOT IN ('" + item.SearchValue + "') "));
                            }

                            break;
                        case SearchCondition.IsNotNull:
                            text2 = text2 + "\"" + item.SearchField + "\" IS NOT NULL ";
                            break;
                        case SearchCondition.Equal:
                            if (flag)
                            {
                                object[] array3 = ((JArray)item.SearchValue).ToObject<object[]>();
                                string text4 = "";
                                for (int j = 0; j < array3.Length; j++)
                                {
                                    string fullName = array3[j].GetType().FullName;
                                    if (!(fullName == "System.String"))
                                    {
                                        if (fullName == "System.Boolean")
                                        {
                                            text4 = ((j <= 0) ? ((!string.IsNullOrEmpty(tableName)) ? (text4 + "\"" + tableName + "\".\"" + item.SearchField + "\" = " + array3[j]) : (text4 + "\"" + item.SearchField + "\" = " + array3[j])) : (text4 + ((tableName == null) ? (" OR \"" + item.SearchField + "\" = " + array3[j]?.ToString() + " ") : (" OR \"" + tableName + "\".\"" + item.SearchField + "\" = " + array3[j]?.ToString() + " "))));
                                        }
                                    }
                                    else
                                    {
                                        text4 = ((j <= 0) ? ((!string.IsNullOrEmpty(tableName)) ? (text4 + "\"" + tableName + "\".\"" + item.SearchField + "\" ILIKE  N'" + array3[j]?.ToString() + "'") : (text4 + "\"" + item.SearchField + "\" ILIKE N'" + array3[j]?.ToString() + "'")) : (text4 + ((tableName == null) ? (" OR \"" + item.SearchField + "\" ILIKE N'" + array3[j]?.ToString() + "' ") : (" OR \"" + tableName + "\".\"" + item.SearchField + "\" ILIKE N'" + array3[j]?.ToString() + "' "))));
                                    }
                                }

                                text4 = "( " + text4 + " )";
                                text2 += text4;
                            }
                            else
                            {
                                dynamic val = item.SearchValue.GetType().FullName;
                                text2 = ((!((val == "System.DateTime") ? true : false)) ? ((!((val == "System.String") ? true : false)) ? ((!string.IsNullOrEmpty(tableName)) ? ((string)(text2 + ("\"" + tableName + "\".\"" + item.SearchField + "\" = '" + item.SearchValue + "'"))) : ((string)(text2 + ("\"" + item.SearchField + "\" = '" + item.SearchValue + "'")))) : ((!string.IsNullOrEmpty(tableName)) ? ((string)(text2 + ("\"" + tableName + "\".\"" + item.SearchField + "\" ILIKE '" + item.SearchValue + "'"))) : ((string)(text2 + ("\"" + item.SearchField + "\" ILIKE '" + item.SearchValue + "'"))))) : ((!string.IsNullOrEmpty(tableName)) ? (text2 + "\"" + tableName + "\".\"" + item.SearchField + "\" = " + string.Format("to_timestamp('{0}', '{1}')", (object)item.SearchValue.ToString(), "dd/MM/yyyy HH24:MI:SS")) : (text2 + "\"" + item.SearchField + "\" = " + string.Format("to_timestamp('{0}', '{1}')", (object)item.SearchValue.ToString(), "dd/MM/yyyy HH24:MI:SS"))));
                            }

                            break;
                        case SearchCondition.EqualExact:
                            {
                                dynamic val5 = item.SearchValue.GetType().FullName;
                                text2 = ((!((val5 == "System.DateTime") ? true : false)) ? ((!((val5 == "System.String") ? true : false)) ? ((!string.IsNullOrEmpty(tableName)) ? ((string)(text2 + ("\"" + tableName + "\".\"" + item.SearchField + "\" = '" + item.SearchValue + "'"))) : ((string)(text2 + ("\"" + item.SearchField + "\" = '" + item.SearchValue + "'")))) : ((!string.IsNullOrEmpty(tableName)) ? ((string)(text2 + ("\"" + tableName + "\".\"" + item.SearchField + "\"='" + item.SearchValue + "'"))) : ((string)(text2 + ("\"" + item.SearchField + "\" = '" + item.SearchValue + "'"))))) : ((!string.IsNullOrEmpty(tableName)) ? (text2 + "\"" + tableName + "\".\"" + item.SearchField + "\" = " + string.Format("to_timestamp('{0}', '{1}')", (object)item.SearchValue.ToString(), "dd/MM/yyyy HH24:MI:SS")) : (text2 + "\"" + item.SearchField + "\" = " + string.Format("to_timestamp('{0}', '{1}')", (object)item.SearchValue.ToString(), "dd/MM/yyyy HH24:MI:SS"))));
                                break;
                            }
                        case SearchCondition.NotEqual:
                            if (flag)
                            {
                                object[] array4 = ((JArray)item.SearchValue).ToObject<object[]>();
                                string text5 = "";
                                for (int k = 0; k < array4.Length; k++)
                                {
                                    if (array4[k].GetType().FullName == "System.String")
                                    {
                                        text5 = ((k <= 0) ? ((!string.IsNullOrEmpty(tableName)) ? (text5 + "\"" + tableName + "\".\"" + item.SearchField + "\" != N'" + array4[k]?.ToString() + "'") : (text5 + "\"" + item.SearchField + "\" != N'" + array4[k]?.ToString() + "'")) : ((!string.IsNullOrEmpty(tableName)) ? (text5 + " OR \"" + tableName + "\".\"" + item.SearchField + "\" != N'" + array4[k]?.ToString() + "' ") : (text5 + " OR \"" + item.SearchField + "\" != N'" + array4[k]?.ToString() + "' ")));
                                    }
                                }

                                text5 = "( " + text5 + " )";
                                text2 += text5;
                            }
                            else
                            {
                                dynamic val2 = item.SearchValue.GetType().FullName;
                                text2 = ((!((val2 == "System.DateTime") ? true : false)) ? ((!string.IsNullOrEmpty(tableName)) ? ((string)(text2 + ("\"" + tableName + "\".\"" + item.SearchField + "\" != '" + item.SearchValue + "'"))) : ((string)(text2 + ("\"" + item.SearchField + "\" != '" + item.SearchValue + "'")))) : ((!string.IsNullOrEmpty(tableName)) ? (text2 + "\"" + tableName + "\".\"" + item.SearchField + "\" != " + string.Format("to_timestamp('{0}', '{1}')", (object)item.SearchValue.ToString(), "dd/MM/yyyy HH24:MI:SS")) : (text2 + "\"" + item.SearchField + "\" != " + string.Format("to_timestamp('{0}', '{1}')", (object)item.SearchValue.ToString(), "dd/MM/yyyy HH24:MI:SS"))));
                            }

                            break;
                        case SearchCondition.Contains:
                            if (flag)
                            {
                                object[] array2 = ((JArray)item.SearchValue).ToObject<object[]>();
                                string text3 = "";
                                for (int i = 0; i < array2.Length; i++)
                                {
                                    text3 = ((i <= 0) ? ((!string.IsNullOrEmpty(tableName)) ? (text3 + "\"" + tableName + "\".\"" + item.SearchField + "\"::text ILIKE N'%" + array2[i]?.ToString() + "%'") : (text3 + "\"" + item.SearchField + "\"::text ILIKE N'%" + array2[i]?.ToString() + "%'")) : ((!string.IsNullOrEmpty(tableName)) ? (text3 + " OR \"" + tableName + "\".\"" + item.SearchField + "\"::text ILIKE N'%" + array2[i]?.ToString() + "%' ") : (text3 + " OR \"" + item.SearchField + "\"::text ILIKE N'%" + array2[i]?.ToString() + "%' ")));
                                }

                                text3 = "( " + text3 + " )";
                                text2 += text3;
                            }
                            else
                            {
                                text2 = ((!string.IsNullOrEmpty(tableName)) ? ((string)(text2 + ("\"" + tableName + "\".\"" + item.SearchField + "\"::text ILIKE N'%" + item.SearchValue + "%'"))) : ((string)(text2 + ("\"" + item.SearchField + "\"::text ILIKE N'%" + item.SearchValue + "%'"))));
                            }

                            break;
                        case SearchCondition.GreaterThan:
                            {
                                dynamic val3 = item.SearchValue.GetType().FullName;
                                text2 = ((!((val3 == "System.DateTime") ? true : false)) ? ((!string.IsNullOrEmpty(tableName)) ? ((string)(text2 + ("\"" + tableName + "\".\"" + item.SearchField + "\" > " + item.SearchValue))) : ((string)(text2 + ("\"" + item.SearchField + "\" > " + item.SearchValue)))) : ((!string.IsNullOrEmpty(tableName)) ? (text2 + "\"" + tableName + "\".\"" + item.SearchField + "\" > " + string.Format("to_timestamp('{0}', '{1}')", (object)item.SearchValue.ToString(), "dd/MM/yyyy HH24:MI:SS")) : (text2 + "\"" + item.SearchField + "\" > " + string.Format("to_timestamp('{0}', '{1}')", (object)item.SearchValue.ToString(), "dd/MM/yyyy HH24:MI:SS"))));
                                break;
                            }
                        case SearchCondition.GreaterThanEqual:
                            {
                                dynamic val4 = item.SearchValue.GetType().FullName;
                                text2 = ((!((val4 == "System.DateTime") ? true : false)) ? ((!string.IsNullOrEmpty(tableName)) ? ((string)(text2 + ("\"" + tableName + "\".\"" + item.SearchField + "\" >= " + item.SearchValue))) : ((string)(text2 + ("\"" + item.SearchField + "\" >= " + item.SearchValue)))) : ((!string.IsNullOrEmpty(tableName)) ? (text2 + "\"" + tableName + "\".\"" + item.SearchField + "\" >= " + string.Format("to_timestamp('{0}', '{1}')", (object)item.SearchValue.ToString(), "dd/MM/yyyy HH24:MI:SS")) : (text2 + "\"" + item.SearchField + "\" >= " + string.Format("to_timestamp('{0}', '{1}')", (object)item.SearchValue.ToString(), "dd/MM/yyyy HH24:MI:SS"))));
                                break;
                            }
                        case SearchCondition.LessThan:
                            {
                                dynamic val6 = item.SearchValue.GetType().FullName;
                                text2 = ((!((val6 == "System.DateTime") ? true : false)) ? ((!string.IsNullOrEmpty(tableName)) ? ((string)(text2 + ("\"" + tableName + "\".\"" + item.SearchField + "\" < " + item.SearchValue))) : ((string)(text2 + ("\"" + item.SearchField + "\" < " + item.SearchValue)))) : ((!string.IsNullOrEmpty(tableName)) ? (text2 + "\"" + tableName + "\".\"" + item.SearchField + "\" < " + string.Format("to_timestamp('{0}', '{1}')", (object)item.SearchValue.ToString(), "dd/MM/yyyy HH24:MI:SS")) : (text2 + "\"" + item.SearchField + "\" < " + string.Format("to_timestamp('{0}', '{1}')", (object)item.SearchValue.ToString(), "dd/MM/yyyy HH24:MI:SS"))));
                                break;
                            }
                        case SearchCondition.LessThanEqual:
                            {
                                dynamic val7 = item.SearchValue.GetType().FullName;
                                text2 = ((!((val7 == "System.DateTime") ? true : false)) ? ((!string.IsNullOrEmpty(tableName)) ? ((string)(text2 + ("\"" + tableName + "\".\"" + item.SearchField + "\" <= " + item.SearchValue))) : ((string)(text2 + ("\"" + item.SearchField + "\" <= " + item.SearchValue)))) : ((!string.IsNullOrEmpty(tableName)) ? (text2 + "\"" + tableName + "\".\"" + item.SearchField + "\" <= " + string.Format("to_timestamp('{0}', '{1}')", (object)item.SearchValue.ToString(), "dd/MM/yyyy HH24:MI:SS")) : (text2 + "\"" + item.SearchField + "\" <= " + string.Format("to_timestamp('{0}', '{1}')", (object)item.SearchValue.ToString(), "dd/MM/yyyy HH24:MI:SS"))));
                                break;
                            }
                        case SearchCondition.DateTimeRange:
                            {
                                object[] array = ((JArray)item.SearchValue).ToObject<object[]>();
                                DateTime dateTime = DateTime.Parse(array[0].ToString());
                                if (array.Length <= 1)
                                {
                                    text2 = ((!string.IsNullOrEmpty(tableName)) ? (text2 + "\"" + tableName + "\".\"" + item.SearchField + "\" >= to_timestamp('" + dateTime.ToString("dd/MM/yyyy HH:mm:ss") + "', 'dd/MM/yyyy HH24:MI:SS')") : (text2 + "\"" + item.SearchField + "\" >= to_timestamp('" + dateTime.ToString("dd/MM/yyyy HH:mm:ss") + "', 'dd/MM/yyyy HH24:MI:SS')"));
                                }
                                else if (array[1] != null)
                                {
                                    DateTime dateTime2 = DateTime.Parse(array[1].ToString());
                                    text2 = ((!string.IsNullOrEmpty(tableName)) ? (text2 + "( \"" + tableName + "\".\"" + item.SearchField + "\" >= to_timestamp('" + dateTime.ToString("dd/MM/yyyy HH:mm:ss") + "', 'dd/MM/yyyy HH24:MI:SS') AND \"" + tableName + "\".\"" + item.SearchField + "\" <= to_timestamp('" + dateTime2.ToString("dd/MM/yyyy HH:mm:ss") + "', 'dd/MM/yyyy HH24:MI:SS'))") : (text2 + "( \"" + item.SearchField + "\" >= to_timestamp('" + dateTime.ToString("dd/MM/yyyy HH:mm:ss") + "', 'dd/MM/yyyy HH24:MI:SS') AND \"" + item.SearchField + "\" <= to_timestamp('" + dateTime2.ToString("dd/MM/yyyy HH:mm:ss") + "', 'dd/MM/yyyy HH24:MI:SS'))"));
                                }
                                else
                                {
                                    text2 = ((!string.IsNullOrEmpty(tableName)) ? (text2 + "\"" + tableName + "\".\"" + item.SearchField + "\" >= to_timestamp('" + dateTime.ToString("dd/MM/yyyy HH:mm:ss") + "', 'dd/MM/yyyy HH24:MI:SS')") : (text2 + "\"" + item.SearchField + "\" >= to_timestamp('" + dateTime.ToString("dd/MM/yyyy HH:mm:ss") + "', 'dd/MM/yyyy HH24:MI:SS')"));
                                }

                                break;
                            }
                    }
                }

                text2 = "( " + text2 + " )";
                text = ((num <= 0) ? (text + text2) : (text + " OR " + text2));
                num++;
            }
        }

        double num2 = default(double);
        double num3 = default(double);
        double num7 = default(double);
        foreach (SearchDTO SearchField in SearchFieldList)
        {
            if (SearchField.GroupID.HasValue)
            {
                continue;
            }

            if (!string.IsNullOrEmpty(text))
            {
                text = ((!string.IsNullOrEmpty(SearchField.CombineCondition)) ? (text + " " + SearchField.CombineCondition + " ") : (text + " AND "));
            }

            bool flag2 = JsonUtilities.CheckJsonArray(SearchField.SearchValue);
            switch (SearchField.SearchCondition)
            {
                case SearchCondition.IsNull:
                    text += (string.IsNullOrEmpty(tableName) ? (" \"" + SearchField.SearchField + "\" IS NULL ") : ("\"" + tableName + "\".\"" + SearchField.SearchField + "\" IS NULL "));
                    break;
                case SearchCondition.In:
                    if (flag2)
                    {
                        object[] array8 = ((JArray)SearchField.SearchValue).ToObject<object[]>();
                        string text9 = "";
                        for (int num4 = 0; num4 < array8.Length; num4++)
                        {
                            text9 = text9 + "'" + array8[num4]?.ToString() + "',";
                        }

                        text9 = text9.Substring(0, text9.Length - 1);
                        text += (string.IsNullOrEmpty(tableName) ? (" \"" + SearchField.SearchField + "\" IN (" + text9 + ") ") : ("\"" + tableName + "\".\"" + SearchField.SearchField + "\" IN (" + text9 + ") "));
                    }
                    else
                    {
                        text += (string.IsNullOrEmpty(tableName) ? (" \"" + SearchField.SearchField + "\" IN ('" + SearchField.SearchValue + "') ") : ("\"" + tableName + "\".\"" + SearchField.SearchField + "\" IN ('" + SearchField.SearchValue + "') "));
                    }

                    break;
                case SearchCondition.NotIn:
                    if (flag2)
                    {
                        object[] array12 = ((JArray)SearchField.SearchValue).ToObject<object[]>();
                        string text12 = "";
                        for (int num8 = 0; num8 < array12.Length; num8++)
                        {
                            text12 = text12 + "'" + array12[num8]?.ToString() + "',";
                        }

                        text12 = text12.Substring(0, text12.Length - 1);
                        text += (string.IsNullOrEmpty(tableName) ? (" \"" + SearchField.SearchField + "\" NOT IN (" + text12 + ") ") : ("\"" + tableName + "\".\"" + SearchField.SearchField + "\" NOT IN (" + text12 + ") "));
                    }
                    else
                    {
                        text += (string.IsNullOrEmpty(tableName) ? (" \"" + SearchField.SearchField + "\" NOT IN ('" + SearchField.SearchValue + "') ") : ("\"" + tableName + "\".\"" + SearchField.SearchField + "\" NOT IN ('" + SearchField.SearchValue + "') "));
                    }

                    break;
                case SearchCondition.IsNotNull:
                    text += (string.IsNullOrEmpty(tableName) ? (" \"" + SearchField.SearchField + "\" IS NOT NULL ") : ("\"" + tableName + "\".\"" + SearchField.SearchField + "\" IS NOT NULL "));
                    break;
                case SearchCondition.Equal:
                    {
                        if (flag2)
                        {
                            object[] array7 = ((JArray)SearchField.SearchValue).ToObject<object[]>();
                            string text8 = "";
                            for (int n = 0; n < array7.Length; n++)
                            {
                                text8 = ((!(array7[n].GetType().FullName == "System.String")) ? ((n <= 0) ? ((!string.IsNullOrEmpty(tableName)) ? (text8 + "\"" + tableName + "\".\"" + SearchField.SearchField + "\" = " + array7[n]) : (text8 + "\"" + SearchField.SearchField + "\" = " + array7[n])) : (text8 + ((tableName == null) ? (" OR \"" + SearchField.SearchField + "\" = " + array7[n]) : (" OR \"" + tableName + "\".\"" + SearchField.SearchField + "\" = " + array7[n])))) : ((n <= 0) ? ((!string.IsNullOrEmpty(tableName)) ? (text8 + "\"" + tableName + "\".\"" + SearchField.SearchField + "\" ILIKE N'" + array7[n]?.ToString() + "'") : (text8 + "\"" + SearchField.SearchField + "\" ILIKE N'" + array7[n]?.ToString() + "'")) : (text8 + ((tableName == null) ? (" OR \"" + SearchField.SearchField + "\" ILIKE N'" + array7[n]?.ToString() + "' ") : (" OR \"" + tableName + "\".\"" + SearchField.SearchField + "\" ILIKE N'" + array7[n]?.ToString() + "' ")))));
                            }

                            text8 = "( " + text8 + " )";
                            text += text8;
                            break;
                        }

                        dynamic val9 = ((SearchField.SearchValue == null) ? "" : SearchField.SearchValue.GetType().FullName);
                        if (val9 == "System.DateTime")
                        {
                            var dtValue = Convert.ToDateTime(SearchField.SearchValue);
                            text = ((!string.IsNullOrEmpty(tableName)) ? (text + "\"" + tableName + "\".\"" + SearchField.SearchField + "\" = " + string.Format("to_timestamp('{0}', '{1}')", dtValue.ToString("dd/MM/yyyy HH:mm:ss"), "dd/MM/yyyy HH24:MI:SS")) : (text + "\"" + SearchField.SearchField + "\" = " + string.Format("to_timestamp('{0}', '{1}')", dtValue.ToString("dd/MM/yyyy HH:mm:ss"), "dd/MM/yyyy HH24:MI:SS")));
                        }
                        else if (val9 == "System.Boolean")
                        {
                            text = ((!string.IsNullOrEmpty(tableName)) ? (((object)SearchField.SearchValue != null) ? ((string)(text + ("\"" + tableName + "\".\"" + SearchField.SearchField + "\" = " + SearchField.SearchValue))) : (text + "\"" + tableName + "\".\"" + SearchField.SearchField + "\" IS NULL ")) : ((string)(text + ("\"" + SearchField.SearchField + "\" = " + SearchField.SearchValue))));
                        }
                        else if (val9 == "System.String")
                        {
                            text = ((!string.IsNullOrEmpty(tableName)) ? (((object)SearchField.SearchValue != null) ? ((string)(text + ("\"" + tableName + "\".\"" + SearchField.SearchField + "\" ILIKE N'" + SearchField.SearchValue + "'"))) : (text + "\"" + tableName + "\".\"" + SearchField.SearchField + "\" IS NULL ")) : ((string)(text + ("\"" + SearchField.SearchField + "\" ILIKE N'" + SearchField.SearchValue + "'"))));
                        }
                        else if (string.IsNullOrEmpty(tableName))
                        {
                            text += "\"" + SearchField.SearchField + "\" = N'" + SearchField.SearchValue + "'";
                        }
                        else if ((object)SearchField.SearchValue == null)
                        {
                            text = text + "\"" + tableName + "\".\"" + SearchField.SearchField + "\" IS NULL ";
                        }
                        else
                        {
                            dynamic val10 = double.TryParse(SearchField.SearchValue.ToString(), out num2);
                            text = ((!(val10 ? true : false)) ? ((string)(text + ("\"" + tableName + "\".\"" + SearchField.SearchField + "\" = N'" + SearchField.SearchValue + "'"))) : ((string)(text + ("\"" + tableName + "\".\"" + SearchField.SearchField + "\" = " + SearchField.SearchValue))));
                        }

                        break;
                    }
                case SearchCondition.EqualExact:
                    {
                        dynamic val11 = ((SearchField.SearchValue == null) ? "" : SearchField.SearchValue.GetType().FullName);
                        if (val11 == "System.DateTime")
                        {
                            var dtValue = Convert.ToDateTime(SearchField.SearchValue);
                            text = ((!string.IsNullOrEmpty(tableName)) ? (text + "\"" + tableName + "\".\"" + SearchField.SearchField + "\" = " + string.Format("to_timestamp('{0}', '{1}')", dtValue.ToString("dd/MM/yyyy HH:mm:ss"), "dd/MM/yyyy HH24:MI:SS")) : (text + "\"" + SearchField.SearchField + "\" = " + string.Format("to_timestamp('{0}', '{1}')", dtValue.ToString("dd/MM/yyyy HH:mm:ss"), "dd/MM/yyyy HH24:MI:SS")));
                        }
                        else if (val11 == "System.Boolean")
                        {
                            text = ((!string.IsNullOrEmpty(tableName)) ? (((object)SearchField.SearchValue != null) ? ((string)(text + ("\"" + tableName + "\".\"" + SearchField.SearchField + "\" = " + SearchField.SearchValue))) : (text + "\"" + tableName + "\".\"" + SearchField.SearchField + "\" IS NULL ")) : ((string)(text + ("\"" + SearchField.SearchField + "\" = " + SearchField.SearchValue))));
                        }
                        else if (val11 == "System.String")
                        {
                            text = ((!string.IsNullOrEmpty(tableName)) ? (((object)SearchField.SearchValue != null) ? ((string)(text + ("\"" + tableName + "\".\"" + SearchField.SearchField + "\"='" + SearchField.SearchValue + "'"))) : (text + "\"" + tableName + "\".\"" + SearchField.SearchField + "\" IS NULL ")) : ((string)(text + ("\"" + SearchField.SearchField + "\"='" + SearchField.SearchValue + "'"))));
                        }
                        else if (string.IsNullOrEmpty(tableName))
                        {
                            text += "\"" + SearchField.SearchField + "\" = N'" + SearchField.SearchValue + "'";
                        }
                        else if ((object)SearchField.SearchValue == null)
                        {
                            text = text + "\"" + tableName + "\".\"" + SearchField.SearchField + "\" IS NULL ";
                        }
                        else
                        {
                            dynamic val12 = double.TryParse(SearchField.SearchValue.ToString(), out num3);
                            text = ((!(val12 ? true : false)) ? ((string)(text + ("\"" + tableName + "\".\"" + SearchField.SearchField + "\" = N'" + SearchField.SearchValue + "'"))) : ((string)(text + ("\"" + tableName + "\".\"" + SearchField.SearchField + "\" = " + SearchField.SearchValue))));
                        }

                        break;
                    }
                case SearchCondition.NotEqual:
                    {
                        if (flag2)
                        {
                            object[] array11 = ((JArray)SearchField.SearchValue).ToObject<object[]>();
                            string text11 = "";
                            for (int num6 = 0; num6 < array11.Length; num6++)
                            {
                                if (array11[num6].GetType().FullName == "System.String")
                                {
                                    text11 = ((num6 <= 0) ? ((!string.IsNullOrEmpty(tableName)) ? (text11 + "\"" + tableName + "\".\"" + SearchField.SearchField + "\" != N'" + array11[num6]?.ToString() + "'") : (text11 + "\"" + SearchField.SearchField + "\" != N'" + array11[num6]?.ToString() + "'")) : ((!string.IsNullOrEmpty(tableName)) ? (text11 + " OR \"" + tableName + "\".\"" + SearchField.SearchField + "\" != N'" + array11[num6]?.ToString() + "' ") : (text11 + " OR \"" + SearchField.SearchField + "\" != N'" + array11[num6]?.ToString() + "' ")));
                                }
                            }

                            text11 = "( " + text11 + " )";
                            text += text11;
                            break;
                        }

                        dynamic val14 = SearchField.SearchValue.GetType().FullName;
                        if (val14 == "System.DateTime")
                        {
                            var dtValue = Convert.ToDateTime(SearchField.SearchValue);
                            text = ((!string.IsNullOrEmpty(tableName)) ? (text + "\"" + tableName + "\".\"" + SearchField.SearchField + "\" != " + string.Format("to_timestamp('{0}', '{1}')", dtValue.ToString("dd/MM/yyyy HH:mm:ss"), "dd/MM/yyyy HH24:MI:SS")) : (text + "\"" + SearchField.SearchField + "\" != " + string.Format("to_timestamp('{0}', '{1}')", dtValue.ToString("dd/MM/yyyy HH:mm:ss"), "dd/MM/yyyy HH24:MI:SS")));
                        }
                        else if (val14 == "System.Boolean")
                        {
                            text = ((!string.IsNullOrEmpty(tableName)) ? (((object)SearchField.SearchValue != null) ? ((string)(text + ("\"" + tableName + "\".\"" + SearchField.SearchField + "\" != " + SearchField.SearchValue))) : (text + "\"" + tableName + "\".\"" + SearchField.SearchField + "\" IS NULL ")) : ((string)(text + ("\"" + SearchField.SearchField + "\" != " + SearchField.SearchValue))));
                        }
                        else if (val14 == "System.String")
                        {
                            text = ((!string.IsNullOrEmpty(tableName)) ? (((object)SearchField.SearchValue != null) ? ((string)(text + ("\"" + tableName + "\".\"" + SearchField.SearchField + "\" != '" + SearchField.SearchValue + "'"))) : (text + "\"" + tableName + "\".\"" + SearchField.SearchField + "\" IS NULL ")) : ((string)(text + ("\"" + SearchField.SearchField + "\" != '" + SearchField.SearchValue + "'"))));
                        }
                        else if (string.IsNullOrEmpty(tableName))
                        {
                            text += "\"" + SearchField.SearchField + "\" != N'" + SearchField.SearchValue + "'";
                        }
                        else
                        {
                            dynamic val15 = double.TryParse(SearchField.SearchValue.ToString(), out num7);
                            text = ((!(val15 ? true : false)) ? ((string)(text + ("\"" + tableName + "\".\"" + SearchField.SearchField + "\" != N'" + SearchField.SearchValue + "'"))) : ((string)(text + ("\"" + tableName + "\".\"" + SearchField.SearchField + "\" = " + SearchField.SearchValue))));
                        }

                        break;
                    }
                case SearchCondition.Contains:
                    if (flag2)
                    {
                        object[] array10 = ((JArray)SearchField.SearchValue).ToObject<object[]>();
                        string text10 = "";
                        for (int num5 = 0; num5 < array10.Length; num5++)
                        {
                            text10 = ((num5 <= 0) ? ((!string.IsNullOrEmpty(tableName)) ? (text10 + "\"" + tableName + "\".\"" + SearchField.SearchField + "\"::text ILIKE N'%" + array10[num5]?.ToString() + "%'") : (text10 + "\"" + SearchField.SearchField + "\"::text ILIKE N'%" + array10[num5]?.ToString() + "%'")) : ((!string.IsNullOrEmpty(tableName)) ? (text10 + " OR \"" + tableName + "\".\"" + SearchField.SearchField + "\"::text ILIKE N'%" + array10[num5]?.ToString() + "%' ") : (text10 + " OR \"" + SearchField.SearchField + "\"::text ILIKE N'%" + array10[num5]?.ToString() + "%' ")));
                        }

                        text10 = "( " + text10 + " )";
                        text += text10;
                    }
                    else
                    {
                        text = ((!string.IsNullOrEmpty(tableName)) ? ((string)(text + ("\"" + tableName + "\".\"" + SearchField.SearchField + "\"::text ILIKE N'%" + SearchField.SearchValue + "%'"))) : ((string)(text + ("\"" + SearchField.SearchField + "\"::text ILIKE N'%" + SearchField.SearchValue + "%'"))));
                    }

                    break;
                case SearchCondition.GreaterThan:
                    {
                        dynamic val16 = SearchField.SearchValue.GetType().FullName;
                        text = ((!((val16 == "System.DateTime") ? true : false)) ? ((!string.IsNullOrEmpty(tableName)) ? ((string)(text + ("\"" + tableName + "\".\"" + SearchField.SearchField + "\" > " + SearchField.SearchValue))) : ((string)(text + ("\"" + SearchField.SearchField + "\" > " + SearchField.SearchValue)))) : ((!string.IsNullOrEmpty(tableName)) ? (text + "\"" + tableName + "\".\"" + SearchField.SearchField + "\" > " + string.Format("to_timestamp('{0}', '{1}')", (object)SearchField.SearchValue.ToString(), "dd/MM/yyyy HH24:MI:SS")) : (text + "\"" + SearchField.SearchField + "\" > " + string.Format("to_timestamp('{0}', '{1}')", (object)SearchField.SearchValue.ToString(), "dd/MM/yyyy HH24:MI:SS"))));
                        break;
                    }
                case SearchCondition.GreaterThanEqual:
                    {
                        dynamic val17 = SearchField.SearchValue.GetType().FullName;
                        text = ((!((val17 == "System.DateTime") ? true : false)) ? ((!string.IsNullOrEmpty(tableName)) ? ((string)(text + ("\"" + tableName + "\".\"" + SearchField.SearchField + "\" >= " + SearchField.SearchValue))) : ((string)(text + ("\"" + SearchField.SearchField + "\" >= " + SearchField.SearchValue)))) : ((!string.IsNullOrEmpty(tableName)) ? (text + "\"" + tableName + "\".\"" + SearchField.SearchField + "\" >= " + string.Format("to_timestamp('{0}', '{1}')", (object)SearchField.SearchValue.ToString(), "dd/MM/yyyy HH24:MI:SS")) : (text + "\"" + SearchField.SearchField + "\" >= " + string.Format("to_timestamp('{0}', '{1}')", (object)SearchField.SearchValue.ToString(), "dd/MM/yyyy HH24:MI:SS"))));
                        break;
                    }
                case SearchCondition.LessThan:
                    {
                        dynamic val8 = SearchField.SearchValue.GetType().FullName;
                        text = ((!((val8 == "System.DateTime") ? true : false)) ? ((!string.IsNullOrEmpty(tableName)) ? ((string)(text + ("\"" + tableName + "\".\"" + SearchField.SearchField + "\" < " + SearchField.SearchValue))) : ((string)(text + ("\"" + SearchField.SearchField + "\" < " + SearchField.SearchValue)))) : ((!string.IsNullOrEmpty(tableName)) ? (text + "\"" + tableName + "\".\"" + SearchField.SearchField + "\" < " + string.Format("to_timestamp('{0}', '{1}')", (object)SearchField.SearchValue.ToString(), "dd/MM/yyyy HH24:MI:SS")) : (text + "\"" + SearchField.SearchField + "\" < " + string.Format("to_timestamp('{0}', '{1}')", (object)SearchField.SearchValue.ToString(), "dd/MM/yyyy HH24:MI:SS"))));
                        break;
                    }
                case SearchCondition.LessThanEqual:
                    {
                        dynamic val13 = SearchField.SearchValue.GetType().FullName;
                        text = ((!((val13 == "System.DateTime") ? true : false)) ? ((!string.IsNullOrEmpty(tableName)) ? ((string)(text + ("\"" + tableName + "\".\"" + SearchField.SearchField + "\" <= " + SearchField.SearchValue))) : ((string)(text + ("\"" + SearchField.SearchField + "\" <= " + SearchField.SearchValue)))) : ((!string.IsNullOrEmpty(tableName)) ? (text + "\"" + tableName + "\".\"" + SearchField.SearchField + "\" <= " + string.Format("to_timestamp('{0}', '{1}')", (object)SearchField.SearchValue.ToString(), "dd/MM/yyyy HH24:MI:SS")) : (text + "\"" + SearchField.SearchField + "\" <= " + string.Format("to_timestamp('{0}', '{1}')", (object)SearchField.SearchValue.ToString(), "dd/MM/yyyy HH24:MI:SS"))));
                        break;
                    }
                case SearchCondition.DateTimeRange:
                    {
                        object[] array9 = ((JArray)SearchField.SearchValue).ToObject<object[]>();
                        DateTime dateTime3 = DateTime.Parse(array9[0].ToString());
                        if (array9.Length <= 1)
                        {
                            text = ((!string.IsNullOrEmpty(tableName)) ? (text + "\"" + tableName + "\".\"" + SearchField.SearchField + "\" >= to_timestamp('" + dateTime3.ToString("dd/MM/yyyy HH:mm:ss") + "', 'dd/MM/yyyy HH24:MI:SS')") : (text + "\"" + SearchField.SearchField + "\" >= to_timestamp('" + dateTime3.ToString("dd/MM/yyyy HH:mm:ss") + "', 'dd/MM/yyyy HH24:MI:SS')"));
                        }
                        else if (array9[1] != null)
                        {
                            DateTime dateTime4 = DateTime.Parse(array9[1].ToString());
                            text = ((!string.IsNullOrEmpty(tableName)) ? (text + "( \"" + tableName + "\".\"" + SearchField.SearchField + "\" >= to_timestamp('" + dateTime3.ToString("dd/MM/yyyy HH:mm:ss") + "', 'dd/MM/yyyy HH24:MI:SS') AND \"" + tableName + "\".\"" + SearchField.SearchField + "\" <= to_timestamp('" + dateTime4.ToString("dd/MM/yyyy HH:mm:ss") + "', 'dd/MM/yyyy HH24:MI:SS'))") : (text + "( \"" + SearchField.SearchField + "\" >= to_timestamp('" + dateTime3.ToString("dd/MM/yyyy HH:mm:ss") + "', 'dd/MM/yyyy HH24:MI:SS') AND \"" + SearchField.SearchField + "\" <= to_timestamp('" + dateTime4.ToString() + "', 'dd/MM/yyyy HH24:MI:SS'))"));
                        }
                        else
                        {
                            text = ((!string.IsNullOrEmpty(tableName)) ? (text + "\"" + tableName + "\".\"" + SearchField.SearchField + "\" >= to_timestamp('" + dateTime3.ToString("dd/MM/yyyy HH:mm:ss") + "', 'dd/MM/yyyy HH24:MI:SS')") : (text + "\"" + SearchField.SearchField + "\" >= to_timestamp('" + dateTime3.ToString("dd/MM/yyyy HH:mm:ss") + "', 'dd/MM/yyyy HH24:MI:SS')"));
                        }

                        break;
                    }
                case SearchCondition.DayPart:
                    text = ((!string.IsNullOrEmpty(tableName)) ? (text + " date_part('day',\"" + tableName + "\".\"" + string.Format("{0}{1})  = {2}", SearchField.SearchField, "\"", (object)SearchField.SearchValue)) : (text + string.Format(" date_part('day', {0}{1}{2})  = {3}", "\"", SearchField.SearchField, "\"", SearchField.SearchValue)));
                    break;
                case SearchCondition.MonthPart:
                    text = ((!string.IsNullOrEmpty(tableName)) ? (text + " date_part('month',\"" + tableName + "\".\"" + string.Format("{0}{1})  = {2}", SearchField.SearchField, "\"", (object)SearchField.SearchValue)) : (text + string.Format(" date_part('month', {0}{1}{2})  = {3}", "\"", SearchField.SearchField, "\"", SearchField.SearchValue)));
                    break;
                case SearchCondition.DatePart:
                    text = ((!string.IsNullOrEmpty(tableName)) ? (text + "\"" + tableName + "\".\"" + string.Format("{0}{1}::date  = to_date('{2}', '{3}')", SearchField.SearchField, "\"", SearchField.SearchValue, "dd/MM/yyyy")) : (text + string.Format(" {0}{1}{2}::date  = {3}", "\"", SearchField.SearchField, "\"", SearchField.SearchValue)));
                    break;
            }
        }

        return text;
    }

    public static string BuildRawQueryPaging(int pageSize, int pageIndex, string rootTbl, List<ReferenceTable> refTblList, List<SearchDTO> SearchFieldList, SortDTO sortDTO, List<string> fields = null)
    {
        string format = "WITH TempResult AS(\r\n                                {2}\r\n                            ), TempCount AS(\r\n                                SELECT COUNT(*) AS \"TotalRow\"FROM TempResult\r\n                            )\r\n                            SELECT * FROM TempResult, TempCount\r\n                            ORDER BY TempResult.{3}\r\n                                OFFSET({1} - 1) * {0}\r\n                                   FETCH NEXT {0} ROWS ONLY";
        string text = "r";
        string text2 = "";
        List<string> list = null;
        if (fields != null)
        {
            // Filter empty and normalize casing
            list = fields.Where(x => !string.IsNullOrEmpty(x) && !x.Contains("."))
                         .Select(x => char.IsLower(x[0]) ? char.ToUpper(x[0]) + x.Substring(1) : x)
                         .ToList();

            // Treat empty list as Select All
            if (!list.Any()) list = null;

            if (list != null)
            {
                foreach (string item in list)
                {
                    if (((refTblList == null) ? null : Enumerable.FirstOrDefault(refTblList, (ReferenceTable x) => x.AliasName == item)) == null)
                    {
                        text2 = text2 + text + ".\"" + item.ToString() + "\",";
                    }
                }
            }

            if (!Enumerable.Any(list, (string x) => x.ToString() == "CompanyCode"))
            {
                text2 = text2 + text + ".\"" + "CompanyCode".ToString() + "\",";
            }

            text2 = text2.Substring(0, text2.Length - 1);
        }

        string format2 = (string.IsNullOrEmpty(text2) ? ("SELECT " + text + ".* {0} FROM \"" + rootTbl + "\" AS " + text) : ("SELECT " + text2 + " {0} FROM \"" + rootTbl + "\" AS " + text));
        string text3 = "";
        string text4 = "";
        int num = 1;
        if (refTblList != null)
        {
            foreach (ReferenceTable refTbl in refTblList)
            {
                // Skip invalid reference tables to prevent SQL errors
                if (string.IsNullOrEmpty(refTbl.ColumnName) || 
                    string.IsNullOrEmpty(refTbl.AliasName) ||
                    string.IsNullOrEmpty(refTbl.TableName) ||
                    string.IsNullOrEmpty(refTbl.ForeignKeyCol))
                {
                    continue;
                }
                
                if (list == null || Enumerable.Any(list, (string x) => x.ToString() == refTbl.AliasName))
                {
                    string text5 = "t" + num;
                    string aliasName = refTbl.AliasName;
                    text4 = text4 + ", " + text5 + ".\"" + refTbl.ColumnName + "\" AS \"" + aliasName + "\"";
                    text3 = text3 + " LEFT JOIN \"" + refTbl.TableName + "\" AS " + text5 + " ON " + text + ".\"" + refTbl.ForeignKeyCol + "\"=" + text5 + ".\"Code\"";
                    if (refTbl.FilterType != "All" || string.IsNullOrEmpty(refTbl.FilterType))
                    {
                        text3 = text3 + "AND " + text + ".\"CompanyCode\" = " + text5 + ".\"CompanyCode\" ";
                    }

                    num++;
                }
            }
        }

        sortDTO = ((sortDTO == null) ? new SortDTO
        {
            SortBy = "Code",
            Sort = "desc"
        } : sortDTO);

        // Normalize SortBy casing
        if (!string.IsNullOrEmpty(sortDTO.SortBy) && char.IsLower(sortDTO.SortBy[0]))
        {
             sortDTO.SortBy = char.ToUpper(sortDTO.SortBy[0]) + sortDTO.SortBy.Substring(1);
        }

        string text6 = "\"" + sortDTO.SortBy + "\" " + sortDTO.Sort;
        string text7 = BuildRawQueryCondition(SearchFieldList, text);
        format2 = string.Format(format2, text4);
        format2 += text3;
        format2 += (string.IsNullOrEmpty(text7) ? " " : (" WHERE " + text7));
        
        var finalSql = string.Format(format, pageSize, pageIndex, format2, text6);
        Console.WriteLine($"[DEBUG-SQL-PAGING] {finalSql}");
        return finalSql;
    }
}
