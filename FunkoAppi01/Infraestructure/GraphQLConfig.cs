using FunkoAppi01.GraphQL;

namespace FunkoAppi01.Infraestructure;

/// <summary>
/// GraphQL (HotChocolate) convive con los controladores REST, no los sustituye:
/// ambos hablan con los mismos Service para no duplicar reglas de negocio.
/// </summary>
public static class GraphQLConfig
{
    public static IServiceCollection AddGraphQLConfig(this IServiceCollection services, IWebHostEnvironment environment)
    {
        services
            .AddGraphQLServer()
            .AddAuthorization()
            .AddQueryType<FunkosQuery>()
            .AddMutationType<FunkoMutation>()
            .AddSubscriptionType<FunkoSubscription>()
            .AddInMemorySubscriptions()
            .ModifyRequestOptions(options => options.IncludeExceptionDetails = environment.IsDevelopment());

        return services;
    }

    public static WebApplication MapGraphQLEndpoints(this WebApplication app)
    {
        // Imprescindible para las subscriptions: sin esto, la petición de
        // actualización a WebSocket nunca llega a completarse (HotChocolate
        // necesita que el pipeline sepa gestionar el handshake).
        app.UseWebSockets();
        app.MapGraphQL();
        return app;
    }
}
