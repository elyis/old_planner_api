using System.Text;
using old_planner_api.src.Infrastructure.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MimeDetective;
using webApiTemplate.src.App.IService;
using webApiTemplate.src.App.Service;
using webApiTemplate.src.Domain.Entities.Config;
using old_planner_api.src.App.IService;
using old_planner_api.src.App.Service;
using old_planner_api.src.Ws.App.IService;
using old_planner_api.src.Ws.App.Service;
using Microsoft.AspNetCore.Authentication.Cookies;
using old_planner_api.src.Domain.Entities.Config;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;

namespace old_planner_api
{

    public class Startup
    {
        private readonly IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var jwtSettings = _config.GetSection("JwtSettings").Get<JwtSettings>() ?? throw new Exception("jwt settings is empty");
            var googleSettings = _config.GetSection("GoogleSettings").Get<GoogleSettings>() ?? throw new Exception("google options is empty");
            var mailruSettings = _config.GetSection("MailSettings").Get<MailRuSettings>() ?? throw new Exception("mailru options is empty");
            var emailServiceSettings = _config.GetSection("EmailServiceSettings").Get<EmailServiceSettings>() ?? throw new Exception();


            var fileInspector = new ContentInspectorBuilder()
            {
                Definitions = MimeDetective.Definitions.Default.All(),
            }
            .Build();


            services.AddControllers(config =>
            {
                config.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();
            })
            .AddNewtonsoftJson()
            .ConfigureApiBehaviorOptions(options =>
            {
                options.SuppressMapClientErrors = true;
            });

            services.AddCors(setup =>
            {
                setup.AddDefaultPolicy(options =>
                {
                    options.AllowAnyHeader();
                    options.AllowAnyOrigin();
                    options.AllowAnyMethod();
                });
            });
            services.AddEndpointsApiExplorer();
            services.AddDbContext<AppDbContext>();

            services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo("/keys"))
                .SetApplicationName("old_planner_api");
            services
                .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    })
                .AddCookie(options =>
                {
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.LoginPath = "/google-login";
                })

                .AddGoogle(options =>
                {
                    options.ClientSecret = googleSettings.ClientSecret;
                    options.ClientId = googleSettings.ClientId;
                    options.Scope.Add("https://mail.google.com");
                    options.SaveTokens = true;
                    options.AccessType = "offline";
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    };
                });

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            services.ConfigureApplicationCookie(options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                options.SlidingExpiration = true;
            });

            services.AddAuthorization();
            services.AddLogging(builder =>
            {
                builder.AddConsole();
            });


            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "old_planner_api",
                    Description = "Api",
                });

                options.EnableAnnotations();
            })
            .AddSwaggerGenNewtonsoftSupport();


            services.AddSingleton<IJwtService, JwtService>();
            services.AddSingleton<IFileUploaderService, LocalFileUploaderService>();
            services.AddSingleton<ITaskChatConnectionService, TaskChatConnectionService>();
            services.AddSingleton<IChatConnectionService, ChatConnectionService>();
            services.AddSingleton<IMainMonitoringService, MainMonitoringService>();
            services.AddSingleton<INotificationService, WsNotificationService>();
            services.AddSingleton<IEmailService, EmailService>();
            services.AddSingleton<IGoogleTokenService, GoogleTokenService>(sp => new GoogleTokenService(googleSettings.ClientId, googleSettings.ClientSecret));
            services.AddSingleton<IMailRuTokenService, MailRuTokenService>(sp => new MailRuTokenService(mailruSettings.ClientId, mailruSettings.ClientSecret, mailruSettings.RedirectUri));

            services.AddSingleton(fileInspector);
            services.AddSingleton(jwtSettings);
            services.AddSingleton(emailServiceSettings);

            services.AddScoped<IAuthService, AuthService>();

            services.Scan(scan =>
            {
                scan.FromCallingAssembly()
                    .AddClasses(classes =>
                        classes.Where(type =>
                            type.Name.EndsWith("Repository")))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime();
            });

            services.Scan(scan =>
            {
                scan.FromCallingAssembly()
                    .AddClasses(classes =>
                        classes.Where(type =>
                            type.Name.EndsWith("Manager")))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime();
            });

            services.Scan(scan =>
            {
                scan.FromCallingAssembly()
                    .AddClasses(classes =>
                        classes.Where(type =>
                            type.Name.EndsWith("Handler")))
                    .AsImplementedInterfaces()
                    .WithScopedLifetime();
            });
        }

        public void ApplyMigrations(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            try
            {
                var context = services.GetRequiredService<AppDbContext>();
                context.Database.Migrate();
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while migrating the database.");
            }
        }

        public void Configure(WebApplication app, IWebHostEnvironment env)
        {
            var webSocketOptions = new WebSocketOptions
            {
                KeepAliveInterval = new TimeSpan(0, 0, 20)
            };

            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors();
            app.UseHttpLogging();
            app.UseRequestLocalization();
            app.UseRouting();
            app.UseWebSockets(webSocketOptions);

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}