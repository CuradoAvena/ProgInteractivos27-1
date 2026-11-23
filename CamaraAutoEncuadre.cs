using UnityEngine;

// =====================================================================
//  CamaraAutoEncuadre.cs   (la cámara se acomoda sola)
//
//  DÓNDE SE PONE: en la Main Camera.
//
//  QUÉ HACE: al dar Play, busca todas las mesas, dispensadores, el basurero
//  y al jugador, calcula el rectángulo que ocupan y coloca la cámara arriba,
//  inclinada hacia el piso, a la distancia justa para que TODO quepa en pantalla.
//  Así no tienes que calcular posiciones a mano.
//
//  QUÉ CONFIGURAR:
//    - angulo:  qué tan inclinada mira hacia el piso (grados).
//               90 = totalmente desde arriba, 60 = vista tipo Overcooked, 45 = más de lado.
//    - margen:  metros extra alrededor de las mesas. Déjalo en 2 o más para que
//               los bloques que resbalan no se salgan de la pantalla.
//
//  NOTA: la rotación en Y queda en 0 para que X sea izquierda/derecha de la
//  pantalla y Z sea arriba/abajo. Para encuadrar sin dar Play, clic derecho en el
//  nombre del componente > "Encuadrar ahora".
// =====================================================================
[RequireComponent(typeof(Camera))]
public class CamaraAutoEncuadre : MonoBehaviour
{
    public float angulo = 60f;
    public float margen = 2f;

    Camera cam;

    void Start()
    {
        Encuadrar();
    }

    [ContextMenu("Encuadrar ahora")]
    public void Encuadrar()
    {
        if (cam == null) cam = GetComponent<Camera>();

        // 1. Calculamos el rectángulo (en 3D) que ocupa todo lo importante
        Bounds area = new Bounds();
        bool hayObjetos = false;

        Agregar<MesaProcesamiento>(ref area, ref hayObjetos);
        Agregar<MesaEnsamblaje>(ref area, ref hayObjetos);
        Agregar<MesaEntrega>(ref area, ref hayObjetos);
        Agregar<Dispensador>(ref area, ref hayObjetos);
        Agregar<Basurero>(ref area, ref hayObjetos);
        Agregar<ThridPersonController>(ref area, ref hayObjetos);

        if (!hayObjetos) return;   // no hay nada que encuadrar

        // 2. Le damos un margen de seguridad por todos lados
        area.Expand(margen * 2f);

        // 3. Hacia dónde mira la cámara y cuáles son sus ejes
        Quaternion rotacion = Quaternion.Euler(angulo, 0f, 0f);
        Vector3 adelante = rotacion * Vector3.forward;
        Vector3 derecha = rotacion * Vector3.right;
        Vector3 arriba = rotacion * Vector3.up;

        // 4. Qué tan abierto es el "cono de visión" de la cámara
        float tanVertical = Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float tanHorizontal = tanVertical * cam.aspect;

        // 5. Revisamos las 8 esquinas del rectángulo y buscamos la distancia
        //    mínima a la que TODAS caben en pantalla.
        Vector3 centro = area.center;
        Vector3 mitad = area.extents;
        float distancia = 0f;

        for (int sx = -1; sx <= 1; sx += 2)
        {
            for (int sy = -1; sy <= 1; sy += 2)
            {
                for (int sz = -1; sz <= 1; sz += 2)
                {
                    Vector3 v = new Vector3(sx * mitad.x, sy * mitad.y, sz * mitad.z);

                    float lateral = Mathf.Abs(Vector3.Dot(v, derecha));
                    float vertical = Mathf.Abs(Vector3.Dot(v, arriba));
                    float profundidad = Vector3.Dot(v, adelante);

                    distancia = Mathf.Max(distancia,
                        lateral / tanHorizontal - profundidad,
                        vertical / tanVertical - profundidad);
                }
            }
        }

        // 6. Colocamos la cámara: en el centro, hacia atrás la distancia calculada
        transform.rotation = rotacion;
        transform.position = centro - adelante * distancia;
    }

    // Busca todos los objetos de un tipo y agranda el rectángulo para incluirlos.
    void Agregar<T>(ref Bounds area, ref bool hayObjetos) where T : Component
    {
        foreach (T objeto in FindObjectsByType<T>(FindObjectsSortMode.None))
        {
            foreach (Renderer r in objeto.GetComponentsInChildren<Renderer>())
            {
                if (!hayObjetos)
                {
                    area = r.bounds;
                    hayObjetos = true;
                }
                else
                {
                    area.Encapsulate(r.bounds);
                }
            }
        }
    }
}
