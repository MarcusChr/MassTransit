namespace MassTransit.Serialization;

using System.IO;


public class MessagePackMessageBody : MessageBody
{
    public long? Length { get; }

    readonly byte[] _messagePackSerializedObject;

    public MessagePackMessageBody(byte[] messagePackSerializedObject)
    {
        _messagePackSerializedObject = messagePackSerializedObject;
        Length = _messagePackSerializedObject.Length;
    }

    public Stream GetStream() => new MemoryStream(_messagePackSerializedObject, false);

    public byte[] GetBytes() => _messagePackSerializedObject;

    public string GetString() => System.Convert.ToBase64String(_messagePackSerializedObject);
}
