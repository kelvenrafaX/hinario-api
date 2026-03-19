using Hinario.Models;
using Microsoft.EntityFrameworkCore;
using Hinario.Context;
using Hinario.Interfaces;
using Hinario.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFront",
        policy =>
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
});

builder.Services.AddControllers();

builder.Services.AddDbContext<HinarioApiContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"), o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));

builder.Services.AddScoped<IHinoRepository, HinoRepository>();

builder.Services.AddHttpClient<ICampinaGrandeMineracaoService, CampinaGrandeMineracaoService>();

builder.Services.AddOpenApi();

var app = builder.Build();

app.UseCors("AllowFront");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
