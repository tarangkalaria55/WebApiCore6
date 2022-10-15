using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Application.Common.Persistence;
using Infrastructure.Common;
using System.Data.SqlClient;

namespace Infrastructure.Persistence.ConnectionString;
internal class ConnectionStringValidator : IConnectionStringValidator
{
    private readonly DatabaseSettings _dbSettings;
    private readonly ILogger<ConnectionStringValidator> _logger;

    public ConnectionStringValidator(IOptions<DatabaseSettings> dbSettings, ILogger<ConnectionStringValidator> logger)
    {
        _dbSettings = dbSettings.Value;
        _logger = logger;
    }

    public bool TryValidate(string connectionString)
    {

        try
        {
            var mssqlcs = new SqlConnectionStringBuilder(connectionString);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Connection String Validation Exception : {ex.Message}");
            return false;
        }
    }
}