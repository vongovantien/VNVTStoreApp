using System;

namespace FW.WAPI.Core.Attribute
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ChildListAttribute : System.Attribute
    {
        public string ForeignKeyCode { get; set; }
        public string CombineKey { get; set; }
        public bool ManyToManyRelation { get; set; }
        public bool IsIdentity { get; set; }
        public bool CascadeDelete { get; set; }
        public object OptionJoin { get; set; }

        public ChildListAttribute(string foreignKeyCode, string combineKey = null,
            bool manyToManyRelation = false, bool IsIdentity = false, bool cascadeDelete = true, object optionJoin = null)
        {
            ForeignKeyCode = foreignKeyCode;
            CombineKey = combineKey;
            ManyToManyRelation = manyToManyRelation;
            this.IsIdentity = IsIdentity;
            CascadeDelete = cascadeDelete;
            OptionJoin = optionJoin;
        }
    }
}