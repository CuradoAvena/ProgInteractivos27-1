using UnityEngine;

// =====================================================================
//  MesaProcesamiento.cs   (la tabla de picar / la estufa)
//
//  DÓNDE SE PONE: en la mesa de procesamiento.
//
//  CÓMO FUNCIONA (3 fases):
//    LIBRE       -> espera un cubo CRUDO en su zona.
//    PROCESANDO  -> cuenta tiempoProceso. Al terminar cambia el cubo por
//                   el prefabResultado (Procesado).
//    LISTO       -> si nadie lo recoge en tiempoParaArruinarse, se quema
//                   y se cambia por prefabInservible (Basura).
//  Si el jugador agarra el cubo en medio del proceso, se cancela.
//
//  QUÉ CONFIGURAR:
//    - zona:              un objeto HIJO con BoxCollider (Is Trigger) encima de la mesa.
//    - puntoColocacion:   un objeto vacío sobre la superficie, donde se acomoda el cubo.
//    - prefabResultado:   cubo con IdentidadProducto en estado Procesado.
//    - prefabInservible:  cubo con IdentidadProducto en estado Basura.
//  La mesa también necesita un Collider sólido para que los cubos se apoyen.
// =====================================================================
public class MesaProcesamiento : MonoBehaviour
{
    [Header("Zona")]
    public BoxCollider zona;
    public Transform puntoColocacion;

    [Header("Procesamiento")]
    public GameObject prefabResultado;
    public float tiempoProceso = 3f;

    [Header("Sobrecalentamiento")]
    public GameObject prefabInservible;
    public float tiempoParaArruinarse = 5f;

    // Las 3 fases posibles de la mesa
    enum Fase { Libre, Procesando, Listo }
    Fase fase = Fase.Libre;

    IdentidadProducto actual;   // el cubo que la mesa tiene ahora
    float cronometro;           // cuántos segundos lleva en la fase actual
    float proximoEscaneo;       // para revisar la zona solo cada 0.1 s

    void Awake()
    {
        // Si olvidaste arrastrar algo, intentamos encontrarlo solos
        if (puntoColocacion == null) puntoColocacion = transform;
        if (zona == null) zona = GetComponentInChildren<BoxCollider>();
    }

    void Update()
    {
        // "switch" = según la fase, haz una cosa distinta
        switch (fase)
        {
            case Fase.Libre:
                BuscarCrudo();
                break;

            case Fase.Procesando:
                if (!SigueEnMesa()) { Liberar(); break; }   // se lo llevaron: cancelamos
                cronometro += Time.deltaTime;               // Time.deltaTime = segundos desde el frame anterior
                if (cronometro >= tiempoProceso) Transformar(prefabResultado, true);
                break;

            case Fase.Listo:
                if (!SigueEnMesa()) { Liberar(); break; }
                cronometro += Time.deltaTime;
                if (cronometro >= tiempoParaArruinarse) Transformar(prefabInservible, false);
                break;
        }
    }

    // Revisa si hay un cubo crudo en la zona y, si lo hay, empieza a procesarlo.
    void BuscarCrudo()
    {
        if (Time.time < proximoEscaneo) return;
        proximoEscaneo = Time.time + 0.1f;

        foreach (IdentidadProducto id in ZonaUtil.Detectar(zona))
        {
            if (id.enMano || id.bloqueado) continue;                  // lo cargan o está ocupado: lo saltamos
            if (id.estadoActual != EstadoProducto.Crudo) continue;    // solo aceptamos crudos

            actual = id;
            Fijar(id);
            cronometro = 0f;
            fase = Fase.Procesando;
            return;
        }
    }

    // Falso si el jugador lo agarró o si lo destruyeron.
    bool SigueEnMesa() { return actual != null && !actual.enMano; }

    // Vuelve a dejar la mesa libre.
    void Liberar()
    {
        actual = null;
        fase = Fase.Libre;
    }

    // Cambia el cubo actual por otro prefab (el resultado o la basura).
    void Transformar(GameObject prefab, bool esperarRecogida)
    {
        // Recordamos dónde estaba para poner el nuevo exactamente ahí
        Vector3 pos = actual.transform.position;
        Quaternion rot = actual.transform.rotation;
        Destroy(actual.gameObject);
        actual = null;

        if (prefab == null) { fase = Fase.Libre; return; }

        GameObject nuevo = Instantiate(prefab, pos, rot);

        // La basura ya no se procesa: la mesa queda libre
        if (!esperarRecogida) { fase = Fase.Libre; return; }

        actual = nuevo.GetComponent<IdentidadProducto>();
        if (actual == null)
        {
            Debug.LogWarning("El prefab resultado no tiene IdentidadProducto.", nuevo);
            fase = Fase.Libre;
            return;
        }
        Fijar(actual);
        cronometro = 0f;
        fase = Fase.Listo;
    }

    // Acomoda el cubo en la mesa y le apaga la física para que no se mueva.
    void Fijar(IdentidadProducto id)
    {
        Rigidbody rb = id.GetComponentInParent<Rigidbody>();
        if (rb != null)
        {
            // Solo frenamos si NO es kinematic (Unity avisa si se intenta en uno kinematic)
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            rb.isKinematic = true;
        }
        id.transform.position = puntoColocacion.position;
        id.transform.rotation = Quaternion.identity;
    }
}