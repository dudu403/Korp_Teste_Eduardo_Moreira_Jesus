using FaturamentoService.Application;
using FaturamentoService.Application.Middleware;
using FaturamentoService.Clients;
using FaturamentoService.Data;
using FaturamentoService.Domain.Entities;
using FaturamentoService.Domain.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddDbContext<FaturamentoDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IValidator<NotaFiscal>, NotaFiscalValidator>();

builder.Services.AddHttpClient<EstoqueClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7093/");
});

builder.Services.AddScoped<NotaFiscalService>();

builder.Services.AddFluentValidationAutoValidation(config =>
{
    config.DisableDataAnnotationsValidation = true;
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();