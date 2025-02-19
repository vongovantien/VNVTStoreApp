using Npgsql;
using System.Data.Common;
using System.Data.SqlClient;
using static FW.WAPI.Core.General.EnumTypes;

namespace FW.WAPI.Core.Repository
{
    public static class DbConnectionFactory
    {
        public static DbConnection CreateDbConnection(DatabaseProvider databaseProvider, string connectionString)
        {
            DbConnection dbConnection = null;

            switch (databaseProvider)
            {
                case DatabaseProvider.MSSQL:
                    dbConnection = new SqlConnection(connectionString);
                    break;
                case DatabaseProvider.POSTGRESQL:
                    dbConnection = new NpgsqlConnection(connectionString);
                    break;
                default:
                    break;
            }

            return dbConnection;
        }
    }
}
