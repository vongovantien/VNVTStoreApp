using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace FW.WAPI.Core.DAL.Model.Slack
{
    public class SlackMessage
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }
        [JsonProperty("text")]
        public string Text { get; set; }
        //[JsonProperty("blocks")]
        //public string Blocks { get; set; }
        //[JsonProperty("attachments")]
        //public string Attachments { get; set; }        
    }

    public static class Slack
    {
        // replace "&" with "&amp;" 
        public const string _Ampersand = "&amp;";
        // replace "<" with "&lt;" 
        public const string _LessThanSign = "&lt;";
        // replace ">" with "&gt;" 
        public const string _GreaterThanSign = "&gt;";
        public const string _Italic = "_";
        public const string _Bold = "*";
        public const string _Strike = "~";
        public const string _LineBreak = "\n";
        public const string _Quotes = ">";
        public const string _InlineCode = "`";
        public const string _MultiLineCode = "```";
        public const string _List = "- ";

        // Icon
        public const string _Alert = "🔔";
        public const string _Error = "‼️";
        public const string _Warning = "⚠️";

        public static string Italic(string text)
        {
            return $"{_Italic}{text}{_Italic}";
        }

        public static string Bold(string text)
        {
            return $"{_Bold}{text}{_Bold}";
        }

        public static string Strike(string text)
        {
            return $"{_Strike}{text}{_Strike}";
        }

        public static string Quotes(List<string> listLineText)
        {
            var result = "";

            foreach(var line in listLineText)
            {
                result += $"{_Quotes}{line}{_LineBreak}";
            }

            return result;
        }

        public static string InlineCode(string text)
        {
            return $"{_InlineCode}{text}{_InlineCode}";
        }

        public static string MultiLineCode(List<string> listLineText)
        {
            var result = $"{_MultiLineCode}";

            foreach(var line in listLineText)
            {
                result += $"{line}{_LineBreak}";
            }

            return $"{result}{_MultiLineCode}";
        }

        public static string List(List<string> listLineText)
        {
            var result = "";

            foreach(var line in listLineText)
            {
                result += $"{_List}{line}{_LineBreak}";
            }

            return result;
        }
    }
}
