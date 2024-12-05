namespace MassTransit.SqlTransport.Sqlite.SqlTransport.Sqlite;

using Configuration;
using Microsoft.Data.Sqlite;


public class SqliteHostSettings :
    ConfigurationSqlHostSettings
{
    readonly SqliteConnectionStringBuilder _builder;

    public string ConnectionString => _builder.ConnectionString;

    public SqliteHostSettings(string connectionString)
    {
        _builder = new SqliteConnectionStringBuilder(connectionString);
    }

    public override ConnectionContextFactory CreateConnectionContextFactory(ISqlHostConfiguration configuration)
    {
        throw new NotImplementedException();
    }
}
