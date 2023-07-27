namespace Model;
using Newtonsoft.Json;

public class Modulo
{
    [JsonIgnore]
    public int Id { get; set; }
    [JsonIgnore]
    public int AlumnoId { get; set; }
    public string? codigoModulo { get; set; }
    public int tiempoConectividad { get; set; }
    public int porcentajeAvance { get; set; } = 100;
    public int estado { get; set; } = 1;
    public string? fechaInicio { get; set; }
    public List<Actividad> listaActividades { get; set; } = new List<Actividad>();
    public string? fechaFin { get; set; }
    public int notaModulo { get; set; }
    public int cantActividadAsincronica
    {
        get { return listaActividades.Count(a => a.codigoActividad != null && !(a.codigoActividad.Contains("sinc") || a.codigoActividad.Contains("Sinc") || a.codigoActividad.Contains("vide") || a.codigoActividad.Contains("Vide"))); }
    }
    public int cantActividadSincronica
    {
        get { return listaActividades.Count - cantActividadAsincronica; }
    }
}
