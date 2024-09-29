namespace MassTransit.Serialization;

using System;
using System.Net.Mime;
using MessagePack;
using MessagePack.Resolvers;


public class MessagePackMessageSerializer : IMessageSerializer,
    IMessageDeserializer,
    IObjectDeserializer
{
    const string ContentTypeHeaderValue = "application/vnd.masstransit+msgpack";
    const string ProviderKey = "MessagePack";

    public ContentType ContentType { get; } = new(ContentTypeHeaderValue);

    public ConsumeContext Deserialize(ReceiveContext receiveContext)
    {
        var deserializerContext = Deserialize(receiveContext.Body, receiveContext.TransportHeaders, receiveContext.InputAddress);
        return new BodyConsumeContext(receiveContext, deserializerContext);
    }

    public SerializerContext Deserialize(MessageBody body, Headers headers, Uri destinationAddress = null)
    {
        var messageBuffer = body.GetBytes();
        var envelope = DeserializeMessageBuffer<MessageEnvelope>(messageBuffer);

        var messageContext = new EnvelopeMessageContext(envelope, this);

        var messageTypes = envelope.MessageType ?? [];

        return new MessagePackMessageSerializerContext(this, messageContext, messageTypes, envelope);
    }

    public MessageBody GetMessageBody(string text)
    {
        return new Base64MessageBody(text);
    }

    public MessageBody GetMessageBody<T>(SendContext<T> context)
        where T : class =>
        InternalSerializeObjectToMessagePackBody(context.Message);

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
            _ => throw new ArgumentException("The value must be a string or byte[]")
        };

    public T DeserializeObject<T>(object value, T defaultValue = default)
        where T : class =>
        InternalDeserializeObject(value, defaultValue);

    public T? DeserializeObject<T>(object value, T? defaultValue = null)
        where T : struct =>
        InternalDeserializeObject(value, defaultValue);

    public MessageBody SerializeObject(object value) => InternalSerializeObjectToMessagePackBody(value);

    static T InternalDeserializeObject<T>(object value, T defaultValue)
    {
        if (value is null || Equals(value, defaultValue))
        {
            return defaultValue;
        }

        var messageSerializedBuffer = EnsureObjectBufferFormatIsByteArray(value);

        return DeserializeMessageBuffer<T>(messageSerializedBuffer);
    }

    public static T DeserializeMessageBuffer<T>(byte[] messageBuffer)
    {
        return MessagePackSerializer.Deserialize<T>(messageBuffer, ContractlessStandardResolver.Options);
    }

    public static byte[] SerializeMessageBuffer<T>(T message)
    {
        return MessagePackSerializer.Serialize(message, ContractlessStandardResolver.Options);
    }

    static MessagePackMessageBody InternalSerializeObjectToMessagePackBody<T>(T value)
    {
        var serializedObject = SerializeMessageBuffer(value);
        return new MessagePackMessageBody(serializedObject);
    }
}
