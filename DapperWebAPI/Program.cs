using DataAccess.Data;
using Hangfire;
using DataAccess.Interfases;
using DataAccess.Repositories;
using Hangfire.MySql;
using DapperWebAPI.services;

internal class Program
{
    private static void Main(string[] args)
    {
        // Establecer la zona horaria de Chile
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Pacific SA Standard Time");

        var builder = WebApplication.CreateBuilder(args);
        var connectionString = "server=200.73.115.66; port=3306; database=chileniv_mo1; user=chileniv_mo1; password=T.7Z8C2wnRwH3x3VjFx37; maxpoolsize=200;Allow User Variables=true;";
        Console.WriteLine("Cadena de conexión: " + connectionString);

        // Add services to the container.
        builder.Services.AddControllers();

        // Agregamos dependenciasde swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Agregar cadena de conexion MySQL
        var mySQLConfig = new MySQLConfig(connectionString);

        //builder.Services.AddSingleton(new MySqlConnection(builder.Configuration.GetConnectionString("DbConn")));
        builder.Services.AddSingleton(mySQLConfig);

        // Agregar repositorio
        builder.Services.AddScoped<IAlumnoRepo, AlumnoRepo>();
        builder.Services.AddScoped<IRootRepo, RootRepo>();
        builder.Services.AddScoped<IServiceSendInformationToSIC, ServiceSendInformationToSIC>();

        // Add Hangfire services.
        builder.Services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseStorage(
                new MySqlStorage(
                    connectionString,
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

        // usamos todas las dependencias con el builder compilado
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.UseHangfireDashboard();

        // esta funcion env�a la informacion al SIC apartir de las 22:02
        // en este controlador se agregan las ids de los cursos a enviar
        // pero para que el trabajo se cumpla, se deben admitir 2 pasos
        // 1) la ip de la localizacion donde se est� viviendo la api, debe estar permitida en la base de datos para hacer consultas
        // 2) los cambios deben estar actualizados con la rama /master
        RecurringJob.AddOrUpdate<IServiceSendInformationToSIC>("enviar la informacion al SIC", servicio => servicio.sendInformationToSIC(), "01 22 * * *");

        app.MapControllers();

        app.Run();
    }
}