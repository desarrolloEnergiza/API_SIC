namespace Model;

public class Root
{
    public int Id { get; set; }
    public string? rutOtec { get; set; }
    public int idSistema { get; set; }
    public string? token { get; set; }
    public string? codigoOferta { get; set; }
    public string? codigoGrupo { get; set; }
    public string? codigoEnvio { get; set; }
    public int? cantTotalActividad { get; set; }
    public int? cantActividadSincronica { get; set; }
    public int? cantActividadAsincronica { get; set; }
    public List<Alumno> listaAlumnos { get; set; } = new List<Alumno>();
}
