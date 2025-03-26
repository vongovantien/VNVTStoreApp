using System;

namespace FW.WAPI.Core.Attribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
    public class OneToOneAttribute : System.Attribute
    {
        private readonly Type _referenceTableType;
        private readonly string _refColumn;
        private readonly string _colVal;
        private readonly string _originColumn;

        public OneToOneAttribute(Type referenceTableType, string originColumn, string refColumn, string colValName)
        {
            _referenceTableType = referenceTableType;
            _refColumn = refColumn;
            _colVal = colValName;
            _originColumn = originColumn;
        }

        public Type ReferenceTableType { get => _referenceTableType; }
        public string RefColumn { get => _refColumn; }
        public string ColVal { get => _colVal; }
        public string OriginColumn { get => _originColumn; }
    }
}
