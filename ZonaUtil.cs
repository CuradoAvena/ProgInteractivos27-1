using System.Collections.Generic;
using UnityEngine;

// =====================================================================
//  ZonaUtil.cs   (una "herramienta" compartida)
//
//  NO se pone en ningún objeto. Es una clase estática: otros scripts la
//  llaman directamente con ZonaUtil.Detectar(...).
//
//  QUÉ HACE: le das una zona (un BoxCollider) y te devuelve la lista de
//  productos que están adentro en ese momento.
//
//  POR QUÉ NO USAMOS OnTriggerEnter: ese aviso de Unity solo llega una
//  vez al entrar, y deja de avisar cuando el cubo se queda quieto
//  (el Rigidbody "se duerme"). Preguntar cada 0.1 s es más confiable.
// =====================================================================
public static class ZonaUtil
{
    public static List<IdentidadProducto> Detectar(BoxCollider zona)
    {
        var lista = new List<IdentidadProducto>();
        if (zona == null) return lista;   // si olvidaste asignar la zona, no hacemos nada

        // Calculamos el centro y tamaño REAL de la caja en el mundo
        // (tomando en cuenta la posición, rotación y escala del objeto).
        Transform t = zona.transform;
        Vector3 centro = t.TransformPoint(zona.center);
        Vector3 e = t.lossyScale;
        Vector3 mitad = new Vector3(
            Mathf.Abs(e.x * zona.size.x),
            Mathf.Abs(e.y * zona.size.y),
            Mathf.Abs(e.z * zona.size.z)) * 0.5f;

        // OverlapBox = "dame todos los colliders que toquen esta caja".
        // QueryTriggerInteraction.Ignore = ignora otros triggers.
        Collider[] cols = Physics.OverlapBox(centro, mitad, t.rotation, ~0, QueryTriggerInteraction.Ignore);

        foreach (Collider c in cols)
        {
            // Buscamos la "credencial" en el objeto o en su padre
            IdentidadProducto id = c.GetComponentInParent<IdentidadProducto>();

            // Solo la agregamos si existe y no está ya en la lista
            // (un cubo con 2 colliders aparecería dos veces).
            if (id != null && !lista.Contains(id)) lista.Add(id);
        }
        return lista;
    }
}
