namespace Model;
using Newtonsoft.Json;

public class Actividad
{
    [JsonIgnore]
    public int Id { get; set; }
    public string? codigoActividad { get; set; }
    [JsonIgnore]
    public int ModuloId { get; set; }
}
