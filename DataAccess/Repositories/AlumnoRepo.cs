using Dapper;
using DataAccess.Data;
using DataAccess.Interfases;
using Model;
using MySql.Data.MySqlClient;
using System.Text;
using Newtonsoft.Json;

namespace DataAccess.Repositories;

public class AlumnoRepo : IAlumnoRepo
{
    // Inyectamos la dependencia de MySQLConfig
    private readonly MySQLConfig _config;
    private readonly IRootRepo rootRepo;
    private List<Alumno> listaAlumnos = new List<Alumno>();

    public AlumnoRepo(MySQLConfig config, IRootRepo rootRepo)
    {
        _config = config;
        this.rootRepo = rootRepo;
    }

    // Abrir la conexión a la base de datos
    protected MySqlConnection ObtenerConexion()
    {
        return new MySqlConnection(_config.CadenaConexion);
    }

    public List<Alumno> GetAlumnos()
    {
        using (var conexion = ObtenerConexion())
        {
            conexion.Open();
            var query = @"SELECT mo_role_assignments.userid AS Id, firstname AS Nombre, lastname AS Apellido, LEFT(mo_user.idnumber, LENGTH(mo_user.idnumber - 2)) AS rutAlumno,
                                RIGHT(mo_user.idnumber, 1) AS dvAlumno, email AS Correo, mo_course.fullname AS NombreCurso, mo_course.idnumber AS codigoOferta,
                                FROM_UNIXTIME(startdate) AS fechaInicio, FROM_UNIXTIME(enddate) AS fechaFin
                                FROM mo_course
                                INNER JOIN mo_context ON mo_context.instanceid = mo_course.id
                                INNER JOIN mo_role_assignments ON mo_context.id = mo_role_assignments.contextid
                                INNER JOIN mo_user ON (mo_role_assignments.userid = mo_user.id)
                                WHERE (LEFT(mo_user.idnumber, LENGTH(mo_user.idnumber - 2)) != '' AND mo_role_assignments.roleid = 5);";

            var resultado = conexion.Query<Alumno>(query);
            foreach (var item in resultado)
            {
                Alumno alumno = new Alumno();
                alumno.Id = item.Id;
                alumno.rutAlumno = item.rutAlumno;
                alumno.dvAlumno = item.dvAlumno;
                alumno.codigoOferta = item.codigoOferta;
                alumno.tiempoConectividad = 0;
                alumno.estado = 0;
                alumno.porcentajeAvance = 0;
                alumno.fechaInicio = item.fechaInicio;
                alumno.fechaFin = item.fechaFin;
                alumno.listaModulos = item.listaModulos;
                listaAlumnos.Add(alumno);
            }
            conexion.Close();
        }
        return listaAlumnos;
    }

