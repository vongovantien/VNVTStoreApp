using FW.WAPI.Core.Domain.Context;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace FW.WAPI.Core.General
{
    public static class Utilities
    {
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="companyCode"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static string GenUniqueCode<T>(IBaseDBContext context, string companyCode = null, string prefix = null)
            where T : class

        {
            string newCode;

            try
            {
                int maxLen = 8;

                object item;
                var mapping = context.Model.FindEntityType(typeof(T).FullName).Relational();
                var schema = mapping.Schema;
                var tableName = mapping.TableName;

                if (prefix == null)
                {
                    prefix = context.GetUniqueCodePrefix(tableName, ref maxLen, companyCode);
                }

                if (companyCode == null)
                {
                    //var query = String.Format("SELECT * FROM {2} WHERE Code LIKE '{1}%'  and ModifiedType != '{0}'", "Delete", prefix + ".", tableName);
                    //TuanNT
                    var query = string.Format("SELECT * FROM {1} WHERE \"Code\" LIKE '%{0}%'", prefix + ".", tableName);
                    var result = context.Set<T>().FromSql(query);

                    item = EntityExpression.OrderByDescendingExpression(result, "Code").FirstOrDefault();
                }
                else
                {
                    //var query = String.Format("SELECT * FROM {3} WHERE Code LIKE '{2}%' and CompanyCode = '{0}' and ModifiedType != '{1}'",
                    //    companyCode, "Delete", prefix + ".", tableName);
                    //TuanNT
                    var query = string.Format("SELECT * FROM {2} WHERE \"Code\" LIKE '%{1}%' and \"CompanyCode\" = '{0}' ",
                       companyCode, prefix + ".", tableName);
                    IQueryable<T> results = context.Set<T>().FromSql(query);

                    item = EntityExpression.OrderByDescendingExpression(results, "Code").FirstOrDefault();
                }

                if (item == null)
                {
                    newCode = "0";
                }
                else
                {
                    string code = item.GetType().GetProperty("Code").GetValue(item, null).ToString();
                    try
                    {
                        int maxId;

                        int idx = code.IndexOf('.');

                        if (idx > -1)
                        {
                            maxId = int.Parse(code.Remove(0, idx + 1));
                        }
                        else
                        {
                            maxId = int.Parse(code);
                        }

                        newCode = (maxId + 1).ToString();
                    }
                    catch
                    {
                        newCode = "1".PadLeft(maxLen, '0');
                    }
                }

                if (newCode.Length < maxLen)
                {
                    newCode = newCode.PadLeft(maxLen, '0');
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return string.IsNullOrEmpty(prefix) ? newCode : string.Format("{0}.{1}", prefix, newCode);
        }

        public static string NewSecurityStamp()
        {
            byte[] bytes = new byte[20];
            _rng.GetBytes(bytes);
            return Base32.ToBase32(bytes);
        }

        public static string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }



        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TDataContext"></typeparam>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <param name="companyCode"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static string GenUniqueCode<TDataContext, T>(TDataContext context, string companyCode = null, string prefix = null)
            where TDataContext : DbContext
            where T : class
        {
            string newCode;

            try
            {
                int maxLen = 8;

                object item;
                var mapping = context.Model.FindEntityType(typeof(T).FullName).Relational();
                var schema = mapping.Schema;
                var tableName = mapping.TableName;

                if (prefix == null)
                {
                    if (context is IGenerateUniqueCode uniqueCode)
                    {
                        prefix = uniqueCode.GetUniqueCodePrefix(tableName, ref maxLen, companyCode);
                    }
                    else
                    {
                        //set default prefix if not implement inteface
                        prefix = "SMI";
                    }
                    //TODO: handle generate prefix code
                    //prefix = context.GetUniqueCodePrefix(tableName, ref maxLen, companyCode);
                }

                if (companyCode == null)
                {
                    //var query = String.Format("SELECT * FROM {2} WHERE Code LIKE '{1}%'  and ModifiedType != '{0}'", "Delete", prefix + ".", tableName);
                    //TuanNT
                    var query = string.Format("SELECT * FROM {1} WHERE \"Code\" LIKE '%{0}%'", prefix + ".", tableName);
                    var result = context.Set<T>().FromSql(query);

                    item = EntityExpression.OrderByDescendingExpression(result, "Code").FirstOrDefault();
                }
                else
                {
                    //var query = String.Format("SELECT * FROM {3} WHERE Code LIKE '{2}%' and CompanyCode = '{0}' and ModifiedType != '{1}'",
                    //    companyCode, "Delete", prefix + ".", tableName);
                    //TuanNT
                    var query = String.Format("SELECT * FROM {2} WHERE \"Code\" LIKE '%{1}%' and \"CompanyCode\" = '{0}' ",
                       companyCode, prefix + ".", tableName);
                    var results = context.Set<T>().FromSql(query);

                    item = EntityExpression.OrderByDescendingExpression(results, "Code").FirstOrDefault();
                }

                if (item == null)
                {
                    newCode = "0";
                }
                else
                {
                    string code = item.GetType().GetProperty("Code").GetValue(item, null).ToString();
                    try
                    {
                        int maxId;

                        int idx = code.IndexOf('.');

                        if (idx > -1)
                        {
                            maxId = int.Parse(code.Remove(0, idx + 1));
                        }
                        else
                        {
                            maxId = int.Parse(code);
                        }

                        newCode = (maxId + 1).ToString();
                    }
                    catch
                    {
                        newCode = "1".PadLeft(maxLen, '0');
                    }
                }

                if (newCode.Length < maxLen)
                {
                    newCode = newCode.PadLeft(maxLen, '0');
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return string.IsNullOrEmpty(prefix) ? newCode : string.Format("{0}.{1}", prefix, newCode);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dataContext"></param>
        /// <returns></returns>
        public static List<string> GetColumnNames<T>(DbContext dataContext)
            where T : class
        {
            try
            {
                return dataContext.Model.FindEntityType(typeof(T))
                 .GetProperties().Select(x => x.Relational().ColumnName).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        public static List<string> GetColumnNames(DbContext dataContext, Type type)
        {
            try
            {
                return dataContext.Model.FindEntityType(type)
                 .GetProperties().Select(x => x.Relational().ColumnName).ToList();
            }
            catch
            {
                return new List<string>();
            }
        }

        //public static async Task<string> GenUniqueCodeAsync<TDataContext, T>(TDataContext context, string companyCode = null, string prefix = null)
        //  where TDataContext : DbContext
        //  where T : class
        //{
        //    string newCode;

        //    try
        //    {
        //        int maxLen = 8;

        //        object item;
        //        var mapping = context.Model.FindEntityType(typeof(T).FullName).Relational();
        //        var schema = mapping.Schema;
        //        var tableName = mapping.TableName;

        //        if (prefix == null)
        //        {
        //            if (context is IGenerateUniqueCodeAsync uniqueCodeAsync)
        //            {
        //                var autoSettingValue = await uniqueCodeAsync.GetUniqueCodePrefixAsync(tableName, maxLen, companyCode);
        //                prefix = autoSettingValue.PrefixOfDefaultValueForCode;

        //                if (autoSettingValue.LengthOfDefaultValueForCode.HasValue)
        //                {
        //                    maxLen = autoSettingValue.LengthOfDefaultValueForCode.Value;
        //                }
        //            }
        //            else
        //            {
        //                //set default prefix if not implement inteface
        //                prefix = "SMI";
        //            }
        //            //TODO: handle generate prefix code
        //            //prefix = context.GetUniqueCodePrefix(tableName, ref maxLen, companyCode);
        //        }

        //        if (companyCode == null)
        //        {
        //            //var query = String.Format("SELECT * FROM {2} WHERE Code LIKE '{1}%'  and ModifiedType != '{0}'", "Delete", prefix + ".", tableName);
        //            //TuanNT
        //            var query = string.Format("SELECT * FROM {1} WHERE \"Code\" LIKE '%{0}%'", prefix + ".", tableName);
        //            var result = context.Set<T>().FromSql(query);

        //            item = EntityExpression.OrderByDescendingExpression(result, "Code").FirstOrDefault();
        //        }
        //        else
        //        {
        //            //var query = String.Format("SELECT * FROM {3} WHERE Code LIKE '{2}%' and CompanyCode = '{0}' and ModifiedType != '{1}'",
        //            //    companyCode, "Delete", prefix + ".", tableName);
        //            //TuanNT
        //            var query = String.Format("SELECT * FROM {2} WHERE \"Code\" LIKE '%{1}%' and \"CompanyCode\" = '{0}' ",
        //               companyCode, prefix + ".", tableName);
        //            var results = context.Set<T>().FromSql(query);

        //            item = EntityExpression.OrderByDescendingExpression(results, "Code").FirstOrDefault();
        //        }

        //        if (item == null)
        //        {
        //            newCode = "0";
        //        }
        //        else
        //        {
        //            string code = item.GetType().GetProperty("Code").GetValue(item, null).ToString();
        //            try
        //            {
        //                int maxId;

        //                int idx = code.IndexOf('.');

        //                if (idx > -1)
        //                {
        //                    maxId = int.Parse(code.Remove(0, idx + 1));
        //                }
        //                else
        //                {
        //                    maxId = int.Parse(code);
        //                }

        //                newCode = (maxId + 1).ToString();
        //            }
        //            catch
        //            {
        //                newCode = "1".PadLeft(maxLen, '0');
        //            }
        //        }

        //        if (newCode.Length < maxLen)
        //        {
        //            newCode = newCode.PadLeft(maxLen, '0');
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }

        //    return string.IsNullOrEmpty(prefix) ? newCode : string.Format("{0}.{1}", prefix, newCode);
        //}

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="originalObject"></param>
        /// <returns></returns>
        public static T ConvertObjectToObject<T>(object originalObject)
        {
            var jObject = JsonConvert.SerializeObject(originalObject);

            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;

            jsonSerializerSettings.Converters.Add(new FormatConverter());
            T toObject = JsonConvert.DeserializeObject<T>(jObject, jsonSerializerSettings);

            return toObject;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object ConvertList(List<object> value, Type type)
        {
            Type genericListType = typeof(List<>).MakeGenericType(type);
            var list = (IList)Activator.CreateInstance(genericListType);

            foreach (var item in value)
            {
                list.Add(item);
            }

            return list;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetTableName<T>(IBaseDBContext context)
        {
            var mapping = context.Model.FindEntityType(typeof(T).FullName).Relational();
            var tableName = mapping.TableName;

            return tableName;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="context"></param>
        /// <returns></returns>
        public static string GetTableName<TDatacontex, T>(TDatacontex context)
            where TDatacontex : DbContext
        {
            var mapping = context.Model.FindEntityType(typeof(T).FullName).Relational();
            var tableName = mapping.TableName;

            return tableName;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTableName<TDataContext>(TDataContext context, Type type)
            where TDataContext : DbContext
        {
            var mapping = context.Model.FindEntityType(type).Relational();
            var schema = mapping.Schema;
            var tableName = mapping.TableName;

            return tableName;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="context"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTableName(IBaseDBContext context, Type type)
        {
            var mapping = context.Model.FindEntityType(type).Relational();
            var tableName = mapping.TableName;

            return tableName;
        }

        /// <summary>
        /// Encrypt Data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string EncryptData(string data, string key = "")
        {
            try
            {
                key = "SMCVN" + key;

                byte[] PP = Encoding.Unicode.GetBytes(key);
                byte[] DataByte = Encoding.Unicode.GetBytes(data);
                HashAlgorithm HashPassword = (HashAlgorithm)CryptoConfig.CreateFromName("MD5");
                //HashAlgorithm.Create("MD5");
                byte[] V = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 2, 2, 0, 6, 8, 2 };
                RijndaelManaged EncryptData = new RijndaelManaged();
                EncryptData.Key = HashPassword.ComputeHash(PP);
                ICryptoTransform encryptor = EncryptData.CreateEncryptor(EncryptData.Key, V);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);
                cs.Write(DataByte, 0, DataByte.Length);
                cs.FlushFinalBlock();
                byte[] Result = ms.ToArray();
                ms.Close();
                cs.Close();
                EncryptData.Clear();
                var result = Convert.ToBase64String(Result);
                return result;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        /// Decrypt Data
        /// </summary>
        /// <param name="data"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string DecryptData(string data, string key = "")
        {
            try
            {
                key = "SMCVN" + key;

                byte[] PP = Encoding.Unicode.GetBytes(key);
                byte[] DataEncryptedByte = Convert.FromBase64String(data);
                HashAlgorithm HashPassword = (HashAlgorithm)CryptoConfig.CreateFromName("MD5");
                byte[] V = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 2, 2, 0, 6, 8, 2 };
                RijndaelManaged Decrypt = new RijndaelManaged();
                Decrypt.Key = HashPassword.ComputeHash(PP);
                ICryptoTransform decryptor = Decrypt.CreateDecryptor(Decrypt.Key, V);
                MemoryStream ms = new MemoryStream(DataEncryptedByte);
                CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                byte[] Result = new byte[DataEncryptedByte.Length];
                cs.Read(Result, 0, Result.Length);
                ms.Close();
                cs.Close();
                var result = Encoding.Unicode.GetString(Result).Replace("\0", "");
                return result;
            }
            catch
            {
                return "";
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="objectType"></param>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="isIncludeAuditColumn"></param>
        /// <returns></returns>
        public static object CopyObject(Type objectType, object source, object destination, bool isIncludeAuditColumn)
        {
            try
            {
                var jSource = JObject.FromObject(source);

                var jDestination = JObject.FromObject(destination);

                if (!isIncludeAuditColumn)
                {
                    jSource.Property(TableColumnConst.CREATED_DATE).Remove();
                    jSource.Property(TableColumnConst.CREATED_BY).Remove();
                    jSource.Property(TableColumnConst.MODIFIED_DATE).Remove();
                    jSource.Property(TableColumnConst.MODIFIED_BY).Remove();
                    jSource.Property(TableColumnConst.SCOPE).Remove();
                    jSource.Property(TableColumnConst.SCOPE_TYPE).Remove();
                    jSource.Property(TableColumnConst.MODIFIED_TYPE_COL).Remove();
                    jSource.Property(TableColumnConst.TRANSFER_TIME).Remove();
                    jSource.Property(TableColumnConst.CREATED_AT).Remove();
                }

                foreach (var item in jSource.Properties())
                {
                    jDestination.Property(item.Name).Value = item.Value;
                }
                return jDestination.ToObject(objectType);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T ConvertJsonToObject<T>(string json)
        {
            var jsonSerializerSettings = new JsonSerializerSettings();
            jsonSerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;
            jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;

            jsonSerializerSettings.Converters.Add(new FormatConverter());
            return JsonConvert.DeserializeObject<T>(json, jsonSerializerSettings);
        }

        /// <summary>
        /// Encrypt String With XOR to Hex
        /// </summary>
        /// <param name="input"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string EncryptStringWithXORtoHex(string input, string key)
        {
            var c = "";
            while (key.Length < input.Length)
            {
                key += key;
            }
            for (var i = 0; i < input.Length; i++)
            {
                var value1 = input[i];

                var value2 = key[i];

                var xorValue = value1 ^ value2;

                var xorValueAsHexString = xorValue.ToString("X2");

                if (xorValueAsHexString.Length < 2)
                {
                    xorValueAsHexString = "0" + xorValueAsHexString;
                }

                c += xorValueAsHexString;
            }
            return c;
        }
    }

    public static class Base32
    {
        private static readonly string _base32Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

        public static string ToBase32(byte[] input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            StringBuilder sb = new StringBuilder();
            for (int offset = 0; offset < input.Length;)
            {
                byte a, b, c, d, e, f, g, h;
                int numCharsToOutput = GetNextGroup(input, ref offset, out a, out b, out c, out d, out e, out f, out g, out h);

                sb.Append((numCharsToOutput >= 1) ? _base32Chars[a] : '=');
                sb.Append((numCharsToOutput >= 2) ? _base32Chars[b] : '=');
                sb.Append((numCharsToOutput >= 3) ? _base32Chars[c] : '=');
                sb.Append((numCharsToOutput >= 4) ? _base32Chars[d] : '=');
                sb.Append((numCharsToOutput >= 5) ? _base32Chars[e] : '=');
                sb.Append((numCharsToOutput >= 6) ? _base32Chars[f] : '=');
                sb.Append((numCharsToOutput >= 7) ? _base32Chars[g] : '=');
                sb.Append((numCharsToOutput >= 8) ? _base32Chars[h] : '=');
            }

            return sb.ToString();
        }

        public static byte[] FromBase32(string input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }
            input = input.TrimEnd('=').ToUpperInvariant();
            if (input.Length == 0)
            {
                return new byte[0];
            }

            var output = new byte[input.Length * 5 / 8];
            var bitIndex = 0;
            var inputIndex = 0;
            var outputBits = 0;
            var outputIndex = 0;
            while (outputIndex < output.Length)
            {
                var byteIndex = _base32Chars.IndexOf(input[inputIndex]);
                if (byteIndex < 0)
                {
                    throw new FormatException();
                }

                var bits = Math.Min(5 - bitIndex, 8 - outputBits);
                output[outputIndex] <<= bits;
                output[outputIndex] |= (byte)(byteIndex >> (5 - (bitIndex + bits)));

                bitIndex += bits;
                if (bitIndex >= 5)
                {
                    inputIndex++;
                    bitIndex = 0;
                }

                outputBits += bits;
                if (outputBits >= 8)
                {
                    outputIndex++;
                    outputBits = 0;
                }
            }
            return output;
        }

        // returns the number of bytes that were output
        private static int GetNextGroup(byte[] input, ref int offset, out byte a, out byte b, out byte c, out byte d, out byte e, out byte f, out byte g, out byte h)
        {
            uint b1, b2, b3, b4, b5;

            int retVal;
            switch (offset - input.Length)
            {
                case 1: retVal = 2; break;
                case 2: retVal = 4; break;
                case 3: retVal = 5; break;
                case 4: retVal = 7; break;
                default: retVal = 8; break;
            }

            b1 = (offset < input.Length) ? input[offset++] : 0U;
            b2 = (offset < input.Length) ? input[offset++] : 0U;
            b3 = (offset < input.Length) ? input[offset++] : 0U;
            b4 = (offset < input.Length) ? input[offset++] : 0U;
            b5 = (offset < input.Length) ? input[offset++] : 0U;

            a = (byte)(b1 >> 3);
            b = (byte)(((b1 & 0x07) << 2) | (b2 >> 6));
            c = (byte)((b2 >> 1) & 0x1f);
            d = (byte)(((b2 & 0x01) << 4) | (b3 >> 4));
            e = (byte)(((b3 & 0x0f) << 1) | (b4 >> 7));
            f = (byte)((b4 >> 2) & 0x1f);
            g = (byte)(((b4 & 0x3) << 3) | (b5 >> 5));
            h = (byte)(b5 & 0x1f);

            return retVal;
        }
    }
}