using System;
using UnityEngine;


public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }


    [Range(0, 100)] public int sospecha = 0;
    [Range(0, 100)] public int confianza = 50;


    public event Action<int, int> OnStatsChanged;


    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }


    public void AddSospecha(int delta) { SetSospecha(sospecha + delta); }
    public void AddConfianza(int delta) { SetConfianza(confianza + delta); }


    public void SetSospecha(int value)
    {
        sospecha = Mathf.Clamp(value, 0, 100);
        OnStatsChanged?.Invoke(sospecha, confianza);
    }


    public void SetConfianza(int value)
    {
        confianza = Mathf.Clamp(value, 0, 100);
        OnStatsChanged?.Invoke(sospecha, confianza);
    }
}