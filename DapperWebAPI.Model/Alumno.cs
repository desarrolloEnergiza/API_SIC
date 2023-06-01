namespace Model;

public class Alumno
{
    public int Id { get; set; }
    public string? codigoOferta { get; set; }
    public string? rutAlumno { get; set; }
    public string? dvAlumno { get; set; }
    public long tiempoConectividad { get; set; }
    public int estado { get; set; }
    public int porcentajeAvance { get; set; }
    public string? fechaInicio { get; set; }
    public string? fechaFin { get; set; }
    public string? fechaEjecucion { get; set; }
    public int evaluacionFinal { get; set; }

    public int RootId { get; set; }
    public List<Modulo> listaModulos { get; set; } = new List<Modulo>();

 
}
