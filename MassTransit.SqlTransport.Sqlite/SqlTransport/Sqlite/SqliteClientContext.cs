namespace MassTransit.SqlTransport.Sqlite.SqlTransport.Sqlite;

using Topology;


public class SqliteClientContext : SqlClientContext
{
    public SqliteClientContext(ConnectionContext context, CancellationToken cancellationToken)
        : base(context, cancellationToken)
    {
    }

    public override Task<long> CreateQueue(Queue queue)
    {
        throw new NotImplementedException();
    }

    public override Task<long> CreateTopic(Topic topic)
    {
        throw new NotImplementedException();
    }

    public override Task<long> CreateTopicSubscription(TopicToTopicSubscription subscription)
    {
        throw new NotImplementedException();
    }

    public override Task<long> CreateQueueSubscription(TopicToQueueSubscription subscription)
    {
        throw new NotImplementedException();
    }

    public override Task<long> PurgeQueue(string queueName, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override Task Send<T>(string queueName, SqlMessageSendContext<T> context)
    {
        throw new NotImplementedException();
    }

    public override Task Publish<T>(string topicName, SqlMessageSendContext<T> context)
    {
        throw new NotImplementedException();
    }

    public override Task<IEnumerable<SqlTransportMessage>> ReceiveMessages(string queueName, SqlReceiveMode mode, int messageLimit, int concurrentLimit, TimeSpan lockDuration)
    {
        throw new NotImplementedException();
    }

    public override Task TouchQueue(string queueName)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> DeleteMessage(Guid lockId, long messageDeliveryId)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> DeleteScheduledMessage(Guid tokenId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> MoveMessage(Guid lockId, long messageDeliveryId, string queueName, SqlQueueType queueType, SendHeaders sendHeaders)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> RenewLock(Guid lockId, long messageDeliveryId, TimeSpan duration)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> Unlock(Guid lockId, long messageDeliveryId, TimeSpan delay, SendHeaders sendHeaders)
    {
        throw new NotImplementedException();
    }
}
