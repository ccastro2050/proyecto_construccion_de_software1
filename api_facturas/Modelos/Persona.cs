// ============================================================
// Persona — el MODELO de la tabla `persona` (la clase entidad).
//
// Modelo = clase ENTIDAD: una por tabla. Los body de los verbos NO
// son modelos: son PETICIONES y viven en Peticiones/.
//
// Mismo patron que Producto.cs — y eso es el punto: la v1 son SEIS
// rebanadas verticales identicas salvo los campos.
// ============================================================

namespace ApiFacturas.Modelos;

public class Persona
{
    /// <summary>Codigo (la llave primaria).</summary>
    public required string Codigo { get; set; }

    /// <summary>Nombre.</summary>
    public required string Nombre { get; set; }

    /// <summary>Email.</summary>
    public required string Email { get; set; }

    /// <summary>Telefono.</summary>
    public required string Telefono { get; set; }
}
