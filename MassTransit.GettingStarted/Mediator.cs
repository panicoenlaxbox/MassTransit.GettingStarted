// Mediator.cs

using MassTransit.Mediator;

namespace MassTransit.GettingStarted;

public class SubmitOrderConsumer(ILogger<SubmitOrderConsumer> logger) : IConsumer<SubmitOrder>
{
    public async Task Consume(ConsumeContext<SubmitOrder> context)
    {
        await Task.Delay(TimeSpan.FromSeconds(3));
        logger.LogInformation("SubmitOrderConsumer: {OrderId}", context.Message.OrderId);
    }
}

public class SubmitOrderConsumer2(ILogger<SubmitOrderConsumer2> logger) : IConsumer<SubmitOrder>
{
    public async Task Consume(ConsumeContext<SubmitOrder> context)
    {
        await Task.Delay(TimeSpan.FromSeconds(3));
        logger.LogInformation("SubmitOrderConsumer2: {OrderId}", context.Message.OrderId);
    }
}

public class OrderStatusConsumer : IConsumer<GetOrderStatus>
{
    public async Task Consume(ConsumeContext<GetOrderStatus> context)
    {
        await context.RespondAsync<OrderStatus>(new
        {
            context.Message.OrderId,
            Status = "Pending"
        });
    }
}

public class OrderSubmittedConsumer(ILogger<OrderSubmittedConsumer> logger) : IConsumer<OrderSubmitted>
{
    public Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        logger.LogInformation("OrderSubmittedConsumer: {OrderId}", context.Message.OrderId);
        return Task.CompletedTask;
    }
}

public class OrderSubmittedConsumer2(ILogger<OrderSubmittedConsumer2> logger) : IConsumer<OrderSubmitted>
{
    public Task Consume(ConsumeContext<OrderSubmitted> context)
    {
        logger.LogInformation("OrderSubmittedConsumer2: {OrderId}", context.Message.OrderId);
        return Task.CompletedTask;
    }
}

public class OrderSubmitted
{
    public Guid OrderId { get; init; }
}

public class SubmitOrder
{
    public Guid OrderId { get; init; }
}

public record GetOrderStatus
{
    public Guid OrderId { get; init; }
}

public record OrderStatus
{
    public Guid OrderId { get; init; }
    public string Status { get; init; }
}

public class ValidateOrderStatusFilter<T>(ILogger<ValidateOrderStatusFilter<T>> logger)
    : IFilter<SendContext<T>> where T : class
{
    public void Probe(ProbeContext context)
    {
    }

    public Task Send(SendContext<T> context, IPipe<SendContext<T>> next)
    {
        logger.LogInformation("ValidateOrderStatusFilter: {Message}", context.Message);
        if (context.Message is GetOrderStatus getOrderStatus && getOrderStatus.OrderId == Guid.Empty)
        {
            throw new ArgumentException("The OrderId must not be empty");
        }

        return next.Send(context);
    }
}

public class MyQueryResponse
{
    public string YourParameter { get; set; }
    public IEnumerable<int> Values { get; set; }
}

public class MyQueryRequest : Request<MyQueryResponse>
{
    public string MyParameter { get; set; }
}

// public class MyQueryConsumer : IConsumer<MyQueryRequest>
// {
//     public async Task Consume(ConsumeContext<MyQueryRequest> context)
//     {
//         // DestinationAddress, loopback://localhost/mediator
//         var response = new MyQueryResponse
//             { YourParameter = context.Message.MyParameter, Values = new[] { 27, 42 } };
//         await context.RespondAsync(response);
//     }
// }

// https://github.com/MassTransit/MassTransit/blob/2d481dbed1263ca32273ce800f609d7fb647da83/src/MassTransit.Abstractions/MediatorRequestExtensions.cs#L9
// https://github.com/MassTransit/MassTransit/blob/2d481dbed1263ca32273ce800f609d7fb647da83/src/MassTransit.Abstractions/Mediator/MediatorRequestHandler.cs#L31
public class MyQueryConsumer2 : MediatorRequestHandler<MyQueryRequest, MyQueryResponse>
{
    protected override Task<MyQueryResponse> Handle(MyQueryRequest request, CancellationToken cancellationToken)
    {
        var response = new MyQueryResponse
            { YourParameter = request.MyParameter, Values = new[] { 27, 42 } };
        return Task.FromResult(response);
    }
}
