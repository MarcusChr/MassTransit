#nullable enable
namespace MassTransit.Serialization;

using System;
using System.IO;
using MessagePack;
using MessagePack.Resolvers;


public class MessagePackMessageBody<TMessage> : MessageBody
    where TMessage : class
{
    public long? Length => _lazyMessagePackSerializedObject.Value.Length;

    readonly Lazy<byte[]> _lazyMessagePackSerializedObject;

    public MessagePackMessageBody(SendContext<TMessage> context, MessagePackEnvelope? envelope = null)
    {
        _lazyMessagePackSerializedObject = new Lazy<byte[]>(() =>
        {
            var envelopeToSerialize = envelope ?? new MessagePackEnvelope(context, context.Message);

            return MessagePackSerializer.Serialize(envelopeToSerialize, InternalMessagePackResolver.Options);
        });
    }

    public MessagePackMessageBody(TMessage message)
    {
        _lazyMessagePackSerializedObject = new Lazy<byte[]>(() => MessagePackSerializer.Serialize(message));
    }

    public Stream GetStream() => new MemoryStream(_lazyMessagePackSerializedObject.Value, false);

    public byte[] GetBytes() => _lazyMessagePackSerializedObject.Value;

    public string GetString() => Convert.ToBase64String(_lazyMessagePackSerializedObject.Value);
}
