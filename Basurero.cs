using UnityEngine;

// =====================================================================
//  Basurero.cs
//
//  DÓNDE SE PONE: en el basurero.
//
//  CÓMO FUNCIONA: todo producto que esté en su zona y que NO lo cargue
//  el jugador se destruye. (Para tirar algo: lo sueltas sobre el basurero.)
//
//  QUÉ CONFIGURAR:
//    - zona: un objeto HIJO con BoxCollider (Is Trigger) en la boca del basurero.
// =====================================================================
public class Basurero : MonoBehaviour
{
    public BoxCollider zona;
    float proximoEscaneo;

    void Awake()
    {
        if (zona == null) zona = GetComponentInChildren<BoxCollider>();
    }

    void Update()
    {
        if (Time.time < proximoEscaneo) return;
        proximoEscaneo = Time.time + 0.1f;

        foreach (IdentidadProducto id in ZonaUtil.Detectar(zona))
        {
            if (id.enMano) continue;     // lo que cargas no se tira hasta que lo sueltes
            Destroy(id.gameObject);
        }
    }
}
