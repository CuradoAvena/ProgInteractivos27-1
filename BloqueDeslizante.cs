using System.Collections.Generic;
using UnityEngine;

// =====================================================================
//  BloqueDeslizante.cs   (un bloque de mesas que resbala con la inclinación)
//
//  UN BLOQUE = varias mesas/dispensadores/estufas pegados que se mueven
//  JUNTOS como una sola pieza.
//
//  CÓMO ARMAR UN BLOQUE:
//    1. Crea un objeto vacío (clic derecho > Create Empty) llamado "Bloque1"
//       en el centro de ese grupo de mesas.
//    2. Arrastra dentro de "Bloque1" todas las mesas del grupo (quedan como hijas).
//    3. Agrégale a "Bloque1" este script y un Rigidbody con "Is Kinematic" activado.
//       NO uses MesaMovil en el mismo bloque, y no pongas scripts de movimiento
//       ni Rigidbody a las mesas hijas.
//
//  CÓMO FUNCIONA:
//    - Cuando el piso (InclinacionPiso) se inclina, el bloque resbala hacia el
//      lado que quedó más bajo, siguiendo un "riel" (una línea recta).
//    - Si la inclinación es muy leve, el bloque no se mueve (como la fricción real).
//    - Al llegar al final del riel se detiene (con un pequeño rebote).
//    - Cuando el piso se inclina al otro lado, regresa.
//    - Los cubos sobre las mesas del bloque viajan con él.
//
//  QUÉ CONFIGURAR:
//    - piso:             arrastra el objeto con InclinacionPiso (si lo dejas vacío, lo busca solo).
//    - direccionRiel:    por dónde resbala. (1,0,0) = a lo largo del eje X. Si dos
//                        bloques deben moverse al revés, usa (-1,0,0) en uno de ellos.
//    - recorridoNegativo / recorridoPositivo: metros que puede avanzar desde donde
//                        lo pusiste, hacia cada lado del riel.
//    - resbalosidad:     más alto = resbala más rápido y más lejos.
//    - friccion:         más alto = se frena más rápido.
//    - umbralInicio:     inclinación mínima para que empiece a moverse.
//    - rebote:           0 = se detiene seco al final, 1 = rebota fuerte.
//    - zonasCarga:       déjalo VACÍO: encuentra solo las zonas (trigger) del bloque.
// =====================================================================
public class BloqueDeslizante : MonoBehaviour
{
    [Header("Conexión")]
    public InclinacionPiso piso;

    [Header("Riel")]
    public Vector3 direccionRiel = new Vector3(1f, 0f, 0f);
    public float recorridoNegativo = 2f;
    public float recorridoPositivo = 2f;

    [Header("Deslizamiento")]
    public float resbalosidad = 2f;
    public float friccion = 2f;
    public float umbralInicio = 0.3f;
    [Range(0f, 1f)] public float rebote = 0.2f;

    [Header("Carga")]
    [Tooltip("Déjalo vacío: se llenan solas con todas las zonas (trigger) del bloque")]
    public BoxCollider[] zonasCarga;

    Vector3 inicio;         // donde empezó el bloque
    Vector3 riel;           // dirección del riel (de largo 1)
    float posicionEnRiel;   // cuántos metros nos hemos movido desde el inicio
    float velocidad;        // qué tan rápido resbalamos (metros por segundo)

    void Awake()
    {
        inicio = transform.position;

        riel = direccionRiel.normalized;
        if (riel == Vector3.zero) riel = Vector3.right;   // por si la dejaste en (0,0,0)

        // Si no arrastraste el piso, lo buscamos en la escena
        if (piso == null) piso = FindFirstObjectByType<InclinacionPiso>();

        // Si no asignaste zonas, buscamos todas las que sean trigger entre las hijas
        if (zonasCarga == null || zonasCarga.Length == 0)
        {
            List<BoxCollider> encontradas = new List<BoxCollider>();
            foreach (BoxCollider c in GetComponentsInChildren<BoxCollider>())
            {
                if (c.isTrigger) encontradas.Add(c);
            }
            zonasCarga = encontradas.ToArray();
        }
    }

    void Update()
    {
        if (piso == null) return;

        // 1. ¿Cuánto empuja la pendiente a lo largo de nuestro riel?
        //    (Dot = qué parte de la fuerza va en la dirección del riel)
        float aceleracion = Vector3.Dot(piso.AceleracionPendiente, riel) * resbalosidad;

        // 2. Si el empujón es muy débil, la fricción lo gana y no nos movemos
        if (Mathf.Abs(aceleracion) < umbralInicio) aceleracion = 0f;

        // 3. Ganamos velocidad con el empujón y la perdemos con la fricción
        velocidad += aceleracion * Time.deltaTime;
        velocidad *= Mathf.Max(0f, 1f - friccion * Time.deltaTime);

        // 4. Calculamos la nueva posición sobre el riel
        float nueva = posicionEnRiel + velocidad * Time.deltaTime;

        // 5. Al llegar a un extremo nos detenemos (con un poco de rebote)
        if (nueva > recorridoPositivo)
        {
            nueva = recorridoPositivo;
            velocidad = -velocidad * rebote;
        }
        else if (nueva < -recorridoNegativo)
        {
            nueva = -recorridoNegativo;
            velocidad = -velocidad * rebote;
        }

        // 6. Movemos el bloque y los cubos que lleva encima el mismo tramo
        float tramo = nueva - posicionEnRiel;
        if (Mathf.Abs(tramo) > 0f)
        {
            Vector3 cambio = riel * tramo;
            ZonaUtil.Acarrear(zonasCarga, cambio);   // primero los cubos...
            transform.position += cambio;            // ...luego el bloque
            posicionEnRiel = nueva;
        }
    }

    // Dibuja el riel en la ventana Scene (solo se ve en el editor).
    void OnDrawGizmosSelected()
    {
        Vector3 centro = Application.isPlaying ? inicio : transform.position;
        Vector3 dir = Application.isPlaying ? riel : direccionRiel.normalized;

        Vector3 a = centro - dir * recorridoNegativo;
        Vector3 b = centro + dir * recorridoPositivo;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(a, b);
        Gizmos.DrawWireSphere(a, 0.2f);
        Gizmos.DrawWireSphere(b, 0.2f);
    }
}
