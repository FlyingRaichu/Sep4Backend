using System.Net;
using System.Text;
using System.Threading.Tasks;
using Application.Logic;
using Application.LogicInterfaces;
using Application.ServiceInterfaces;
using Application.Services;
using Auth;
using Auth.ServiceInterfaces;
using Auth.Services;
using DatabaseInterfacing;
using DatabaseInterfacing.Context;
using DatabaseInterfacing.Domain.EntityFramework;
using IoTInterfacing.Implementations;
using IoTInterfacing.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IConnectionController, ConnectionController>();
builder.Services.AddScoped<IPlantDataLogic, PlantDataLogic>();
builder.Services.AddScoped<IUserAuthService, UserAuthService>();
builder.Services.AddScoped<ITemplateLogic, TemplateLogic>();
builder.Services.AddScoped<IThresholdConfigurationService, ThresholdConfigurationService>();
builder.Services.AddScoped<IAlertNotificationLogic, AlertNotificationLogic>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAlertNotificationService, AlertNotificationService>();

ConfigureEmailService(builder.Services, builder.Configuration);

builder.Services.AddDbContext<PlantDbContext>(options => options.UseNpgsql(DatabaseUtils.GetConnectionString(),
    b => b.MigrationsAssembly("WebAPI")));

builder.WebHost.UseKestrel(options =>
{
    options.Listen(IPAddress.Any, 5021);
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

AuthorizationPolicies.AddPolicies(builder.Services);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials());

app.UseHttpsRedirection();

app.UseAuthorization();

// Get the ConnectionController instance
var connectionController = app.Services.GetRequiredService<IConnectionController>();

// Start the background task to establish the connection
_ = Task.Run(async () => await connectionController.EstablishConnectionAsync(23));

// Run the application
app.MapControllers();
app.Run();

void ConfigureEmailService(IServiceCollection services, IConfiguration configuration)
{
    var smtpConfig = configuration.GetSection("Smtp");
    var smtpServer = smtpConfig["Server"];
    var smtpPort = smtpConfig["Port"];
    var smtpUsername = smtpConfig["Username"];
    var smtpPassword = smtpConfig["Password"];

    if (string.IsNullOrEmpty(smtpServer) || string.IsNullOrEmpty(smtpPort) ||
        string.IsNullOrEmpty(smtpUsername) || string.IsNullOrEmpty(smtpPassword))
    {
        throw new InvalidOperationException("SMTP configuration is missing or invalid.");
    }

   /* if (!int.TryParse(smtpPort, out int port))
    {
        throw new InvalidOperationException("SMTP port is invalid.");
    }*/

   // services.PostConfigure<EmailService>(emailService => emailService.Configure(smtpServer, port, smtpUsername, smtpPassword));
}