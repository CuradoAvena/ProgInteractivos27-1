using UnityEngine;

// =====================================================================
//  IdentidadProducto.cs   ("la credencial" de cada cubo)
//
//  DÓNDE SE PONE: en el PREFAB de cada cubo (crudo, procesado, servible, basura).
//  Los demás scripts le preguntan a este: "¿qué eres? ¿en qué estado estás?".
//
//  QUÉ CONFIGURAR en el Inspector de cada prefab:
//    - tipoCubo:     qué producto es (Rojo, Azul...)
//    - estadoActual: Crudo, Procesado, Servible o Basura
//  El cubo también necesita un Rigidbody y un Collider (no trigger).
// =====================================================================
public class IdentidadProducto : MonoBehaviour
{
    public TipoProducto tipoCubo;
    public EstadoProducto estadoActual;

    // [HideInInspector] = existen, pero no se muestran en el Inspector.
    // Los cambia el código solo, no los toques tú.

    // Verdadero mientras el jugador lo carga. Así las mesas lo ignoran.
    [HideInInspector] public bool enMano;

    // Verdadero mientras una mesa lo está usando (ensamblando). Nadie lo puede agarrar.
    [HideInInspector] public bool bloqueado;
}
