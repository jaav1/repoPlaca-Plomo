using UnityEngine;
using UnityEngine.Events; // ¡Agrega este using!

public class ClueObject : MonoBehaviour
{
    // El ID que tu sistema de inventario usa para identificar el ítem
    public string clueID;

    // Esta variable nos permite llamar a cualquier función desde el Inspector.
    public UnityEvent OnClickAction;
}