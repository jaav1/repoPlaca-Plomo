using UnityEngine;
using UnityEngine.Events; // �Agrega este using!

public class ClueObject : MonoBehaviour
{
    // El ID que tu sistema de inventario usa para identificar el �tem
    public string clueID;

    // Esta variable nos permite llamar a cualquier funci�n desde el Inspector.
    public UnityEvent OnClickAction;
}