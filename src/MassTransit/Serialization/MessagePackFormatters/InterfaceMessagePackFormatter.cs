namespace MassTransit.Serialization.MessagePackFormatters;

using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using MessagePack;
using MessagePack.Formatters;
using Metadata;


delegate void SerializeDelegate<in TConcrete>(ref MessagePackWriter writer, TConcrete value, MessagePackSerializerOptions options);
delegate TConcrete DeserializeDelegate<out TConcrete>(ref MessagePackReader reader, MessagePackSerializerOptions options);

public class InterfaceMessagePackFormatter<TInterface> : IMessagePackFormatter<TInterface>
{
    static readonly Type _targetType;
    static readonly MethodInfo _getFormatterMethodInfo;
    static readonly MethodInfo _serializeMethodInfo;
    static readonly MethodInfo _deserializeMethodInfo;

    static InterfaceMessagePackFormatter()
    {
        _targetType = TypeMetadataCache.GetImplementationType(typeof(TInterface));

        _getFormatterMethodInfo = typeof(IFormatterResolver)
            .GetMethod(nameof(IFormatterResolver.GetFormatter))
            .MakeGenericMethod(_targetType);

        var formatterType = typeof(IMessagePackFormatter<>)
            .MakeGenericType(_targetType);

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
        var valueParameterForCall = Expression.Parameter(_targetType, "value");
        var optionsParameter = Expression.Parameter(typeof(MessagePackSerializerOptions), "options");

        var formatterInstance = Expression.Constant(formatter);
        var call = Expression.Call(formatterInstance, _serializeMethodInfo, writerParameter, valueParameterForCall, optionsParameter);

        var delegateType = typeof(SerializeDelegate<>).MakeGenericType(_targetType);

        var proxyFuncDelegate = Expression.Lambda(delegateType, call, writerParameter, valueParameterForCall, optionsParameter).Compile();

        var proxyFunc =  Unsafe.As<SerializeDelegate<TInterface>>(proxyFuncDelegate);

        proxyFunc(ref writer, value, options);
    }

    public TInterface Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
    {
        var formatter = _getFormatterMethodInfo.Invoke(options.Resolver, BindingFlags.Default, null, null, null);

        var readerParameter = Expression.Parameter(typeof(MessagePackReader).MakeByRefType(), "reader");
        var optionsParameter = Expression.Parameter(typeof(MessagePackSerializerOptions), "options");

        var formatterInstance = Expression.Constant(formatter);
        var call = Expression.Call(formatterInstance, _deserializeMethodInfo, readerParameter, optionsParameter);
        var proxyFunc = Expression.Lambda<DeserializeDelegate<TInterface>>(call, readerParameter, optionsParameter).Compile();

        return proxyFunc(ref reader, options);
    }
}
