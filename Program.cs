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
using MyProject.Domain.OAuths;
using MyProject.Quartz;
using MyProject.Repos;
using MyProject.Services;
using MyProject.SignalR;

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

#region JWT Authorization

var jwtSection = configuration.GetSection("Jwt");
builder.Services.Configure<JwtKeys>(jwtSection);

// var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSecretKeyHere";
// var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "YourIssuerHere";
var jwtKeys = jwtSection.Get<JwtKeys>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = "DynamicScheme"; // Scheme động
        options.DefaultChallengeScheme = "GitHub";
        options.DefaultSignInScheme = "Cookies"; 
    })
    .AddPolicyScheme("DynamicScheme", "Dynamic Auth", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            // Kiểm tra header Authorization trước
            if (context.Request.Headers.ContainsKey("Authorization"))
                return JwtBearerDefaults.AuthenticationScheme;
            // Nếu không, fallback về Cookies
            return "Cookies";
        };
    })
    .AddCookie("Cookies")
    .AddOAuth("GitHub", options =>
    {
        // Load settings from appsettings.json
        var oAuthSettings = builder.Configuration.GetSection("OAuthSettings");
        options.ClientId = oAuthSettings["ClientId"];
        options.ClientSecret = oAuthSettings["ClientSecret"];
        options.CallbackPath = oAuthSettings["CallbackPath"];

        // Define endpoints
        options.AuthorizationEndpoint = oAuthSettings["AuthorizationEndpoint"];
        options.TokenEndpoint = oAuthSettings["TokenEndpoint"];
        options.UserInformationEndpoint = oAuthSettings["UserInfoEndpoint"];

        // Save tokens for authenticated sessions
        options.SaveTokens = true;
        
        options.Scope.Add("read:user");  // Lấy thông tin cơ bản (bio, company, location, etc.)
        options.Scope.Add("user:email");
        
        // Map user claims
        options.ClaimActions.MapJsonKey("name", "name");
        options.ClaimActions.MapJsonKey("email", "email");
        options.ClaimActions.MapJsonKey("id", "id");
        options.ClaimActions.MapJsonKey("login", "login");

        options.Events = new OAuthEvents
        {
            OnCreatingTicket = async context =>
            {
                var request = new HttpRequestMessage(HttpMethod.Get, context.Options.UserInformationEndpoint);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
                var response = await context.Backchannel.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                    throw new HttpRequestException($"Failed to retrieve user information ({response.StatusCode}).");

                var user = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                context.RunClaimActions(user.RootElement);
                
                // email
                var requestEmail = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user/emails");
                requestEmail.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", context.AccessToken);
                var responseEmail = await context.Backchannel.SendAsync(requestEmail);
                if (!responseEmail.IsSuccessStatusCode)
                {
                    throw new HttpRequestException($"Failed to retrieve emails ({response.StatusCode}): {await response.Content.ReadAsStringAsync()}");
                }

                var emailResponse = await responseEmail.Content.ReadAsStringAsync();
                var emails = System.Text.Json.JsonSerializer.Deserialize<List<EmailGithub>>(emailResponse);

                var primaryEmail = emails?.FirstOrDefault(e => e.Primary)?.Email;

                if (!string.IsNullOrEmpty(primaryEmail))
                {
                    context.Identity?.AddClaim(new Claim("email", primaryEmail));
                }
            }
        };
    })
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