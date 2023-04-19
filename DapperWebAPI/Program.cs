using DataAccess.Data;
using DataAccess.Interfases;
using DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Agregar cadena de conexion MySQL
var mySQLConfig = new MySQLConfig(builder.Configuration.GetConnectionString("DbConn"));
//builder.Services.AddSingleton(new MySqlConnection(builder.Configuration.GetConnectionString("DbConn")));
builder.Services.AddSingleton(mySQLConfig);

// Agregar repositorio
builder.Services.AddScoped<IAlumnoRepo, AlumnoRepo>();
builder.Services.AddScoped<IRootRepo, RootRepo>();

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
