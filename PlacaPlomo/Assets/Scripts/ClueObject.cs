using UnityEngine;

// Este script se usa para identificar los objetos que son pistas.
// No necesita actualizarse, solo guarda información.
public class ClueObject : MonoBehaviour
{
    // Este ID debe coincidir con el nombre de la parte en tu archivo CSV.
    // Por ejemplo: "guantera", "llave", "mapa".
    public string clueID;
}