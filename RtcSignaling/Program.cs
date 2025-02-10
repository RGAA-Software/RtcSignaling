using System.Net;
using RtcSignaling;using RtcSignaling.Controllers;
using RtcSignaling.Password;
using RtcSignaling.Room;
using RtcSignaling.Settings;
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

builder.WebHost.UseUrls("http://0.0.0.0:" + appContext.GetSettings().ListenPort);
builder.Services.AddCors();
// builder.Services.AddCors(options =>
// {
//     options.AddDefaultPolicy(b =>
//     {
//         b.AllowAnyOrigin()
//             .AllowAnyMethod()
//             .AllowAnyHeader();
//     });
// });


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// builder.WebHost.ConfigureKestrel(options =>
// {
//     var settings = appContext.GetSettings();
//     options.Listen(IPAddress.Any, settings.ListenPort, listenOptions =>
//     {
//         listenOptions.UseHttps(settings.Certificate, settings.Password);
//         //listenOptions.UseHttps("./Cert/certificate.pfx", "Dolit@321");
//     });
// });

var app = builder.Build();

app.UseCors(options => options
    .AllowAnyHeader()               // 确保策略允许任何标头
    .AllowAnyMethod()               // 确保策略允许任何方法
    .SetIsOriginAllowed(o => true)  // 设置指定的isOriginAllowed为基础策略
    .AllowCredentials());           // 将策略设置为允许凭据。

// http handler
var httpHandler = new HttpHandler(appContext, app);
var pwdHandler = new PwdHandler(appContext, app);
var userHandler = new UserHandler(appContext, app);
var roomHandler = new RoomHandler(appContext, app);

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
            var remoteIp = context.Connection.RemoteIpAddress?.ToString();
            var handler = new WebSocketHandler(appContext);
            await handler.Handle(websocket, remoteIp);
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