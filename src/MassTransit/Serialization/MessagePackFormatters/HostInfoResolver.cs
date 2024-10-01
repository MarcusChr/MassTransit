namespace MassTransit.Serialization.MessagePackFormatters;

using MessagePack;
using MessagePack.Formatters;
using Metadata;


public sealed class HostInfoResolver : IFormatterResolver
{
    public static HostInfoResolver Instance { get; } = new HostInfoResolver();

    readonly HostInfoFormatter _formatter;

    private HostInfoResolver()
    {
        _formatter = new HostInfoFormatter();
    }

    public IMessagePackFormatter<T> GetFormatter<T>()
    {
        // If the type is HostInfo, we can handle it. Otherwise, return null to let another resolver handle it.
        return typeof(T) == typeof(HostInfo) ? (IMessagePackFormatter<T>)_formatter : null;
    }

    class HostInfoFormatter : IMessagePackFormatter<HostInfo>
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
}
