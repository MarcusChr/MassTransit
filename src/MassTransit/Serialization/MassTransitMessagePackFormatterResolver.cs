namespace MassTransit.Serialization;

using System;
using System.Collections.Generic;
using MessagePack;
using MessagePack.Formatters;
using MessagePackFormatters;


internal class MassTransitMessagePackFormatterResolver : IFormatterResolver
{
    public static MassTransitMessagePackFormatterResolver Instance { get; } = new();

    readonly IReadOnlyDictionary<Type, IMessagePackFormatter> _formatters;

    private MassTransitMessagePackFormatterResolver()
    {
        _formatters = new Dictionary<Type, IMessagePackFormatter>
        {
            { typeof(HostInfo), new HostInfoFormatter() },
        };
    }

    public IMessagePackFormatter<T> GetFormatter<T>()
    {
        if (_formatters.TryGetValue(typeof(T), out var formatter))
        {
            return (IMessagePackFormatter<T>)formatter;
        }

        if (typeof(T).IsInterface)
        {
            return new InterfaceMessagePackFormatter<T>();
        }

        return null;
    }
}