    public async Task<Alumno> GetMulti(int id)
    {

        var query = @"SELECT mo_role_assignments.userid AS Id, 
                        firstname AS Nombre, lastname AS Apellido, 
                        LEFT(mo_user.idnumber, LENGTH(mo_user.idnumber - 2)) AS rutAlumno,
                        RIGHT(mo_user.idnumber, 1) AS dvAlumno, email AS Correo, 
                        mo_course.fullname AS NombreCurso, mo_course.idnumber AS codigoOferta,
                        FROM_UNIXTIME(startdate) AS fechaInicio, FROM_UNIXTIME(enddate) AS fechaFin
                    FROM mo_course
                        INNER JOIN mo_context ON mo_context.instanceid = mo_course.id
                        INNER JOIN mo_role_assignments ON mo_context.id = mo_role_assignments.contextid
                        INNER JOIN mo_user ON (mo_role_assignments.userid = mo_user.id)
                    WHERE mo_user.id = @id AND mo_role_assignments.roleid = 5;" +

                    @"SELECT mo_user.idnumber, mo_user.id AS AlumnoId, 
                    CASE 
                        WHEN LOCATE('PRES0', name) > 0 THEN MID(name, LOCATE('PRES0', name), 16) 
                        ELSE MID(name, LOCATE('ELE', name), 15) 
                    END codigoModulo, name AS NombreModulo, 
                        FROM_UNIXTIME(startdate,'%Y-%m-%d') AS fechaInicio, 
                        FROM_UNIXTIME(enddate,'%Y-%m-%d') AS fechaFin 
                    FROM mo_course_sections 
                        INNER JOIN mo_course ON (mo_course_sections.course = mo_course.id) 
                        INNER JOIN mo_context ON (mo_context.instanceid = mo_course.id) 
                        INNER JOIN mo_role_assignments ON (mo_context.id = mo_role_assignments.contextid) 
                        INNER JOIN mo_user ON (mo_role_assignments.userid = mo_user.id) 
                    WHERE mo_user.id = @id 
                        AND (name LIKE '%PRES%' OR name LIKE '%ELE%') 
                        AND mo_role_assignments.roleid = 5";

        using (var conexion = ObtenerConexion())
        using (var multi = await conexion.QueryMultipleAsync(query, new { id }))
        {
            var alumno = await multi.ReadSingleOrDefaultAsync<Alumno>();
            if (alumno is not null)
            {
                alumno.listaModulos = (await multi.ReadAsync<Modulo>()).ToList();
            }
            else
            {
                return new Alumno();
            }
            return alumno;
        }
    }

    public async Task<Alumno> GetById(int id)
    {
        var query = @"SELECT mo_role_assignments.userid AS Id, firstname AS Nombre, lastname AS Apellido, LEFT(mo_user.idnumber, LENGTH(mo_user.idnumber - 2)) AS rutAlumno,
                        RIGHT(mo_user.idnumber, 1) AS dvAlumno, email AS Correo, mo_course.fullname AS NombreCurso, mo_course.idnumber AS codigoOferta,
                        FROM_UNIXTIME(startdate) AS fechaInicio, FROM_UNIXTIME(enddate) AS fechaFin
                    FROM mo_course
                        INNER JOIN mo_context ON mo_context.instanceid = mo_course.id
                        INNER JOIN mo_role_assignments ON mo_context.id = mo_role_assignments.contextid
                        INNER JOIN mo_user ON (mo_role_assignments.userid = mo_user.id)
                    WHERE mo_user.id = @id AND mo_role_assignments.roleid = 5";

        using (var conexion = ObtenerConexion())
        {
            var alumno = await conexion.QuerySingleOrDefaultAsync<Alumno>(query, new { id });
            return alumno;
        }
    }

    public string GetApiResponse(string apiKey)
    {
        using (HttpClient httpClient = new HttpClient())
        {
            // Realiza una solicitud GET a la API externa
            HttpResponseMessage response = httpClient.GetAsync(apiKey).Result;

            // Verifica si la solicitud fue exitosa
            response.EnsureSuccessStatusCode();

            return response.Content.ReadAsStringAsync().Result;
        }
    }


    public async Task<List<string>> PostApiRequest(string apiKey)
    {
        List<string> responses = new List<string>();
        List<int> ids = new List<int>() { 213, 193 };

            using (HttpClient httpClient = new HttpClient())
        {

            foreach (int id in ids)
            {
                try
                {
                    var rootResponse = await rootRepo.GetData(id);
                    var rawResponse = JsonConvert.SerializeObject(rootResponse);

                    Console.WriteLine(rawResponse);

                    var content = new StringContent(rawResponse, Encoding.UTF8, "application/json");
                        
                    HttpResponseMessage response = await httpClient.PostAsync(apiKey, content);
                    response.EnsureSuccessStatusCode();

                    string responseBody = await response.Content.ReadAsStringAsync();
                    responses.Add(responseBody);
                 }
                catch (Exception ex)
                {
                    responses.Add(ex.Message);
                }
            }

            Console.WriteLine(responses[0]);
            Console.WriteLine(responses[1]);
        }

        return responses;
    }


}

