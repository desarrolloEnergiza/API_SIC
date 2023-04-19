namespace Model;

public class Modulo
{
    public int Id { get; set; }
    public string? codigoModulo { get; set; }
    public int tiempoConectividad { get; set; }
    public int estado { get; set; }
    public int porcentajeAvance { get; set; }
    public DateTime fechaInicio { get; set; }
    public DateTime fechaFin { get; set; }
    public int AlumnoId { get; set; }
    public List<Actividad> Actividades { get; set; } = new List<Actividad>();
}
