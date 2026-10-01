// ============================================================
// ServicioPersona — la capa de NEGOCIO de `persona`.
//
// Recibe POR CONSTRUCTOR la interfaz del repositorio: no sabe si
// detras hay PostgreSQL o un falso en memoria — y asi debe ser.
//
// No conoce HTTP: comunica los problemas con excepciones de negocio
// que el controlador traduce a codigos.
// ============================================================

using ApiFacturas.Excepciones;
using ApiFacturas.Modelos;
using ApiFacturas.Repositorios;

namespace ApiFacturas.Servicios;

public class ServicioPersona : IServicioPersona
{
    private readonly IRepositorioPersona _repositorio;

    public ServicioPersona(IRepositorioPersona repositorio)
    {
        _repositorio = repositorio;
    }

    private static string ValidarClave(string codigo)
    {
        codigo = codigo.Trim();
        if (codigo == "")
        {
            throw new ArgumentException("El campo codigo no puede estar vacio.");
        }
        return codigo;
    }

    public async Task<List<Persona>> ListarAsync(int limite)
    {
        // El contrato dice 400 (no 422) para limites invalidos: es una
        // REGLA DE NEGOCIO, no un problema de forma del body.
        if (limite <= 0)
        {
            throw new ArgumentException("El limite debe ser un entero mayor que cero.");
        }
        return await _repositorio.ObtenerTodosAsync(limite);
    }

    public async Task<Persona> ObtenerAsync(string codigo)
    {
        codigo = ValidarClave(codigo);
        var entidad = await _repositorio.ObtenerPorClaveAsync(codigo);
        if (entidad == null)
        {
            throw new NoEncontradoExcepcion($"No existe la persona con codigo = {codigo}");
        }
        return entidad;
    }

    public async Task CrearAsync(Persona entidad)
    {
        // El body ya paso por la peticion (tipos y rangos): aqui solo se
        // delega. Si la base rechaza —clave duplicada—, la excepcion sube
        // tal cual y el controlador la convierte en 500.
        await _repositorio.CrearAsync(entidad);
    }

    public async Task<int> ActualizarAsync(string codigo, Dictionary<string, object> datos)
    {
        codigo = ValidarClave(codigo);
        // Un PATCH con body {} paso la validacion de la peticion… pero no
        // tiene sentido de negocio: no hay nada que actualizar -> 400.
        if (datos.Count == 0)
        {
            throw new ArgumentException("No se envio ningun campo para actualizar.");
        }
        var filasAfectadas = await _repositorio.ActualizarAsync(codigo, datos);
        if (filasAfectadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe la persona con codigo = {codigo}");
        }
        return filasAfectadas;
    }

    public async Task<int> EliminarAsync(string codigo)
    {
        codigo = ValidarClave(codigo);
        var filasEliminadas = await _repositorio.EliminarAsync(codigo);
        if (filasEliminadas == 0)
        {
            throw new NoEncontradoExcepcion($"No existe la persona con codigo = {codigo}");
        }
        return filasEliminadas;
    }
}
