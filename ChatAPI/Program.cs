using ChatAPI.Application;
using ChatAPI.Application.Common.Services;
using ChatAPI.Controlers.Common;
using ChatAPI.Infrastructure;
using ChatAPI.Infrastructure.Services.CurrentUser;
using ChatAPI.Middlewares;
using ChatAPI.Policies.RateLimitting;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen()
    .AddEndpointsApiExplorer()
    .AddRateLimiter(opt =>
    {
        opt.AddSendMessagePolicy(builder.Configuration);
    });
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped(sp =>
{
    IHttpContextAccessor httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    return new UserData(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress!);
});
builder.Services.AddScoped<TokenRequirementHandler>();
builder.Services.AddAuthorizationCore(opt =>
{
    const string USER_INITIALISED_POLICY = "UserInitialised";
    opt.AddPolicy(USER_INITIALISED_POLICY, policy =>
    {
        policy.AddRequirements(new TokenRequirement());
    });

    opt.DefaultPolicy = opt.GetPolicy(USER_INITIALISED_POLICY)!;
});
builder.Services.AddApplication()
    .AddInfrastructure(builder.Configuration);

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapCurrentUser();
app.MapSimpleControllersFromAssembly();

app.Run();