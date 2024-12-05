namespace MassTransit.SqlTransport.Sqlite.SqlTransport.Sqlite;

using System.Data;
using Configuration;
using MassTransit.Middleware;
using Microsoft.Data.Sqlite;
using Transports;


public class SqliteDbConnectionContext :
    BasePipeContext,
    ConnectionContext,
    IAsyncDisposable
{
    public Uri HostAddress => _hostConfiguration.HostAddress;
    public string? Schema => null; // SQLite does not support schemas.
    public IsolationLevel IsolationLevel => _hostConfiguration.Settings.IsolationLevel;

    readonly ISqlHostConfiguration _hostConfiguration;
    readonly SqliteHostSettings _hostSettings;

    public SqliteDbConnectionContext(ISqlHostConfiguration hostConfiguration, ITransportSupervisor<ConnectionContext> supervisor)
        : base(supervisor.Stopped)
    {
        _hostConfiguration = hostConfiguration;
        _hostSettings = (SqliteHostSettings)hostConfiguration.Settings;
    }

    public ClientContext CreateClientContext(CancellationToken cancellationToken)
    {
        return new SqliteClientContext(this, cancellationToken);
    }

    public async Task<ISqlTransportConnection> CreateConnection(CancellationToken cancellationToken)
    {
        var connectionString = _hostSettings.ConnectionString;

        var connection = new SqliteConnection(connectionString);

        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        return new SqliteTransportConnection(connection);
    }

    public Task DelayUntilMessageReady(long queueId, TimeSpan timeout, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<T> Query<T>(Func<IDbConnection, IDbTransaction, Task<T>> callback, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public ValueTask DisposeAsync() => default;
}
