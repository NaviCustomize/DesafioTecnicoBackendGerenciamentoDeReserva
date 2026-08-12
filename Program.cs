using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SistemaGerenciamentoDeReserva.Application.Services;
using SistemaGerenciamentoDeReserva.Domain.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ==========================================
// 1. REGISTRAR OS SEUS SERVIÇOS E REPOSITÓRIOS (DDD)
// ==========================================
builder.Services.AddScoped<AuthService>();
//builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();

// ==========================================
// 2. CONFIGURAR O JWT (Autenticação)
// ==========================================
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"];

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey!))
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ==========================================
// 3. ORDEM DOS MIDDLEWARES DE SEGURANÇA
// ==========================================
app.UseAuthentication(); // <-- IMPORTANTE: Tem que vir ANTES do Authorization
app.UseAuthorization();

app.MapControllers();

app.Run();
