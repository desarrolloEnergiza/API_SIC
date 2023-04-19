using Model;

namespace DataAccess.Interfases;

public interface IRootRepo
{
    public Task<Root> GetData(int id); //Id del curso

}
