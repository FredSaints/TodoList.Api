using FluentValidation;
using Serilog;
using TodoList.Api.Data.Implementations;
using TodoList.Api.Data.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

// Add services
builder.Services.AddControllers();

// Register FluentValidation validators
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Register Database and Repository
builder.Services.AddScoped<IDatabase, Database>();
builder.Services.AddScoped<ITaskRepository, TaskRepository>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Use Serilog request logging
app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();