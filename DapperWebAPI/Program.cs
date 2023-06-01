using DataAccess.Data;
using Hangfire;
using DataAccess.Interfases;
using DataAccess.Repositories;
using Hangfire.MySql;
using DapperWebAPI.services;

// Establecer la zona horaria de Chile
var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time");

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
builder.Services.AddScoped<IServiceSendInformationToSIC, ServiceSendInformationToSIC>();

// Add Hangfire services.
string hangfireConnectionString = builder.Configuration.GetConnectionString("DbConn");
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseStorage(
        new MySqlStorage(
            hangfireConnectionString,
            new MySqlStorageOptions
            {
                QueuePollInterval = TimeSpan.FromSeconds(10),
                JobExpirationCheckInterval = TimeSpan.FromHours(1),
                CountersAggregateInterval = TimeSpan.FromMinutes(5),
                PrepareSchemaIfNecessary = true,
                DashboardJobListLimit = 25000,
                TransactionTimeout = TimeSpan.FromMinutes(1),
                TablesPrefix = "Hangfire",
            }
        )
    ));

// Add the processing server as IHostedService
builder.Services.AddHangfireServer(options => options.WorkerCount = 1);

builder.Services.AddHangfireServer();
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseHangfireDashboard();

// esta funcion envía la informacion al SIC apartir de las 22:02
// en este controlador se agregan las ids de los cursos a enviar
// pero para que el trabajo se cumpla, se deben admitir 2 pasos
// 1) la ip de la localizacion donde se esté viviendo la api, debe estar permitida en la base de datos para hacer consultas
// 2) los cambios deben estar actualizados con la rama /master
RecurringJob.AddOrUpdate<IServiceSendInformationToSIC>("enviar la informacion al SIC", servicio => servicio.sendInformationToSIC(), "01 22 * * *", timeZone);

app.MapControllers();

app.Run();
