// ============================================================
// PersonaActualizar — la PETICION del PATCH (parcial).
//
// NINGUN campo es obligatorio: el que llegue SI se valida. El
// contraste con PersonaReemplazo es la leccion del verbo — el MISMO
// body falla en PUT (le faltan campos) y pasa en PATCH.
//
// Si el body llega vacio ({}), eso NO es problema de forma sino de
// negocio: lo decide el servicio con un 400.
// ============================================================

using System.ComponentModel.DataAnnotations;

namespace ApiFacturas.Peticiones;

public class PersonaActualizar
{
    [StringLength(100, MinimumLength = 1,
        ErrorMessage = "El campo nombre debe tener entre 1 y 100 caracteres.")]
    public string? Nombre { get; set; }

    [StringLength(100, MinimumLength = 1,
        ErrorMessage = "El campo email debe tener entre 1 y 100 caracteres.")]
    public string? Email { get; set; }

    [StringLength(20, MinimumLength = 1,
        ErrorMessage = "El campo telefono debe tener entre 1 y 20 caracteres.")]
    public string? Telefono { get; set; }
}
