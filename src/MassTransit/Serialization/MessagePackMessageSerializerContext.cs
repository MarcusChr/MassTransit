namespace MassTransit.Serialization;

using System;
using System.Collections.Generic;


public class MessagePackMessageSerializerContext : BaseSerializerContext
{
    public MessagePackMessageSerializerContext(IObjectDeserializer deserializer, MessageContext context, string[] supportedMessageTypes)
        : base(deserializer, context, supportedMessageTypes)
    {
    }

    public override bool TryGetMessage<T>(out T message)
        where T : class
    {
        throw new NotImplementedException();
    }

    public override bool TryGetMessage(Type messageType, out object message)
    {
        throw new NotImplementedException();
    }

    public override IMessageSerializer GetMessageSerializer()
    {
        throw new NotImplementedException();
    }

    public override IMessageSerializer GetMessageSerializer<T>(MessageEnvelope envelope, T message)
    {
        throw new NotImplementedException();
    }

    public override IMessageSerializer GetMessageSerializer(object message, string[] messageTypes)
    {
        throw new NotImplementedException();
    }

    public override Dictionary<string, object> ToDictionary<T>(T message)
        where T : class
    {
        throw new NotImplementedException();
    }
}
