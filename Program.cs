using old_planner_api;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("https://*:8888");
builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(8888, listenOptions =>
    {
        listenOptions.UseHttps("/https/local.pfx", "*Scores");
    });
});

var startUp = new Startup(builder.Configuration);
startUp.ConfigureServices(builder.Services);

var app = builder.Build();

startUp.ApplyMigrations(app);
startUp.Configure(app, builder.Environment);
