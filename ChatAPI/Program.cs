using ChatAPI.Application;
using ChatAPI.Application.Common.Services;
using ChatAPI.Controlers;
using ChatAPI.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped(sp =>
{
    IHttpContextAccessor httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
    return new UserData(httpContextAccessor.HttpContext?.Connection.RemoteIpAddress!);
});
builder.Services.AddApplication()
    .AddInfrastructure();

WebApplication app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapUserRoutes();

app.Run();