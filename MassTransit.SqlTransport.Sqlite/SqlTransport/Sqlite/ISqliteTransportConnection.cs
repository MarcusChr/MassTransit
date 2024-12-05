namespace MassTransit.SqlTransport.Sqlite.SqlTransport.Sqlite;

using Microsoft.Data.Sqlite;


public interface ISqliteTransportConnection : ISqlTransportConnection
{
    SqliteConnection Connection { get; }
}
