namespace MassTransit.Serialization;

using System;
using System.Collections.Generic;
using MessagePack;
using Metadata;


public class MessagePackEnvelope : MessageEnvelope
{
    public string MessageId { get; set; }
    public string RequestId { get; set; }
    public string CorrelationId { get; set; }
    public string ConversationId { get; set; }
    public string InitiatorId { get; set; }
    public string SourceAddress { get; set; }
    public string DestinationAddress { get; set; }
    public string ResponseAddress { get; set; }
    public string FaultAddress { get; set; }
    public string[] MessageType { get; set; }
    public object Message { get; set; }
    public DateTime? ExpirationTime { get; set; }
    public DateTime? SentTime { get; set; }
    public Dictionary<string, object> Headers { get; set; }
    public HostInfo Host { get; set; }

    public MessagePackEnvelope(SendContext context, object message)
    {
        if (context.MessageId.HasValue)
            MessageId = context.MessageId.Value.ToString();

        if (context.RequestId.HasValue)
            RequestId = context.RequestId.Value.ToString();

        if (context.CorrelationId.HasValue)
            CorrelationId = context.CorrelationId.Value.ToString();

        if (context.ConversationId.HasValue)
            ConversationId = context.ConversationId.Value.ToString();

        if (context.InitiatorId.HasValue)
            InitiatorId = context.InitiatorId.Value.ToString();

        if (context.SourceAddress != null)
            SourceAddress = context.SourceAddress.ToString();

        if (context.DestinationAddress != null)
            DestinationAddress = context.DestinationAddress.ToString();

        if (context.ResponseAddress != null)
            ResponseAddress = context.ResponseAddress.ToString();

        if (context.FaultAddress != null)
            FaultAddress = context.FaultAddress.ToString();

        MessageType = context.SupportedMessageTypes;

        Message = MessagePackSerializer.Serialize(message, InternalMessagePackResolver.Options);

        if (context.TimeToLive.HasValue)
            ExpirationTime = DateTime.UtcNow + context.TimeToLive;

        SentTime = context.SentTime ?? DateTime.UtcNow;

        Headers = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        foreach (KeyValuePair<string, object> header in context.Headers.GetAll())
            Headers[header.Key] = header.Value;

        Host = HostMetadataCache.Host;
    }

    /// <summary>
    /// Used for deserialization.
    /// </summary>
    private MessagePackEnvelope()
    {
    }
}
