namespace FW.WAPI.Core.DAL.Model.Configuration
{
    public class TableConfig
    {
        public string TableName { get; set; }
        public string TableType { get; set; }
    }

    public class ReferenceTable
    {
        public string TableName { get; set; }
        public string ColumnName { get; set; }
        public string AliasName { get; set; }
        public string ForeignKeyCol { get; set; }
        public string FilterType { get; set; }
        public string[] OptionJoin { get; set; }
    }
}