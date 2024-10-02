namespace MassTransit.Serialization.MessagePackFormatters;

using System;
using System.Linq.Expressions;
using System.Reflection;
using MessagePack;
using MessagePack.Formatters;
using Metadata;


public class InterfaceMessagePackFormatter<TInterface> : IMessagePackFormatter<TInterface>
{
    delegate void SerializeDelegate(ref MessagePackWriter writer, object value, MessagePackSerializerOptions options);


    delegate TInterface DeserializeDelegate(ref MessagePackReader reader, MessagePackSerializerOptions options);


    static readonly MethodInfo _getFormatterMethodInfo;
    static readonly MethodInfo _serializeMethodInfo;
    static readonly MethodInfo _deserializeMethodInfo;

    static InterfaceMessagePackFormatter()
    {
        var targetType = TypeMetadataCache.GetImplementationType(typeof(TInterface));

        _getFormatterMethodInfo = typeof(IFormatterResolver)
            .GetMethod(nameof(IFormatterResolver.GetFormatter))
            .MakeGenericMethod(targetType);

        var formatterType = typeof(IMessagePackFormatter<>)
            .MakeGenericType(targetType);

        _serializeMethodInfo = formatterType
            .GetMethod(nameof(IMessagePackFormatter<object>.Serialize));

        _deserializeMethodInfo = formatterType
            .GetMethod(nameof(IMessagePackFormatter<object>.Deserialize));
    }

    public void Serialize(ref MessagePackWriter writer, TInterface value, MessagePackSerializerOptions options)
    {
        // IMessagePackFormatter of unknown type
        var formatter = _getFormatterMethodInfo.Invoke(options.Resolver, BindingFlags.Default, null, null, null);

        // Call Serialize method of IMessagePackFormatter
        var writerParameter = Expression.Parameter(typeof(MessagePackWriter).MakeByRefType(), "writer");
        var valueParameter = Expression.Parameter(typeof(object), "value");
        var optionsParameter = Expression.Parameter(typeof(MessagePackSerializerOptions), "options");

        var formatterInstance = Expression.Constant(formatter);
        var call = Expression.Call(formatterInstance, _serializeMethodInfo, writerParameter, valueParameter, optionsParameter);
        var proxyFunc = Expression.Lambda<SerializeDelegate>(call, writerParameter, valueParameter, optionsParameter).Compile();

        proxyFunc(ref writer, value, options);
    }

    public TInterface Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var formatter = _getFormatterMethodInfo.Invoke(options.Resolver, BindingFlags.Default, null, null, null);

        var readerParameter = Expression.Parameter(typeof(MessagePackReader).MakeByRefType(), "reader");
        var optionsParameter = Expression.Parameter(typeof(MessagePackSerializerOptions), "options");

        var formatterInstance = Expression.Constant(formatter);
        var call = Expression.Call(formatterInstance, _deserializeMethodInfo, readerParameter, optionsParameter);
        var proxyFunc = Expression.Lambda<DeserializeDelegate>(call, readerParameter, optionsParameter).Compile();

        return proxyFunc(ref reader, options);
    }
}
