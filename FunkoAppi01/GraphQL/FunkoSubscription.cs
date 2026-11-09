using FunkoAppi01.Dto;
using HotChocolate;
using HotChocolate.Execution;

namespace FunkoAppi01.GraphQL;

/// <summary>
/// Raíz de las subscriptions GraphQL. Los eventos los publica <see cref="FunkoMutation"/>
/// con <c>ITopicEventSender</c> tras cada escritura que tiene éxito.
/// </summary>
public class FunkoSubscription
{
    [Subscribe]
    [Topic("FunkoCreado")]
    public FunkoDto FunkoCreado([EventMessage] FunkoDto funko) => funko;

    [Subscribe]
    [Topic("FunkoActualizado")]
    public FunkoDto FunkoActualizado([EventMessage] FunkoDto funko) => funko;

    [Subscribe]
    [Topic("FunkoEliminado")]
    public long FunkoEliminado([EventMessage] long id) => id;
}
