using System;
using System.Net.Mime;
using MassTransit.Serialization;

namespace MassTransit.Configuration;

public class MessagePackSerializerFactory : ISerializerFactory
{
    public ContentType ContentType => MessagePackMessageSerializer.MessagePackContentType;

    readonly Lazy<MessagePackMessageSerializer> _serializer;

    public MessagePackSerializerFactory()
    {
        _serializer = new Lazy<MessagePackMessageSerializer>(() => new MessagePackMessageSerializer());
    }

    public IMessageSerializer CreateSerializer()
        => _serializer.Value;

    public IMessageDeserializer CreateDeserializer()
        => _serializer.Value;
}
