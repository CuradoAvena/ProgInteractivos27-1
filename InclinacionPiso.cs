using UnityEngine;

// =====================================================================
//  InclinacionPiso.cs   (la inclinación del piso, por eventos)
//
//  DÓNDE SE PONE: en UN objeto vacío de la escena (ej. "Inclinacion").
//
//  QUÉ HACE:
//    - El piso está plano y quieto. Cada cierto tiempo (por ejemplo 20 s)
//      ocurre UNA inclinación: el piso se inclina suavemente hacia un lado,
//      llega a su máximo y vuelve a quedar plano.
//    - Mientras está inclinado, los BloqueDeslizante resbalan hacia el lado
//      bajo. Cuando el piso vuelve a quedar plano, se frenan donde estén.
//    - En la siguiente inclinación el piso se va hacia el lado CONTRARIO
//      (si lo dejas activado), así los bloques regresan.
//    - Se inclina SOLO el piso decorativo ("pisosVisuales"). La cámara, las
//      mesas y el jugador NO se inclinan.
//
//  CÓMO PREPARAR EL PISO:
//    1. Duplica tu Plane (Ctrl+D) y llámalo "PisoVisual".
//    2. A "PisoVisual" quítale el Mesh Collider.
//    3. Al Plane original desactívale solo el Mesh Renderer (queda invisible
//       pero sigue siendo el suelo real, plano).
//    4. Arrastra "PisoVisual" a la lista "pisosVisuales".
//
//  QUÉ CONFIGURAR:
//    - tiempoEntreInclinaciones: segundos de calma entre una inclinación y la siguiente.
//    - primerEventoEn:           segundos que pasan desde que das Play hasta la primera.
//    - duracionInclinacion:      segundos que dura cada inclinación (ida y regreso a plano).
//    - anguloMaximo:             qué tanto se inclina (grados). Más ángulo = bloques más rápidos.
//    - alternarDireccion:        activado = cada vez se inclina al lado contrario.
//    - empezarAlReves:           cambia hacia qué lado va la primera inclinación.
//    - ejeInclinacion:           (0,0,1) = hacia izquierda/derecha (bloques en X).
//    - pisosVisuales / intensidadPiso: el piso decorativo y cuánto del ángulo se ve.
//    - balanceoContinuo:         (opcional) activado = vuelve al balanceo constante de antes.
// =====================================================================
public class InclinacionPiso : MonoBehaviour
{
    [Header("Cuándo se inclina")]
    public float tiempoEntreInclinaciones = 20f;
    public float primerEventoEn = 20f;
    public float duracionInclinacion = 5f;

    [Header("Cómo se inclina")]
    public float anguloMaximo = 8f;
    public bool alternarDireccion = true;
    public bool empezarAlReves = false;
    public Vector3 ejeInclinacion = new Vector3(0f, 0f, 1f);

    [Header("Qué se ve inclinado (solo visual)")]
    public Transform[] pisosVisuales;
    [Range(0f, 1f)] public float intensidadPiso = 0.3f;

    [Header("Opcional")]
    [Tooltip("Si se activa, el piso se balancea todo el tiempo (sin pausas)")]
    public bool balanceoContinuo = false;
    public float periodoContinuo = 10f;

    [Header("Cámara (opcional, 0 = la pantalla NO se inclina)")]
    public Transform camara;
    [Range(0f, 1f)] public float intensidadCamara = 0f;
    public bool invertirCamara = false;

    // Datos que otros scripts pueden LEER (pero no cambiar)
    public float AnguloActual { get; private set; }
    public Vector3 AceleracionPendiente { get; private set; }
    public bool Inclinando { get; private set; }   // ¿hay una inclinación en curso ahora?

    float cronometro;        // segundos transcurridos en la fase actual (calma o inclinación)
    float tiempoContinuo;
    float direccion = 1f;    // +1 o -1: hacia qué lado va la inclinación
    Quaternion rotacionInicialCamara;
    Quaternion[] rotacionesInicialesPisos;

    void Awake()
    {
        if (camara != null) rotacionInicialCamara = camara.localRotation;

        // Guardamos cómo estaba cada piso para poder inclinarlo desde ahí
        if (pisosVisuales != null)
        {
            rotacionesInicialesPisos = new Quaternion[pisosVisuales.Length];
            for (int i = 0; i < pisosVisuales.Length; i++)
            {
                if (pisosVisuales[i] != null)
                    rotacionesInicialesPisos[i] = pisosVisuales[i].rotation;
            }
        }

        direccion = empezarAlReves ? -1f : 1f;

        // Truco para que la primera inclinación llegue a los "primerEventoEn" segundos:
        // adelantamos el cronómetro de calma lo que haga falta.
        cronometro = tiempoEntreInclinaciones - primerEventoEn;
    }

    void Update()
    {
        if (balanceoContinuo) ActualizarContinuo();
        else ActualizarPorEventos();

        Vector3 eje = ejeInclinacion.normalized;

        // La rotación que tendría el piso con ese ángulo
        Quaternion rotacion = Quaternion.AngleAxis(AnguloActual, eje);

        // "normal" = hacia dónde mira la superficie del piso inclinado
        Vector3 normal = rotacion * Vector3.up;

        // Gravedad proyectada sobre el piso = hacia dónde resbalaría algo
        AceleracionPendiente = Vector3.ProjectOnPlane(Physics.gravity, normal);

        // ---- Efecto visual: SOLO se inclina el piso decorativo ----
        if (pisosVisuales != null)
        {
            Quaternion rotacionVisual = Quaternion.AngleAxis(AnguloActual * intensidadPiso, eje);

            for (int i = 0; i < pisosVisuales.Length; i++)
            {
                if (pisosVisuales[i] != null)
                    pisosVisuales[i].rotation = rotacionVisual * rotacionesInicialesPisos[i];
            }
        }

        // ---- Cámara: solo si le diste intensidad (por defecto, no se mueve) ----
        if (camara != null && intensidadCamara > 0f)
        {
            float giro = AnguloActual * intensidadCamara * (invertirCamara ? 1f : -1f);
            camara.localRotation = rotacionInicialCamara * Quaternion.Euler(0f, 0f, giro);
        }
    }

    // Calma -> una inclinación -> calma -> otra inclinación...
    void ActualizarPorEventos()
    {
        cronometro += Time.deltaTime;

        if (Inclinando)
        {
            // t va de 0 a 1 durante la inclinación
            float t = Mathf.Clamp01(cronometro / Mathf.Max(duracionInclinacion, 0.1f));

            // Mathf.Sin(PI * t) sube suave de 0 al máximo y baja suave a 0:
            // el piso arranca y termina plano, sin saltos.
            AnguloActual = direccion * anguloMaximo * Mathf.Sin(Mathf.PI * t);

            if (t >= 1f)
            {
                // Terminó: el piso queda plano y empieza la calma
                Inclinando = false;
                AnguloActual = 0f;
                cronometro = 0f;
                if (alternarDireccion) direccion = -direccion;   // la próxima, al otro lado
            }
        }
        else
        {
            AnguloActual = 0f;

            if (cronometro >= tiempoEntreInclinaciones)
            {
                Inclinando = true;
                cronometro = 0f;
            }
        }
    }

    // Modo de antes: el piso sube y baja todo el tiempo.
    void ActualizarContinuo()
    {
        tiempoContinuo += Time.deltaTime;
        float duracion = Mathf.Max(periodoContinuo, 0.1f);
        AnguloActual = anguloMaximo * Mathf.Sin(2f * Mathf.PI * tiempoContinuo / duracion);
        Inclinando = true;
    }
}