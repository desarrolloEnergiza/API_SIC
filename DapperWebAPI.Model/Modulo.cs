namespace Model;

public class Modulo
{
    public int Id { get; set; }
    public string? codigoModulo { get; set; }
    public int tiempoConectividad { get; set; }
    public int estado { get; set; }
    public int porcentajeAvance { get; set; }
    public string? fechaInicio { get; set; }
    public string? fechaFin { get; set; }
    public int AlumnoId { get; set; }
    public int notaModulo { get; set; }
    public int cantActividadAsincrona
    {
        get { return listaActividades.Count(a => a.codigoActividad != null && !(a.codigoActividad.Contains("sinc") || a.codigoActividad.Contains("Sinc") || a.codigoActividad.Contains("vide") || a.codigoActividad.Contains("Vide"))); }
    }
    public int cantActividadSincrona
    {
        get { return listaActividades.Count - cantActividadAsincrona; }
    }
    public List<Actividad> listaActividades { get; set; } = new List<Actividad>();
}
