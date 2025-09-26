using Core.BackgroundJobs;
using Core.BackgroundJobs.Services;
using Product.Domain.Events;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCoreBackgroundJobs(builder.Configuration,typeof(ProductCreatedEvent).Assembly);

var app = builder.Build();

app.Services
    .GetRequiredService<BackgroundJobProcessor>()
    .AutoSubscribeAll();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();