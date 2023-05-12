using Model;
using Newtonsoft.Json.Linq;
namespace DataAccess.Interfases;

public interface IAlumnoRepo
{
    List<Alumno> GetAlumnos();
    public Task<Alumno> GetById(int id);
    public Task<Alumno> GetMulti(int id);
    public string GetApiResponse(string apiKey);
    public Task<List<string>> PostApiRequest(string apiKey);

}
