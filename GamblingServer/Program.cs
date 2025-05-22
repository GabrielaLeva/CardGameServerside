using GamblingServer;
using GamblingServer.DB;
using GamblingServer.Games;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text;
using Microsoft.AspNetCore.Identity;
using System.Net.WebSockets;
var builder = WebApplication.CreateBuilder(args);
var connectionstring= builder.Configuration.GetConnectionString("AuthDB")
        ?? throw new InvalidOperationException("Connection string"
        + "'DefaultConnection' not found.");
builder.Services.AddDbContext<GamblingContext>(options =>
    options.UseMySql(connectionstring,ServerVersion.AutoDetect(connectionstring)));
// Add services to the container.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(opt =>
{
    opt.ExpireTimeSpan = TimeSpan.FromDays(15);
    opt.SlidingExpiration = true;
});
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
InstanceManager.GetInstance();
var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWebSockets();

app.UseAuthorization();
app.UseAuthentication();
app.MapControllers();
app.MapGet("/wj/fetch", async (HttpContext context) => {
    if (context.WebSockets.IsWebSocketRequest)
    {

        var uname = context.User.Identity.Name;
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var buffer = new byte[1024];
        if (uname == null)
        {
            await webSocket.ReceiveAsync(buffer, CancellationToken.None);
            uname=Encoding.UTF8.GetString(buffer);
        }

        TaskCompletionSource<string> game_completion = new TaskCompletionSource<string>();
        var lobby = InstanceManager.GetLobby(GameType.Whitejack, uname, webSocket);
        await game_completion.Task;
    }
    else
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
    }
});
app.Run();
