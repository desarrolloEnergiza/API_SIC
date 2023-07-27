namespace Model;
using Newtonsoft.Json;

public class Alumno
{
    [JsonIgnore]
    public int Id { get; set; }
    [JsonIgnore]
    public string? codigoOferta { get; set; }
    [JsonIgnore]
    public int RootId { get; set; }
    public int? rutAlumno { get; set; }
    public string? dvAlumno { get; set; }
    public long tiempoConectividad { get; set; }
    public int evaluacionFinal { get; set; }
    public int estado { get; set; } = 2;
    public int porcentajeAvance { get; set; } = 100;
    public string? fechaInicio { get; set; }
    public string? fechaFin { get; set; }
    public string? fechaEjecucion { get; set; }
    public List<Modulo> listaModulos { get; set; } = new List<Modulo>();

 
}
