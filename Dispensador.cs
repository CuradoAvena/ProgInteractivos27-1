using System.Collections;
using UnityEngine;

// =====================================================================
//  Dispensador.cs   (la caja de ingredientes infinitos)
//
//  DÓNDE SE PONE: en el objeto del dispensador (un cubo grande, por ejemplo).
//
//  CÓMO FUNCIONA: al iniciar crea un cubo crudo encima. Cuando el jugador
//  se lo lleva, espera un momento y crea otro. Así nunca se acaba.
//
//  QUÉ CONFIGURAR:
//    - prefabCrudo:    el prefab del cubo (con IdentidadProducto en estado Crudo)
//    - puntoAparicion: (opcional) un objeto vacío donde aparece el cubo.
//                      Si lo dejas vacío, aparece 0.5 m arriba del dispensador.
//  El dispensador debe tener un Collider SÓLIDO para que el cubo se apoye.
// =====================================================================
public class Dispensador : MonoBehaviour
{
    [Header("Configuración del Dispensador")]
    public GameObject prefabCrudo;
    public float tiempoRecarga = 1f;     // segundos antes de sacar otro cubo

    public Transform puntoAparicion;
    [Tooltip("Si el cubo se aleja más que esto del punto, se considera tomado")]
    public float distanciaLiberacion = 0.8f;

    GameObject actual;     // el cubo que está esperando ser tomado
    bool recargando;       // ¿ya estamos esperando para sacar otro?

    // Esto es una "propiedad": se usa como una variable, pero calcula su valor al leerla.
    Vector3 PosicionAparicion
    {
        get { return puntoAparicion != null ? puntoAparicion.position : transform.position + Vector3.up * 0.5f; }
    }

    void Start()
    {
        Generar();   // el primer cubo
    }

    void Update()
    {
        if (recargando) return;   // ya estamos en eso

        // El cubo "fue tomado" si ya no existe (lo destruyeron)
        // o si se alejó del punto de aparición.
        bool tomado = actual == null ||
            (actual.transform.position - PosicionAparicion).sqrMagnitude > distanciaLiberacion * distanciaLiberacion;

        if (tomado)
        {
            actual = null;
            StartCoroutine(Recargar());
        }
    }

    IEnumerator Recargar()
    {
        recargando = true;
        yield return new WaitForSeconds(tiempoRecarga);   // esperamos
        Generar();
        recargando = false;
    }

    void Generar()
    {
        // Instantiate = "crea una copia del prefab en esta posición"
        actual = Instantiate(prefabCrudo, PosicionAparicion, Quaternion.identity);
    }
}
