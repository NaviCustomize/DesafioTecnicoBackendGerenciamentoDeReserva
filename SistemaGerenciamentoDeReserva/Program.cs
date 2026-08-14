using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using SistemaGerenciamentoDeReserva.Application.Interface;
using SistemaGerenciamentoDeReserva.Application.Services;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;
using SistemaGerenciamentoDeReserva.Infrastruture.Repositories;
using System.Data;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IDbConnection>(sp =>
    new NpgsqlConnection(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>(); //faz injecao de dependencia
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IQuartoRepository, QuartoRepository>();
builder.Services.AddScoped<IReservaRepository, ReservaRepository>();

builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<ISenhaHasher, SenhaHasher>();

builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IHotelService, HotelService>();
builder.Services.AddScoped<IQuartoService, QuartoService>();
builder.Services.AddScoped<IReservaService, ReservaService>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");

var secretKey = jwtSettings["Secret"];

if (string.IsNullOrWhiteSpace(secretKey))
{
    throw new InvalidOperationException(
        "JwtSettings:Secret não foi configurado.");
}

var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = issuer,
        ValidAudience = audience,

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(secretKey)
        )
    };

    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            context.HandleResponse();

            context.Response.StatusCode =
                StatusCodes.Status401Unauthorized;

            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                status = 401,
                mensagem =
                    "Você precisa estar autenticado para acessar este recurso."
            });
        },

        OnForbidden = async context =>
        {
            context.Response.StatusCode =
                StatusCodes.Status403Forbidden;

            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new
            {
                status = 403,
                mensagem =
                    "Você não possui permissão para acessar este recurso."
            });
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();