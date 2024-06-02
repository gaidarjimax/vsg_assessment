
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<CryptoInfoContext>(options => options.UseSqlite("Data Source=../prices.db"));
builder.Services.AddControllers().AddXmlSerializerFormatters();
builder.Services.AddMemoryCache();

var app = builder.Build(); 

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();
app.Run();

