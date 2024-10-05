namespace MassTransit.Serialization;

using System;
using System.Collections.Generic;
using MessagePack;


public class MessagePackMessageSerializerContext : BaseSerializerContext
{
    readonly MessageEnvelope _envelope;
    readonly MessagePackMessageSerializer _serializer;

    public MessagePackMessageSerializerContext(MessagePackMessageSerializer serializer, MessageContext context, string[] supportedMessageTypes,
        MessageEnvelope envelope)
        : base(serializer, context, supportedMessageTypes)
    {
        _serializer = serializer;
        _envelope = envelope;
    }

    public override bool TryGetMessage<T>(out T message)
        where T : class
    {
        if (!TryGetMessage(typeof(T), out var outMessage))
        {
            message = default;
            return false;
        }

        message = (T)outMessage;
        return true;
    }

    public override bool TryGetMessage(Type messageType, out object message)
    {
        try
        {
            var messagePackSerializedObjectBuffer = MessagePackMessageSerializer.EnsureObjectBufferFormatIsByteArray(_envelope.Message);
            message = MessagePackSerializer.Deserialize(messageType, messagePackSerializedObjectBuffer, InternalMessagePackResolver.Options);

            return true;
        }
        catch
        {
            message = default;
            return false;
        }
    }

    public override IMessageSerializer GetMessageSerializer() => _serializer;

    public override IMessageSerializer GetMessageSerializer<T>(MessageEnvelope envelope, T message)
    {
        throw new NotImplementedException();
    }

    public override IMessageSerializer GetMessageSerializer(object message, string[] messageTypes) => _serializer;

    public override Dictionary<string, object> ToDictionary<T>(T message)
        where T : class
    {
        var body = MessagePackSerializer.Serialize(message, InternalMessagePackResolver.Options);

        return new Dictionary<string, object> { { MessagePackMessageSerializer.FutureStateObjectKey, body } };
    }
}
