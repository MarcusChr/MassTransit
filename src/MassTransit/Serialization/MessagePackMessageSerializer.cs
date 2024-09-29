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
        var envelope = InternalDeserializeMessageBuffer<MessageEnvelope>(messageBuffer);

        var messageContext = new EnvelopeMessageContext(envelope, this);

        var messageTypes = envelope.MessageType ?? [];

        return new MessagePackMessageSerializerContext(this, messageContext, messageTypes);
    }

    public MessageBody GetMessageBody(string text)
    {
        var messagePackSerializedObjectFromBase64 = Convert.FromBase64String(text);

        return new BytesMessageBody(messagePackSerializedObjectFromBase64);
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

    public T DeserializeObject<T>(object value, T defaultValue = default(T))
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

        var messageSerializedBuffer = value switch
        {
            string base64EncodedMessagePackBody => Convert.FromBase64String(base64EncodedMessagePackBody),
            byte[] messagePackBody => messagePackBody,
            _ => throw new ArgumentException("The value must be a string or byte[]")
        };

        return InternalDeserializeMessageBuffer<T>(messageSerializedBuffer);
    }

    static T InternalDeserializeMessageBuffer<T>(byte[] messageBuffer)
    {
        return MessagePackSerializer.Deserialize<T>(messageBuffer, ContractlessStandardResolver.Options);
    }

    static MessagePackMessageBody InternalSerializeObjectToMessagePackBody<T>(T value)
    {
        var serializedObject = MessagePackSerializer.Serialize(value, ContractlessStandardResolver.Options);
        return new MessagePackMessageBody(serializedObject);
    }
}
