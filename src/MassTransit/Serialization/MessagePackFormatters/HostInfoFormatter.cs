namespace MassTransit.Serialization.MessagePackFormatters;

using MessagePack;
using MessagePack.Formatters;
using Metadata;

internal class HostInfoFormatter : IMessagePackFormatter<HostInfo>
{
    public void Serialize(ref MessagePackWriter writer, HostInfo value, MessagePackSerializerOptions options)
    {
        var busHostInfo = value as BusHostInfo;
        var innerFormatter = options.Resolver.GetFormatterWithVerify<BusHostInfo>();

        innerFormatter.Serialize(ref writer, busHostInfo, options);
    }

    public HostInfo Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var innerFormatter = options.Resolver.GetFormatterWithVerify<BusHostInfo>();

        return innerFormatter.Deserialize(ref reader, options);
    }
}
