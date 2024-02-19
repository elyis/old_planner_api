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
using Microsoft.AspNetCore.Authentication.Google;

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

            services
                .AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                .AddGoogle(options =>
                {
                    options.ClientSecret = "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";
                    options.ClientId = "97012875435-oq304igtldgslsb9kntfp64ba5ej92jo.apps.googleusercontent.com";
                })
                .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
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
            services.AddSingleton<ITaskChatService, TaskChatService>();
            services.AddSingleton<IChatService, ChatService>();
            services.AddSingleton<IMainMonitoringService, MainMonitoringService>();
            services.AddSingleton(fileInspector);
            services.AddSingleton(jwtSettings);

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

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
            app.Run();
        }
    }
}