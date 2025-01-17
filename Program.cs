using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyProject.Context;
using MyProject.Domain;
using MyProject.Domain.Emails;
using MyProject.Repos;
using MyProject.Services;

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
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();

#region JWT Authorization

var jwtSection = configuration.GetSection("Jwt");
builder.Services.Configure<JwtKeys>(jwtSection);

// var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSecretKeyHere";
// var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "YourIssuerHere";
var jwtKeys = jwtSection.Get<JwtKeys>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtKeys.Issuer,
            ValidAudience = "admin",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKeys.Secret))
        };
    });

builder.Services.AddAuthorization();
#endregion

var app = builder.Build();

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