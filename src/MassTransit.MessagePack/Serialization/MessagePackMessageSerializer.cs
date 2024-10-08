namespace MassTransit.Serialization;

using System;
using System.Collections.Generic;
using System.Net.Mime;
using MessagePack;


public class MessagePackMessageSerializer : IMessageSerializer,
    IMessageDeserializer,
    IObjectDeserializer
{
    const string ContentTypeHeaderValue = "application/vnd.masstransit+msgpack";
    const string ProviderKey = "MessagePack";

    public static readonly ContentType MessagePackContentType = new(ContentTypeHeaderValue);

    internal const string FutureStateObjectKey = "body";

    public ContentType ContentType => MessagePackContentType;

    public ConsumeContext Deserialize(ReceiveContext receiveContext)
    {
        var serializerContext = Deserialize(receiveContext.Body, receiveContext.TransportHeaders, receiveContext.InputAddress);
        return new BodyConsumeContext(receiveContext, serializerContext);
    }

    public SerializerContext Deserialize(MessageBody body, Headers headers, Uri destinationAddress = null)
    {
        var messageBuffer = body.GetBytes();
        var envelope = DeserializeMessageBuffer<MessagePackEnvelope>(messageBuffer);

        var messageContext = new EnvelopeMessageContext(envelope, this);

        var messageTypes = envelope.MessageType ?? [];

        return new MessagePackMessageSerializerContext(this, messageContext, messageTypes, envelope);
    }

    public MessageBody GetMessageBody(string text)
    {
        return new Base64MessageBody(text);
    }

    public MessageBody GetMessageBody<T>(SendContext<T> context)
        where T : class
    {
        return new MessagePackMessageBody<T>(context);
    }

    public void Probe(ProbeContext context)
    {
        var scope = context.CreateScope("messagepack");
        scope.Add("contentType", ContentType.MediaType);
        scope.Add("provider", ProviderKey);
    }

    public static byte[] EnsureObjectBufferFormatIsByteArray(object serializedObjectAsUnknownFormat) =>
        serializedObjectAsUnknownFormat switch
        {
            string base64EncodedMessagePackBody => Convert.FromBase64String(base64EncodedMessagePackBody),
            byte[] messagePackBody => messagePackBody,
            Dictionary<string, object> futureState when futureState.TryGetValue(FutureStateObjectKey, out var futureBody) => EnsureObjectBufferFormatIsByteArray(futureBody),
            _ => throw new ArgumentException("The value must be a string or byte[]")
        };

    public T DeserializeObject<T>(object value, T defaultValue = default)
        where T : class
    {

        if (value is Dictionary<string, object> objectByStringPairs
            && !objectByStringPairs.ContainsKey(FutureStateObjectKey))
        {
            return SystemTextJsonMessageSerializer.Instance.DeserializeObject(objectByStringPairs, defaultValue);
        }

        return InternalDeserializeObject(value, defaultValue);
    }

    public T? DeserializeObject<T>(object value, T? defaultValue = null)
        where T : struct =>
        InternalDeserializeObject(value, defaultValue);

    public MessageBody SerializeObject(object value)
    {
        if (value is null)
        {
            return new EmptyMessageBody();
        }

        return new MessagePackMessageBody<object>(value);
    }

    static T InternalDeserializeObject<T>(object value, T defaultValue)
    {
        if (value is null || Equals(value, defaultValue))
        {
            return defaultValue;
        }

        var messageSerializedBuffer = EnsureObjectBufferFormatIsByteArray(value);

        return DeserializeMessageBuffer<T>(messageSerializedBuffer);
    }

    static T DeserializeMessageBuffer<T>(byte[] messageBuffer)
    {
        return MessagePackSerializer.Deserialize<T>(messageBuffer, InternalMessagePackResolver.Options);
    }
}
