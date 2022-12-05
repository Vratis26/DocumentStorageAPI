using DocumentStorageAPI.DB;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<DocumentDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("WebApiDatabase")));
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllerRoute(name: "default", pattern: "{controller}/{id?}");

app.Run();