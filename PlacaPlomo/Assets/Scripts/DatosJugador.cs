using System;
using System.Collections.Generic;

[Serializable]
public class DatosJugador
{
    // Datos del jugador
    public float empatia;
    public float sospecha;
    public string escenaActual;
    public int confianzaMendoza;
    public int rayEmpatia;
    public int rayFrialdad;
    public float tension;

    // Lista de objetos recogidos del mundo
    public List<string> objetosRecogidos;

    // Datos del inventario, ahora como una lista de páginas
    public List<InventarioPaginaDatos> inventario;
}

[Serializable]
public class InventarioPaginaDatos
{
    public string pageName;
    public List<string> items;
}