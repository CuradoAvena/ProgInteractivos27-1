using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

// =====================================================================
//  AgarreFisico.cs   (las "manos" del jugador)
//
//  DÓNDE SE PONE: en el objeto del JUGADOR.
//
//  CÓMO FUNCIONA:
//    1. Cada frame lanza un "rayo grueso" hacia donde mira el jugador.
//    2. Si toca un cubo, lo ilumina (feedback visual).
//    3. Al presionar el botón: agarra (lo pega a las manos) o suelta.
//
//  QUÉ CONFIGURAR en el Inspector:
//    - puntoDeAgarre:     un objeto vacío HIJO del jugador, delante de él
//                         (ej. posición 0, 0.5, 1). Ahí se pega el cubo.
//    - capaInteractuable: la capa (Layer) de tus cubos. Créala en
//                         Layer > Add Layer y asígnala al prefab del cubo.
//    - materialBrillo:    un material brillante/amarillo cualquiera.
//    - interactuarAction: el botón. Clic en "+" y elige, por ejemplo,
//                         <Keyboard>/e
// =====================================================================
public class AgarreFisico : MonoBehaviour
{
    [Header("Manos")]
    public Transform puntoDeAgarre;
    public float distanciaAgarre = 2f;      // qué tan lejos alcanzas
    public float radioDeteccion = 0.3f;     // grosor del rayo (más grueso = más fácil apuntar)
    public float alturaRayo = 0.5f;         // el pivote del jugador suele estar en los pies; subimos el rayo
    public LayerMask capaInteractuable;

    [Header("Feedback visual")]
    public Material materialBrillo;

    [Header("Controles")]
    public InputAction interactuarAction;

    // ---- Memoria interna (el script "recuerda" estas cosas) ----
    Collider colliderJugador;          // para evitar que el cubo choque con nosotros
    bool sosteniendo;                  // ¿tenemos algo en la mano?
    IdentidadProducto objetoEnMano;    // qué cubo tenemos
    Rigidbody rbEnMano;                // su Rigidbody
    Collider[] collidersEnMano;        // sus colliders

    Renderer rendererIluminado;        // el cubo que está brillando ahora
    Material materialOriginal;         // su material de antes, para devolvérselo

    void Awake()
    {
        // Awake se ejecuta una vez al crear el objeto. Buscamos nuestro propio collider.
        colliderJugador = GetComponentInParent<Collider>();
    }

    // El botón de input necesita encenderse y apagarse con el script.
    void OnEnable() { interactuarAction.Enable(); }
    void OnDisable() { interactuarAction.Disable(); }

    void Update()
    {
        // Si el cubo que cargábamos fue destruido (ej. en el basurero), olvidamos todo.
        // (Unity considera "null" a un objeto destruido.)
        if (sosteniendo && objetoEnMano == null)
        {
            sosteniendo = false;
            rbEnMano = null;
            collidersEnMano = null;
        }

        // Si cargamos algo, no buscamos nada más. Si no, buscamos qué estamos mirando.
        IdentidadProducto mirado = sosteniendo ? null : BuscarObjetivo();

        ActualizarBrillo(mirado);

        // ¿Se presionó el botón en este frame?
        if (interactuarAction.WasPressedThisFrame())
        {
            if (sosteniendo) Soltar();
            else if (mirado != null) Agarrar(mirado);
        }
    }

    // Lanza el rayo grueso y devuelve el cubo que estamos mirando (o null si no hay).
    IdentidadProducto BuscarObjetivo()
    {
        Vector3 origen = transform.position + Vector3.up * alturaRayo;

        if (Physics.SphereCast(origen, radioDeteccion, transform.forward, out RaycastHit hit,
                distanciaAgarre, capaInteractuable, QueryTriggerInteraction.Ignore))
        {
            IdentidadProducto id = hit.collider.GetComponentInParent<IdentidadProducto>();

            // Si una mesa lo está usando (bloqueado), no se puede agarrar.
            if (id != null && !id.bloqueado) return id;
        }
        return null;
    }

    void Agarrar(IdentidadProducto id)
    {
        Rigidbody rb = id.GetComponentInParent<Rigidbody>();
        if (rb == null) return;   // sin Rigidbody no podemos manejarlo

        ApagarBrillo();

        // Guardamos en la memoria qué agarramos
        objetoEnMano = id;
        rbEnMano = rb;
        sosteniendo = true;
        id.enMano = true;   // avisa a las mesas: "ignórame, me llevan en la mano"

        // Frenamos cualquier movimiento y apagamos la física
        // (isKinematic = true -> ya no cae ni lo empujan, lo manejamos nosotros)
        // Solo frenamos si NO es kinematic (Unity avisa si se intenta en uno kinematic)
        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        rb.isKinematic = true;

        // Hacemos que el cubo y el jugador no choquen entre sí
        collidersEnMano = id.GetComponentsInChildren<Collider>();
        IgnorarJugador(true);

        // Lo pegamos a las manos: al hacerlo "hijo" del punto, se mueve con el jugador
        Transform t = id.transform;
        t.SetParent(puntoDeAgarre);
        t.position = puntoDeAgarre.position;
        t.rotation = puntoDeAgarre.rotation;
    }

    void Soltar()
    {
        objetoEnMano.transform.SetParent(null);   // lo despegamos de las manos
        rbEnMano.isKinematic = false;             // le regresamos la física: cae solo
        objetoEnMano.enMano = false;              // las mesas ya pueden verlo

        // Un momento después, volvemos a activar el choque con el jugador
        StartCoroutine(RestaurarColision(collidersEnMano));

        // Limpiamos la memoria
        sosteniendo = false;
        objetoEnMano = null;
        rbEnMano = null;
        collidersEnMano = null;
    }

    // Activa/desactiva el "ignorar choque" entre el cubo y el jugador
    void IgnorarJugador(bool ignorar)
    {
        if (colliderJugador == null || collidersEnMano == null) return;
        foreach (Collider c in collidersEnMano)
            if (c != null) Physics.IgnoreCollision(c, colliderJugador, ignorar);
    }

    // Una corrutina es una función que puede "esperar". Aquí esperamos 0.4 s
    // para que el cubo suelto no empuje al jugador al reactivar el choque.
    IEnumerator RestaurarColision(Collider[] cols)
    {
        yield return new WaitForSeconds(0.4f);
        if (colliderJugador == null || cols == null) yield break;
        foreach (Collider c in cols)
            if (c != null) Physics.IgnoreCollision(c, colliderJugador, false);
    }

    // Ilumina el cubo que miramos y apaga el anterior.
    void ActualizarBrillo(IdentidadProducto mirado)
    {
        Renderer nuevo = mirado != null ? mirado.GetComponentInChildren<Renderer>() : null;

        if (nuevo == rendererIluminado) return;   // seguimos mirando lo mismo, nada que hacer

        ApagarBrillo();
        if (nuevo != null && materialBrillo != null)
        {
            rendererIluminado = nuevo;
            materialOriginal = nuevo.sharedMaterial;   // guardamos su material original
            nuevo.sharedMaterial = materialBrillo;     // le ponemos el brillante
        }
    }

    void ApagarBrillo()
    {
        if (rendererIluminado != null) rendererIluminado.sharedMaterial = materialOriginal;
        rendererIluminado = null;
    }
}