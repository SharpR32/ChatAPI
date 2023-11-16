using ChatAPI.Application;
using ChatAPI.Application.Common.Services;
using ChatAPI.Controlers.Common;
using ChatAPI.Infrastructure;
using ChatAPI.Middlewares;
using ChatAPI.Policies.RateLimitting;
using ChatAPI.SignalRHubs;
using System.Security.Claims;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen()
    .AddEndpointsApiExplorer()
    .AddRateLimiter(opt =>
    {
        opt.AddSendMessagePolicy(builder.Configuration);
    })
    .AddHttpContextAccessor()
    .AddScoped(sp =>
    {
        IHttpContextAccessor httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
        return new UserData(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress!);
    })
    .AddWebEncoders()
    .AddScoped<TokenAuthenticator>()
    .AddAuthorization(opt =>
    {
        const string USER_INITIALISED_POLICY = "UserInitialised";
        opt.AddPolicy(USER_INITIALISED_POLICY, policy =>
        {
            policy.RequireClaim(ClaimTypes.Authentication, true.ToString());
        });

        opt.DefaultPolicy = opt.GetPolicy(USER_INITIALISED_POLICY)!;
    })
    .AddAuthenticationCore(opt =>
    {
        opt.AddScheme("Bearer", opt =>
        {
            opt.HandlerType = typeof(TokenAuthenticator);
        });
    })
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

builder.Services
    .AddSignalR();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapSimpleControllersFromAssembly();
app.MapHub<NotificationHub>("/api/notifications");

app.Run();