using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using OrderService.BackgrondService;
using OrderService.Context;
using OrderService.IMessageBus;
using OrderService.OrderService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFastEndpoints();

builder.Services.AddDbContext<OrderDbContext>(option =>
    option.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

builder.Services.AddSingleton<IMessageBus, MessageBus>();
builder.Services.AddScoped<IOrderService, OrderService.OrderService.OrderService>();

builder.Services.AddHostedService<ValidationOrderSubmitBackgrondService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.UseFastEndpoints();
app.Run();
