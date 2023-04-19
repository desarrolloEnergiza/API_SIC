using Model;

namespace DataAccess.Interfases;

public interface IAlumnoRepo
{
    List<Alumno> GetAlumnos();
    public Task<Alumno> GetById(int id);
    public Task<Alumno> GetMulti(int id);

}
