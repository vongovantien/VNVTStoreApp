using System.Data;

namespace VNVTStore.Application.Interfaces;

public interface IDapperContext
{
    IDbConnection CreateConnection();
}
