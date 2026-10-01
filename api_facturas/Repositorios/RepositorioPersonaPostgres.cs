// ============================================================
// RepositorioPersonaPostgres — la capa de DATOS de `persona`.
//
// SQL escrito A MANO y SIEMPRE parametrizado; Dapper como
// micro-ejecutor. Sin Entity Framework: nada genera SQL por
// nosotros (constitucion, Art. 2).
// ============================================================

using ApiFacturas.Modelos;
using Dapper;
using Npgsql;

namespace ApiFacturas.Repositorios;

public class RepositorioPersonaPostgres : IRepositorioPersona
{
    private readonly string _cadenaConexion;

    public RepositorioPersonaPostgres(string cadenaConexion)
    {
        _cadenaConexion = cadenaConexion;
    }

    private NpgsqlConnection CrearConexion() => new(_cadenaConexion);

    public async Task<List<Persona>> ObtenerTodosAsync(int limite)
    {
        const string sql = @"SELECT codigo, nombre, email, telefono
                             FROM persona ORDER BY codigo LIMIT @limite";
        await using var conexion = CrearConexion();
        var filas = await conexion.QueryAsync<Persona>(sql, new { limite });
        return filas.ToList();
    }

    public async Task<Persona?> ObtenerPorClaveAsync(string codigo)
    {
        const string sql = @"SELECT codigo, nombre, email, telefono
                             FROM persona WHERE codigo = @codigo";
        await using var conexion = CrearConexion();
        // Cero filas -> null. El SERVICIO decide que significa ese null:
        // aqui solo hay hechos.
        return await conexion.QueryFirstOrDefaultAsync<Persona>(sql, new { codigo });
    }

    public async Task CrearAsync(Persona entidad)
    {
        const string sql = @"INSERT INTO persona (codigo, nombre, email, telefono)
                             VALUES (@Codigo, @Nombre, @Email, @Telefono)";
        await using var conexion = CrearConexion();
        await conexion.ExecuteAsync(sql, entidad);
    }

    public async Task<int> ActualizarAsync(string codigo, Dictionary<string, object> datos)
    {
        // SET dinamico SOLO con las columnas que llegaron. Los NOMBRES
        // salen de las PETICIONES (lista blanca) — jamas del cliente; los
        // VALORES van parametrizados.
        var asignaciones = string.Join(", ", datos.Keys.Select(c => $"{c} = @{c}"));
        var sql = $"UPDATE persona SET {asignaciones} WHERE codigo = @clave";
        var parametros = new DynamicParameters(datos);
        parametros.Add("clave", codigo);
        await using var conexion = CrearConexion();
        return await conexion.ExecuteAsync(sql, parametros);
    }

    public async Task<int> EliminarAsync(string codigo)
    {
        // Si otras tablas lo referencian, la FK del motor rechaza -> 500.
        const string sql = "DELETE FROM persona WHERE codigo = @codigo";
        await using var conexion = CrearConexion();
        return await conexion.ExecuteAsync(sql, new { codigo });
    }
}
