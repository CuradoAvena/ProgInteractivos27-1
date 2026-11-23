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
    // Busca entre el objeto y sus hijos un BoxCollider marcado como "Is Trigger".
    // Se usa cuando olvidaste arrastrar la zona en el Inspector. Ignora el collider
    // SÓLIDO de la mesa (ese no es la zona), que era el que se elegía por error antes.
    public static BoxCollider BuscarZona(Component dueno)
    {
        foreach (BoxCollider c in dueno.GetComponentsInChildren<BoxCollider>())
        {
            if (c.isTrigger) return c;
        }
        return null;
    }

    // Mueve "cambio" metros todos los cubos que estén dentro de las zonas dadas
    // (menos los que el jugador carga). Lo usan las mesas que se mueven, para
    // que los cubos viajen con ellas y no se queden flotando.
    public static void Acarrear(BoxCollider[] zonas, Vector3 cambio)
    {
        if (zonas == null) return;

        // Un cubo podría estar en dos zonas a la vez: este conjunto evita moverlo doble
        HashSet<IdentidadProducto> yaMovidos = new HashSet<IdentidadProducto>();

        foreach (BoxCollider zona in zonas)
        {
            foreach (IdentidadProducto id in Detectar(zona))
            {
                if (id.enMano) continue;
                if (!yaMovidos.Add(id)) continue;

                id.transform.position += cambio;
            }
        }
    }

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
