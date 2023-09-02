using RoundRobinApi.Core;
using RoundRobinApi.Domain;
using RoundRobinApi.Interface;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton(builder.Configuration.Get<AddressesConfiguration>());
builder.Services.AddHttpClient(nameof(RoundRobinCore), x => { x.Timeout = TimeSpan.FromMilliseconds(500); });
builder.Services.AddScoped<IRoundRobinCore, RoundRobinCore>();
builder.Services.AddControllers().AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
