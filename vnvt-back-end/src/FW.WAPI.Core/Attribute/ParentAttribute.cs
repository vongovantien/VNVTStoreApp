using System;

namespace FW.WAPI.Core.Attribute
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ParentAttribute : System.Attribute
    {
        public string ParentColName;
        public string TableName;
        public string RefColName;
        public object OptionJoin { get; set; }
        public bool InDetail { get; set; } = false;

        public ParentAttribute(string parentAttribute, string tableName = null,
            string refColName = null, object optionJoin = null, bool inDetail = false)
        {
            ParentColName = parentAttribute;
            TableName = tableName;
            RefColName = refColName;
            OptionJoin = optionJoin;
            InDetail = inDetail;
        }
    }

    public class OptionJoin
    {
        public string From { get; set; }
        public string To { get; set; }
    }
}