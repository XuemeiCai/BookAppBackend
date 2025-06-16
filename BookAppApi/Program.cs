using BookAppApi.Models;
using BookAppApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Text;
using DotNetEnv;


var builder = WebApplication.CreateBuilder(args);
Env.Load();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200","https://localhost:4200","https://bookappfrontend.netlify.app")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var jwtConfig = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtConfig["SecretKey"];
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddSingleton<TokenStoreService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = TimeSpan.Zero,
            ValidIssuer = jwtConfig["Issuer"],
            ValidAudience = jwtConfig["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!))
        };
    });



builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

var mongoConnectionString = Environment.GetEnvironmentVariable("MONGODB_URI");
if (string.IsNullOrEmpty(mongoConnectionString))
{
    throw new Exception("Missing MongoDB connection string in environment variable 'MONGODB_URI'");
}
builder.Services.AddSingleton<IMongoClient>(s =>
    new MongoClient(mongoConnectionString));

builder.Services.AddSingleton<BookListService>();
builder.Services.AddSingleton<QuoteService>();
builder.Services.AddSingleton<UserService>();


var app = builder.Build();
app.UseCors("AllowFrontend");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();
app.Urls.Add($"http://*:{Environment.GetEnvironmentVariable("PORT") ?? "5000"}");

app.UseAuthentication();  
app.UseAuthorization();
app.MapControllers(); 
app.Run();

