global using FluentResults;
using Bank.Api.Abstractions;
using Bank.Api.Accounts;
using Bank.Api.Cards;
using Bank.Api.Data;
using Bank.Api.Infrastructure;
using Bank.Api.Transactions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddSignalR()
    .AddNewtonsoftJsonProtocol(options =>
    {
        options.PayloadSerializerSettings.TypeNameHandling = TypeNameHandling.All;
    });

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtOptions"));
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
{
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "lab_1",
        ValidAudience = "lab_1",
        ClockSkew = TimeSpan.FromSeconds(0),
        RequireExpirationTime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("some-very-secret-key-very-super-some-some"))
    };
});


builder.Services.AddScoped<AccountRepository>();
builder.Services.AddScoped<CardRepository>();
builder.Services.AddScoped<TransactionRepository>();
builder.Services.AddScoped<JwtTokenGenerator>();

builder.Services.AddSingleton<IUserIdProvider, UserIdProvider>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    //app.UseSwagger();
    //app.UseSwaggerUI();
}

/*app.UseCors(policy => policy
    .AllowAnyOrigin()
    .AllowAnyHeader()
    .AllowAnyMethod());*/


app.UseHttpsRedirection();
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<TransactionsHub>("hubs/transactions");

app.Run();
