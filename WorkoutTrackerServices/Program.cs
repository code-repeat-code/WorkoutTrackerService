using System.Reflection;
using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using WorkoutTrackerServices.Entities;
using WorkoutTrackerServices.Repositories;
using WorkoutTrackerServices.Repositories.Interfaces;
using WorkoutTrackerServices.Services;
using WorkoutTrackerServices.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

ConfigureEnvironment();
ConfigureServices(builder);
ConfigureAuthentication(builder);

var app = builder.Build();

ConfigureMiddleware(app);

app.Run();
return;


// --- Configuration Methods ---
void ConfigureEnvironment()
{
    Env.Load();
    if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JWT_KEY")))
    {
        throw new ApplicationException("JWT_KEY is missing");
    }
}

void ConfigureServices(WebApplicationBuilder builder)
{
    string connectionString = Env.GetString("DB_CONNECTION_STRING");
    // Configure DbContext
    builder.Services.AddDbContext<WorkoutContext>(options =>
        options.UseNpgsql(connectionString));
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IWorkoutRepository, WorkoutRepository>();
    builder.Services.AddScoped<IWorkoutService, WorkoutService>();
    builder.Services.AddControllers(options =>
    {
        // Apply [Authorize] globally
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
        options.Filters.Add(new AuthorizeFilter(policy));
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddAutoMapper(typeof(Program));
    ConfigureSwagger(builder);
}

void ConfigureSwagger(WebApplicationBuilder builder)
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "WorkoutTracker API", Version = "v1" });
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
        
        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "JWT Authentication",
            Description = "Enter your JWT token in this field",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        };

        c.AddSecurityDefinition("Bearer", securityScheme);

        var securityRequirement = new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                new string[] {}
            }
        };

        c.AddSecurityRequirement(securityRequirement);
    });
}

void ConfigureAuthentication(WebApplicationBuilder builder)
{
    var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
    var key = Encoding.ASCII.GetBytes(jwtKey);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

    builder.Services.AddAuthorization();
}

void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
}
