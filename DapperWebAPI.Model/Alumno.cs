namespace Model;

public class Alumno
{
    public int Id { get; set; }
    public string? codigoOferta { get; set; }
    public int rutAlumno { get; set; }
    public string? dvAlumno { get; set; }
    public long tiempoConectividad { get; set; }
    public int estado { get; set; }
    public int porcentajeAvance { get; set; }
    public DateTime fechaInicio { get; set; }
    public DateTime fechaFin { get; set; }
    public DateTime fechaEjecucion { get; set; }

    public int RootId { get; set; }
    public List<Modulo> Modulos { get; set; } = new List<Modulo>();

 
}
