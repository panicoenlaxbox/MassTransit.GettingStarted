// Program.cs
using MassTransit;
using MassTransit.GettingStarted;
using MassTransit.Mediator;
using Microsoft.AspNetCore.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// We want to return tuple in the response
builder.Services.Configure<JsonOptions>(options => options.SerializerOptions.IncludeFields = true);

builder.Services.AddMediator(cfg =>
{
    // cfg.AddConsumer<SubmitOrderConsumer>();
    // cfg.AddConsumer<SubmitOrderConsumer2>();
    // cfg.AddConsumer<OrderSubmittedConsumer>();
    // cfg.AddConsumer<OrderSubmittedConsumer2>();
    // cfg.AddConsumer<OrderStatusConsumer>();
    // cfg.AddConsumer<MyQueryConsumer>();
    
    cfg.AddConsumers(typeof(IApiMarker).Assembly);
    
    cfg.ConfigureMediator((context, configure) =>
    {
        configure.UseSendFilter(typeof(ValidateOrderStatusFilter<>), context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/submit-order", async (IMediator mediator) =>
{
    var submitOrder = new SubmitOrder { OrderId = NewId.NextGuid() };
    await mediator.Send(submitOrder);
    
    var orderSubmitted = new OrderSubmitted { OrderId = submitOrder.OrderId };
    await mediator.Publish(orderSubmitted);
    
    var client = mediator.CreateRequestClient<GetOrderStatus>();
    Response<OrderStatus> response = await client.GetResponse<OrderStatus>(new { submitOrder.OrderId });
    
    // Mediator best-way
    MyQueryResponse response2 = await mediator.SendRequest(new MyQueryRequest { MyParameter = "Hello" });
    
    return (response.Message, response2);
});

app.Run();