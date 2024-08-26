using System.Net;
using RtcSignaling;using RtcSignaling.Controllers;
using RtcSignaling.Password;
using RtcSignaling.User;
using AppContext = RtcSignaling.AppContext;
using Serilog;

// 初始化日志的共享实例
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .Enrich.FromLogContext()
    .WriteTo.File("logs/app.log", 
        rollingInterval: RollingInterval.Day,
        shared: true
    ).CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// app context
var appContext = new AppContext();
appContext.Init();

// system monitor
var systemMonitor = new SystemMonitor(appContext);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Any, 9999, listenOptions =>
    {
        listenOptions.UseHttps("./Cert/syxmsg.xyz.pfx", "jt182l0laf75v1");
    });
});

var app = builder.Build();

// http handler
var httpHandler = new HttpHandler(appContext, app);
var pwdHandler = new PwdHandler(appContext, app);
var userHandler = new UserHandler(appContext, app);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.UseWebSockets();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/signaling")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            var websocket = await context.WebSockets.AcceptWebSocketAsync();
            var handler = new WebSocketHandler(appContext);
            await handler.Handle(websocket);
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }
    else
    {
        await next();
    }
});

app.Run();