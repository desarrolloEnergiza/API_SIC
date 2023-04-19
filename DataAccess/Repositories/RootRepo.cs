using Dapper;
using DataAccess.Data;
using DataAccess.Interfases;
using Model;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.SqlClient;

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
                        MID(fullname, INSTR(fullname, 'RLAB'), 20) AS codigoOferta,
                        MID(fullname, INSTR(fullname, 'RLAB'), 20) AS codigoGrupo,
                        MID(fullname, INSTR(fullname, 'RLAB'), 20) AS codigoEnvio,
                        FROM_UNIXTIME(startdate,'%Y-%m-%d') AS fechaInicio, 
                        FROM_UNIXTIME(enddate,'%Y-%m-%d') AS fechaFin
                    FROM mo_course 
                    WHERE mo_course.id = @id
                        AND MID(fullname, INSTR(fullname, 'RLAB'), 20) != '';" +

                    @"SELECT mo_user.id AS Id, 
                        MID(fullname, INSTR(fullname, 'RLAB'), 20) AS codigoOferta,
                        CAST(LEFT(mo_user.idnumber, LENGTH(mo_user.idnumber) - 2) AS UNSIGNED) AS rutAlumno,
                        RIGHT(mo_user.idnumber, 1) AS dvAlumno, mo_user.mnethostid AS estado,
                        FROM_UNIXTIME(startdate,'%Y-%m-%d') AS fechaInicio, 
                        FROM_UNIXTIME(enddate,'%Y-%m-%d') AS fechaFin,
                        CURDATE() AS fechaEjecucion,
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
                    curso.Alumnos = (await multi.ReadAsync<Alumno>()).ToList();
                }
            }
        
                var modulosQuery = @"SELECT cs.id AS Id, u.idnumber, u.id AS AlumnoId, 
                                    CASE
  	                                    WHEN LOCATE('PRES0', name) > 0 
                                        THEN MID(name, LOCATE('PRES0', name), 16)
                                        ELSE MID(name, LOCATE('ELE', name), 15)
                                    END codigoModulo, 
                                        name AS NombreModulo,
                                        FROM_UNIXTIME(startdate,'%Y-%m-%d') AS fechaInicio, 
                                        FROM_UNIXTIME(enddate,'%Y-%m-%d') AS fechaFin
                                    FROM mo_course_sections AS cs
                                        JOIN mo_course AS c ON cs.course = c.id
                                        JOIN mo_context ctx ON ctx.instanceid = c.id
                                        JOIN mo_role_assignments AS ra ON ctx.id = ra.contextid
                                        JOIN mo_user AS u ON ra.userid = u.id
                                    WHERE c.id = @id
                                        AND (name LIKE '%PRES%' OR name LIKE '%ELE%')
                                        AND ra.roleid = 5;";


                var modulos = await conexion.QueryAsync<Modulo>(modulosQuery, new { id });

            // Agregar módulos a los alumnos
            foreach (var alumno in curso.Alumnos)
            {
                alumno.Modulos = modulos.Where(m => m.AlumnoId == alumno.Id).ToList();

            }

            var actividadesQuery = @"SELECT gi.id AS Id, itemname AS codigoActividad, cs.id AS ModuloId
                                FROM mo_grade_items AS gi
	                                JOIN mo_course_modules AS cm ON gi.iteminstance = cm.instance
	                                JOIN mo_course_sections AS cs ON cm.section = cs.id
	                                JOIN mo_course AS c ON cs.course = c.id
                                WHERE gi.courseid = @id
                                GROUP BY gi.id, itemname, cs.id";

            var actividades = await conexion.QueryAsync<Actividad>(actividadesQuery, new { id });
            //contar actividades
            var actividadesTotalQuery = @"SELECT count(id) FROM mo_course_sections where course = @id";
            conexion.Open();

            using (MySqlCommand command = new MySqlCommand(actividadesTotalQuery, conexion))
            {

                command.Parameters.AddWithValue("@id", id);
                int actividadesTotales = Convert.ToInt32(command.ExecuteScalar());
                curso.cantTotalActividad = actividadesTotales;
                conexion.Close();
            }

            // Agregar actividades a los módulos
            foreach (var modulo in curso.Alumnos.SelectMany(alumno => alumno.Modulos))
            {
                modulo.Actividades = actividades.Where(a => a.ModuloId == modulo.Id).ToList();
            }
            //agregar actividades sincronicas
            var actividadesSincronicasQuery = @"SELECT count(id) FROM mo_course_sections where course = @id and (name like '%video%'or name  like '%sinc%')";


            using (MySqlCommand command = new MySqlCommand(actividadesSincronicasQuery, conexion))
            {
                conexion.Open();
                command.Parameters.AddWithValue("@id", id);
                int actividadesSincronicas = Convert.ToInt32(command.ExecuteScalar());
                curso.cantActividadSincronica = actividadesSincronicas;
            }
            //agregar actividades asincronicas
            var actividadesAsincronicasQuery = @"SELECT count(id) FROM mo_course_sections where course = @id and (name not like '%video%'and name not like '%sinc%')";

            

            using (MySqlCommand command = new MySqlCommand(actividadesAsincronicasQuery, conexion))
            {
                command.Parameters.AddWithValue("@id", id);
                int actividadesAsincronicas = Convert.ToInt32(command.ExecuteScalar());
                curso.cantActividadAsincronica = actividadesAsincronicas;
            }
            //obtener tiempos de conexion


            foreach (var alumno in curso.Alumnos)
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


                long tiempoMinimo = Convert.ToInt64(tiempoMinimoCmd.ExecuteScalar());
                long tiempoMaximo = Convert.ToInt64(tiempoMaximoCmd.ExecuteScalar());

                long tiempoConexion = tiempoMaximo - tiempoMinimo;

                return tiempoConexion; 
            }

        }
        return curso;

    }

}