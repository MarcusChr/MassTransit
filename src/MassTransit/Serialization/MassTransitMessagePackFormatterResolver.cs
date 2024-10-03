namespace MassTransit.Serialization;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Internals;
using MessagePack;
using MessagePack.Formatters;
using MessagePackFormatters;


internal class MassTransitMessagePackFormatterResolver : IFormatterResolver
{
    public static MassTransitMessagePackFormatterResolver Instance { get; } = new();

    readonly IReadOnlyCollection<KeyValuePair<Type, Type>> _mappedFormatterByType;
    readonly IDictionary<Type, IMessagePackFormatter> _cachedFormatters;

    private MassTransitMessagePackFormatterResolver()
    {
        _mappedFormatterByType = new List<KeyValuePair<Type, Type>>
        {
            new(typeof(MessageData<>), typeof(MessageDataFormatter<>)),
        };

        _cachedFormatters = new Dictionary<Type, IMessagePackFormatter>
        {
            { typeof(HostInfo), new HostInfoFormatter() },
        };
    }

    public IMessagePackFormatter<T> GetFormatter<T>()
    {
        var tType = typeof(T);

        if (_cachedFormatters.TryGetValue(tType, out var cachedFormatter))
        {
            return (IMessagePackFormatter<T>)cachedFormatter;
        }

        if (TryGetMappedType(tType, out var mappedPair))
        {
            var formatterType = mappedPair.Value;

            EnsureFormatterTypeHasGenericParametersOfOriginalType(tType, ref formatterType);

            var formatter = (IMessagePackFormatter)Activator.CreateInstance(formatterType);
            _cachedFormatters[tType] = formatter;
            return (IMessagePackFormatter<T>)formatter;
        }

        if (typeof(T).IsInterface)
        {
            return new InterfaceMessagePackFormatter<T>();
        }

        return null;
    }

    static void EnsureFormatterTypeHasGenericParametersOfOriginalType(Type tType, ref Type formatterType)
    {
        if (!formatterType.IsGenericType)
        {
            return;
        }

        formatterType = formatterType
            .MakeGenericType(tType.GenericTypeArguments);
    }

    bool TryGetMappedType(Type originType, out KeyValuePair<Type, Type> mappedPair)
    {
        foreach (var mapPair in _mappedFormatterByType)
        {
            if (originType.ClosesType(mapPair.Key))
            {
                mappedPair = mapPair;
                return true;
            }
        }

        mappedPair = default;
        return false;
    }
}
