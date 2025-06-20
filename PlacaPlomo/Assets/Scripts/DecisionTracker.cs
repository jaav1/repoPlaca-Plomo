using UnityEngine;

public class DecisionTracker : MonoBehaviour
{
    public static DecisionTracker Instance;

    public bool recogioCarpeta = false;
    public bool ayudoAlMercado = false;
    public bool accedioALaFinca = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}