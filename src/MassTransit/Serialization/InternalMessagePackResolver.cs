namespace MassTransit.Serialization;

using MessagePack;
using MessagePack.Formatters;
using MessagePack.Resolvers;
using MessagePackFormatters;


internal static class InternalMessagePackResolver
{
    static IFormatterResolver InternalResolverInstance { get; } =
        CompositeResolver.Create(ContractlessStandardResolverAllowPrivate.Instance, MassTransitMessagePackFormatterResolver.Instance, DynamicGenericResolver.Instance);

    public static MessagePackSerializerOptions Options { get; } = MessagePackSerializerOptions.Standard.WithResolver(InternalResolverInstance);
}
