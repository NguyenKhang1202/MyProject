using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyProject.Context;
using MyProject.Domain;
using MyProject.Domain.Elasticsearchs;
using MyProject.Domain.Emails;
using MyProject.Domain.Keycloaks;
using MyProject.Domain.OAuths;
using MyProject.Quartz;
using MyProject.Repos;
using MyProject.Services;
using MyProject.SignalR;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services.AddControllers(options =>
{
    // options.Filters.Add<GlobalProducesResponseTypeFilter>();
});
// builder.Services.AddControllers(opt => opt.Filters.Add(new ValidationFilterAttribute()))
//     .AddNewtonsoftJson(opt =>
//     {
//         opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
//         opt.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
//     });
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddEndpointsApiExplorer();

#region SignalR

builder.Services.AddSignalR();

#endregion

builder.Services.AddSwaggerGen(options =>
{
    // options.OperationFilter<AddResponseHeadersFilter>();
    options.AddSecurityDefinition("Bearer",
        new OpenApiSecurityScheme
        {
            In = ParameterLocation.Header,
            Description = "Please insert JWT with Bearer into field",
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });
    
    OpenApiSecurityScheme securityScheme = new OpenApiSecurityScheme()
    {
        Reference = new OpenApiReference()
        {
            Id = "Bearer",
            Type = ReferenceType.SecurityScheme
        },
        Scheme = "Bearer",
        Name = "Authorization",
        In = ParameterLocation.Header
    };
    OpenApiSecurityRequirement securityRequirements = new OpenApiSecurityRequirement()
    {
        {securityScheme, new string[] { }},
    };
    
    options.AddSecurityRequirement(securityRequirements);
    // options.OperationFilter<AddAuthHeaderOperationFilter>();
});
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<MyDbContext>();
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<IVerificationCodeRepo, VerificationCodeRepo>();
builder.Services.AddScoped<IExternalLoginRepo, ExternalLoginRepo>();
builder.Services.AddScoped<IChatRoomRepo, ChatRoomRepo>();
builder.Services.AddScoped<IMessageRepo, MessageRepo>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<IChatRoomService, ChatRoomService>();

#region Quartz

builder.Services.AddQuartzJobs();
builder.Services.AddScoped<VerificationCodeCleanupJob>();

#endregion

#region Elaticsearch

var eConfigurationSection = configuration.GetSection("ElasticsearchSettings");
builder.Services.Configure<ElasticsearchSettings>(eConfigurationSection);
builder.Services.AddScoped<ElasticSearchService>();

#endregion

#region log

// 3. Cấu hình Serilog để ghi logs vào Console, File, và AWS CloudWatch
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console() // Ghi logs ra console
    .WriteTo.File("logs/myapp.log", rollingInterval: RollingInterval.Day) // Ghi logs ra file
    .Enrich.FromLogContext()
    .CreateLogger();

// 4. Thêm Serilog vào Logging của ASP.NET Core
builder.Host.UseSerilog();

#endregion

#region JWT Authorization

var jwtSection = configuration.GetSection("Jwt");
builder.Services.Configure<JwtKeys>(jwtSection);
builder.Services.Configure<KeycloakOptions>(builder.Configuration.GetSection("Authentication:Keycloak"));
var keycloakSection = configuration.GetSection("Authentication:Keycloak");
var keycloakOptions = keycloakSection.Get<KeycloakOptions>();
var jwtKeys = jwtSection.Get<JwtKeys>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    {
        options.Authority = keycloakOptions?.Authority;
        options.RequireHttpsMetadata = keycloakOptions!.RequireHttpsMetadata;
        options.Audience = keycloakOptions.Audience;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            RoleClaimType = ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var identity = context.Principal.Identity as ClaimsIdentity;
                var realmAccessClaim = identity?.FindFirst("realm_access");
                if (realmAccessClaim != null)
                {
                    var realmAccess = System.Text.Json.JsonDocument.Parse(realmAccessClaim.Value);
                    if (realmAccess.RootElement.TryGetProperty("roles", out var roles))
                    {
                        foreach (var role in roles.EnumerateArray())
                        {
                            var roleName = role.GetString();
                            if (!string.IsNullOrEmpty(roleName))
                            {
                                identity?.AddClaim(new Claim(ClaimTypes.Role, roleName));
                            }
                        }
                    }
                }

                var resourceAccessClaim = identity?.FindFirst("resource_access");
                if (resourceAccessClaim != null)
                {
                    var resourceAccess = System.Text.Json.JsonDocument.Parse(resourceAccessClaim.Value);
                    if (resourceAccess.RootElement.TryGetProperty("account", out var accountObj) &&
                        accountObj.TryGetProperty("roles", out var roles))
                    {
                        var resourceRoles = new List<string>();
                        foreach (var role in roles.EnumerateArray())
                        {
                            var roleName = role.GetString();
                            if (!string.IsNullOrEmpty(roleName))
                            {
                                resourceRoles.Add(roleName);
                                identity?.AddClaim(new Claim(ClaimTypes.Role, roleName));
                            }
                        }
                        context.HttpContext.Items["ResourceRoles"] = resourceRoles;
                    }
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Log.Error("Authentication failed: {Message}", context.Exception.Message);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();
#endregion

var app = builder.Build();

app.MapHub<ChatHub>("/chatHub");

// Configure the HTTP request pipeline.
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