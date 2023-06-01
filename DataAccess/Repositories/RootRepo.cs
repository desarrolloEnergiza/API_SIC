using Dapper;
using DataAccess.Data;
using DataAccess.Interfases;
using Model;
using MySql.Data.MySqlClient;
using System.Data;

namespace DataAccess.Repositories;

public class RootRepo : IRootRepo
{
    // Inyectamos la dependencia de MySQLConfig
    private readonly MySQLConfig _config;
    private List<Root> listaCursos = new List<Root>();

    public RootRepo(MySQLConfig config)
    {
        _config = config;
    }

    // Abrir la conexión a la base de datos
    protected MySqlConnection ObtenerConexion()
    {
        return new MySqlConnection(_config.CadenaConexion);
    }

    public async Task<Root> GetData(int id)
    {
        var query = @"SELECT id AS Id, fullname AS NombreCurso,  
                        idnumber AS codigoOferta,
                        idnumber AS codigoGrupo,
                        idnumber AS codigoEnvio,
                        DATE_FORMAT(FROM_UNIXTIME(startdate), '%Y-%m-%d') AS fechaInicio, 
                        DATE_FORMAT(FROM_UNIXTIME(enddate), '%Y-%m-%d') AS fechaFin
                    FROM mo_course 
                    WHERE mo_course.id = @id
                        AND idnumber != '' AND idnumber IS NOT NULL;" +

                    @"SELECT mo_user.id AS Id, 
                        mo_course.idnumber AS codigoOferta,
                        SUBSTRING_INDEX(mo_user.idnumber, '-', 1) AS rutAlumno,
                        RIGHT(mo_user.idnumber, 1) AS dvAlumno, mo_user.mnethostid AS estado,
                        DATE_FORMAT(FROM_UNIXTIME(startdate), '%Y-%m-%d') AS fechaInicio, 
                        DATE_FORMAT(FROM_UNIXTIME(enddate), '%Y-%m-%d') AS fechaFin,
                        DATE_FORMAT(CURDATE(), '%Y-%m-%d') AS fechaEjecucion,
                        mo_course.id AS RootId
                    FROM mo_course
                        INNER JOIN mo_context ON mo_context.instanceid = mo_course.id
                        INNER JOIN mo_role_assignments ON mo_context.id = mo_role_assignments.contextid
                        INNER JOIN mo_user ON (mo_role_assignments.userid = mo_user.id)
                    WHERE mo_course.id = @id
                        AND mo_role_assignments.roleid = 5;";

        Root curso;
        using (var conexion = ObtenerConexion())
        {
            using (var multi = await conexion.QueryMultipleAsync(query, new { id }))
            {
                curso = await multi.ReadSingleOrDefaultAsync<Root>();
                if (curso is not null)
                {
                    // Agregar rut otec
                    curso.rutOtec = "76697561-5";
                    // Agregar id sistema
                    curso.idSistema = 1350;
                    // Agregar TOKEN
                    curso.token = "41121748-F370-403D-8F03-2F9F7E5C40D2";
                    curso.listaAlumnos = (await multi.ReadAsync<Alumno>()).ToList();
                } else
                {
                    return curso;
                }
            }
        
                var modulosQuery = @"SELECT cs.id AS Id, u.idnumber, u.id AS AlumnoId,
                                        SUBSTRING_INDEX(name, '/', -1) AS codigoModulo,
                                        name as NombreModulo,
                                        DATE_FORMAT(FROM_UNIXTIME(startdate), '%Y-%m-%d') AS fechaInicio, 
                                        DATE_FORMAT(FROM_UNIXTIME(enddate), '%Y-%m-%d') AS fechaFin
                                    FROM mo_course_sections AS cs
                                        JOIN mo_course AS c ON cs.course = c.id
                                        JOIN mo_context ctx ON ctx.instanceid = c.id
                                        JOIN mo_role_assignments AS ra ON ctx.id = ra.contextid
                                        JOIN mo_user AS u ON ra.userid = u.id
                                    WHERE c.id = @id
                                        AND (name LIKE '%/%')
                                        AND ra.roleid = 5;";


            var modulos = await conexion.QueryAsync<Modulo>(modulosQuery, new { id });

            // Agregar módulos a los alumnos
            foreach (var alumno in curso.listaAlumnos)
            {
                alumno.listaModulos = modulos.Where(m => m.AlumnoId == alumno.Id).ToList();

            }

            var actividadesQuery = @"SELECT gi.id AS Id, itemname AS codigoActividad, cs.id AS ModuloId
                                FROM mo_grade_items AS gi
	                                JOIN mo_course_modules AS cm ON gi.iteminstance = cm.instance
	                                JOIN mo_course_sections AS cs ON cm.section = cs.id
	                                JOIN mo_course AS c ON cs.course = c.id
                                WHERE gi.courseid = @id
                                    AND itemname IS NOT NULL
                                    AND itemname != ''
                                GROUP BY gi.id, itemname, cs.id;";

            var actividades = await conexion.QueryAsync<Actividad>(actividadesQuery, new { id });
            //contar actividades
            var actividadesTotalQuery = @"SELECT count(id) FROM mo_course_sections where course = @id;";
            conexion.Open();

            using (MySqlCommand command = new MySqlCommand(actividadesTotalQuery, conexion))
            {

                command.Parameters.AddWithValue("@id", id);
                int actividadesTotales = Convert.ToInt32(command.ExecuteScalar());
                curso.cantTotalActividad = actividadesTotales;
                conexion.Close();
            }

            // Agregar actividades a los módulos
            foreach (var modulo in curso.listaAlumnos.SelectMany(alumno => alumno.listaModulos))
            {
                modulo.listaActividades = actividades.Where(a => a.ModuloId == modulo.Id).ToList();
            }

            //agregar actividades sincronicas
            var actividadesSincronicasQuery = @"SELECT count(id) FROM mo_course_sections where course = @id and (name like '%video%'or name  like '%sinc%');";


            using (MySqlCommand command = new MySqlCommand(actividadesSincronicasQuery, conexion))
            {
                conexion.Open();
                command.Parameters.AddWithValue("@id", id);
                int actividadesSincronicas = Convert.ToInt32(command.ExecuteScalar());
                curso.cantActividadSincronica = actividadesSincronicas;
                conexion.Close();
            }
            //agregar actividades asincronicas
            var actividadesAsincronicasQuery = @"SELECT count(id) FROM mo_course_sections where course = @id and (name not like '%video%'and name not like '%sinc%')";



            using (MySqlCommand command = new MySqlCommand(actividadesAsincronicasQuery, conexion))
            { 
                conexion.Open();
                command.Parameters.AddWithValue("@id", id);
                int actividadesAsincronicas = Convert.ToInt32(command.ExecuteScalar());
                curso.cantActividadAsincronica = actividadesAsincronicas;
                conexion.Close();
            }

            //obtener tiempos de conexion
            foreach (var alumno in curso.listaAlumnos)
            {
                var idAlumno = alumno.Id;
                alumno.tiempoConectividad = calcularTiempoConexion(id, idAlumno);
            }

        }

        long calcularTiempoConexion(int idCurso, int idAlumno)
        {
            var conexion = ObtenerConexion();
            var tiempoMinimoQuery = @"SELECT min(timecreated) 
                            FROM mo_logstore_standard_log l
                            WHERE l.userid = @idAlumno and l.courseid = @idCurso LIMIT 1";

            var tiempoMaximoQuery = @"SELECT max(timecreated) 
                            FROM mo_logstore_standard_log l
                            WHERE l.userid = @idAlumno and l.courseid = @idCurso";
            conexion.Open();
            using (MySqlCommand tiempoMinimoCmd = new MySqlCommand(tiempoMinimoQuery, conexion))
            using (MySqlCommand tiempoMaximoCmd = new MySqlCommand(tiempoMaximoQuery, conexion))
            {
                tiempoMinimoCmd.Parameters.AddWithValue("@idCurso", idCurso);
                tiempoMinimoCmd.Parameters.AddWithValue("@idAlumno", idAlumno);

                tiempoMaximoCmd.Parameters.AddWithValue("@idCurso", idCurso);
                tiempoMaximoCmd.Parameters.AddWithValue("@idAlumno", idAlumno);

                object tiempoMinimoObj = tiempoMinimoCmd.ExecuteScalar();
                object tiempoMaximoObj = tiempoMaximoCmd.ExecuteScalar();

                long tiempoMinimo = Convert.IsDBNull(tiempoMinimoObj) ? 0 : Convert.ToInt64(tiempoMinimoObj);
                long tiempoMaximo = Convert.IsDBNull(tiempoMaximoObj) ? 0 : Convert.ToInt64(tiempoMaximoObj);

                if (tiempoMinimo == 0 || tiempoMaximo == 0)
                {
                    conexion.Close();
                    return 0; // Valor fijo cuando hay un valor nulo o vacío
                }

                long tiempoConexion = tiempoMaximo - tiempoMinimo;

                conexion.Close();
                return tiempoConexion;
            }
        }
        return curso;

    }

}