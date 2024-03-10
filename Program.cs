using System.Text;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using VinhUni_Educator_API.Configs;
using VinhUni_Educator_API.Context;
using VinhUni_Educator_API.Entities;
using VinhUni_Educator_API.Interfaces;
using VinhUni_Educator_API.Middlewares;
using VinhUni_Educator_API.Services;
using VinhUni_Educator_API.Utils;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Development") ?? throw new InvalidOperationException("Connection string invalid.");
var secretKey = builder.Configuration["JWT:AccessTokenSecret"] ?? throw new InvalidOperationException("Secret key invalid.");
var issuer = builder.Configuration["JWT:ValidIssuer"] ?? throw new InvalidOperationException("Issuer invalid.");
var audience = builder.Configuration["JWT:ValidAudience"] ?? throw new InvalidOperationException("Audience invalid.");
var policyName = "CORSPolicy";
// Add services to configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: policyName,
        policy =>
        {
            policy.WithOrigins("http://localhost:3000", "https://educator.vinhuniversity.local", "https://educator.vinhuniversity.edu.vn")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});
// Add services to connect to database
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseNpgsql(connectionString));
// Add services to connect redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Redis connection string invalid.");
    options.InstanceName = "VinhUniEducator";
});
// Add services to Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => { options.SignIn.RequireConfirmedAccount = false; })
    .AddEntityFrameworkStores<ApplicationDBContext>();
// Add services to JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    // Adding Jwt Bearer
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidAudience = audience,
        ValidIssuer = issuer,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
    options.Events = new JwtBearerEvents
    {
        OnChallenge = async context =>
        {
            // Call this to skip the default logic and avoid using the default response
            context.HandleResponse();
            // Write to the response in any way you wish
            context.Response.StatusCode = 401;
            await context.Response.WriteAsJsonAsync(
                new ActionResponse
                {
                    StatusCode = 401,
                    IsSuccess = false,
                    Message = "You are not authorized, please login to get access"
                }
            );
        }
    };
});
// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers();
builder.Services.AddHttpClient();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(
    options =>
    {
        options.EnableAnnotations();
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "VinhUni Educator API", Version = "v1" });
    }
);

// Add services to lower case url
builder.Services.AddRouting(options => options.LowercaseUrls = true);
// Add services to application IoC
builder.Services.AddScoped<IAuthServices, AuthServices>();
builder.Services.AddScoped<IJwtServices, JwtServices>();
builder.Services.AddScoped<ICacheServices, CacheServices>();

// Add services to configure auto mapper
var mapperConfig = new MapperConfiguration(mc =>
{
    mc.AddProfile(new AutoMapperProfile());
});
IMapper mapper = mapperConfig.CreateMapper();
builder.Services.AddSingleton(mapper);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(config =>
    {
        config.SwaggerEndpoint("v1/swagger.json", "VinhUNI Educator API V1");
    });
}
app.UseCors(policyName);
app.UseHttpsRedirection();
app.UseMiddleware<VerifyRevokedToken>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

