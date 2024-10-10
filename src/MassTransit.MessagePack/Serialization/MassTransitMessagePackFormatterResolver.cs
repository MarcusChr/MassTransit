namespace MassTransit.Serialization;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Internals;
using MessagePack;
using MessagePack.Formatters;
using MessagePackFormatters;


class MassTransitMessagePackFormatterResolver :
    IFormatterResolver
{
    public static MassTransitMessagePackFormatterResolver Instance { get; } = new();

    readonly IReadOnlyCollection<KeyValuePair<Type, Type>> _mappedFormatterByType;
    readonly ConcurrentDictionary<Type, IMessagePackFormatter> _cachedFormatters;

    MassTransitMessagePackFormatterResolver()
    {
        _mappedFormatterByType = new List<KeyValuePair<Type, Type>>
        {
            new(typeof(MessageData<>), typeof(MessageDataFormatter<>)),
        };

        _cachedFormatters = new ConcurrentDictionary<Type, IMessagePackFormatter>();
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

        if (!typeof(T).IsInterface)
        {
            // If we have no mapper for the type, and it's not an interface, we can't create a formatter.
            return null;
        }

        var createdConcreteFormatter = new InterfaceMessagePackFormatter<T>();
        _cachedFormatters[tType] = createdConcreteFormatter;

        return createdConcreteFormatter;
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
            if (!originType.ClosesType(mapPair.Key))
            {
                continue;
            }

            mappedPair = mapPair;
            return true;
        }

        mappedPair = default;
        return false;
    }
}
