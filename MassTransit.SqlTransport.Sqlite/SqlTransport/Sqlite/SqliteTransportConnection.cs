namespace MassTransit.SqlTransport.Sqlite.SqlTransport.Sqlite;

using Microsoft.Data.Sqlite;


public class SqliteTransportConnection : ISqliteTransportConnection
{
    public SqliteTransportConnection(SqliteConnection connection)
    {
        Connection = connection;
    }

    public ValueTask DisposeAsync()
    {
        return Connection.DisposeAsync();
    }

    public SqliteConnection Connection { get; }
}
