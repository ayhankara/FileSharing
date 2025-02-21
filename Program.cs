using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SecureFileStorage;
using SecureFileStorage.Services;
using SecureFileStorage.Services.Interfaces;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<AzureBlobStorageService>();
builder.Services.AddScoped<LocalFileStorageService>();
builder.Services.AddScoped<FileStorageServiceResolver>();
builder.Services.AddScoped<IFileStorageService>(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    var storageType = configuration.GetSection("StorageConfiguration:StorageType").Value;
    var resolver = provider.GetRequiredService<FileStorageServiceResolver>();
    return resolver.GetStorageService(storageType);
});

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["Secret"]);

// Kimlik Doðrulama (Authentication)
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
        ValidAudience = jwtSettings["Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };
});

// Yetkilendirme (Authorization)
builder.Services.AddAuthorization();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication(); // Kimlik doðrulama middleware'ini ekleyin
app.UseAuthorization(); // Yetkilendirme middleware'ini ekleyin

app.MapControllers();

app.Run();

 