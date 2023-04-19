namespace DataAccess.Data;

public class MySQLConfig
{
    public MySQLConfig(string cadenaConexion)
    {
        CadenaConexion = cadenaConexion;
    }

    public string CadenaConexion { get; set; }
}
