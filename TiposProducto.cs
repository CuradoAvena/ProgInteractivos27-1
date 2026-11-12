// =====================================================================
//  TiposProducto.cs
//  Aquí se definen las "listas de opciones" (enums) del juego.
//  Un enum es una lista de nombres fijos. En vez de escribir textos
//  como "rojo" (que se pueden escribir mal), usamos TipoProducto.Rojo.
//
//  NO se pone en ningún objeto de la escena. Solo existe en el proyecto.
//  Si ya tenías estos enums en otro archivo, BORRA este para que no
//  salga el error "ya existe una definición de TipoProducto".
// =====================================================================

// Qué producto es (el "color" o "platillo"). CAMBIA estos nombres por los de tu juego.
public enum TipoProducto
{
    Rojo,
    Azul,
    Verde,
    Morado
}

// En qué etapa está el producto. Los scripts usan esto para decidir qué hacer con él.
public enum EstadoProducto
{
    Crudo,       // recién salido del dispensador
    Procesado,   // ya pasó por la mesa de procesamiento
    Servible,    // producto final, listo para entregar
    Basura       // se quemó o salió mal
}
