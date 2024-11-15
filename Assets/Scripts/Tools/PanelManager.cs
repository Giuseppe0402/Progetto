using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    
    private Dictionary<string, Panel> panels = new Dictionary<string, Panel>(); //Dizionario che mappa gli ID dei panels
    private bool initialized = false; //Variabile che tiene traccia se il PanelManager è stato inizializzato.
    private Canvas[] canvas = null;
    private static PanelManager singleton = null;  //Riferimento alla singola istanza del PanelManager (singleton)

    public static PanelManager Singleton //Proprietà statica per ottenere l'istanza unica del PanelManager. Prima lo cerca e se non esiste allora lo crea
    {
        get
        {
            if (singleton == null)
            {
                singleton = FindFirstObjectByType<PanelManager>();
                if (singleton == null)
                {
                    singleton = new GameObject("PanelManager").AddComponent<PanelManager>();
                }
                singleton.Initialize();
            }
            return singleton; 
        }
    }

    private void Initialize() //Metodo che inizializza il PanelManager e carica tutti i panels dalla scena.
    {
        if (initialized) { return; }
        initialized = true;
        panels.Clear();
        canvas = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        if (canvas != null)
        {
            for (int i = 0; i < canvas.Length; i++)
            {
                Panel[] list = canvas[i].gameObject.GetComponentsInChildren<Panel>(true);
                if (list != null)
                {
                    for (int j = 0; j < list.Length; j++)
                    {
                        if (string.IsNullOrEmpty(list[j].ID) == false && panels.ContainsKey(list[j].ID) == false)
                        {
                            list[j].Initialize();
                            list[j].Canvas = canvas[i];
                            panels.Add(list[j].ID, list[j]);
                        }
                    }
                }
            }
        }
    } //Facciamo il Clear della dictionary, cerchiamo in tutte le canvases nella scena per trovare tutti i tipi di panel e dopo li andiamo ad aggiungere nella dictionary
    
    private void OnDestroy()  //Metodo che viene chiamato quando l'oggetto PanelManager viene distrutto.
    {
        if (singleton == this)
        {
            singleton = null;
        }
    }
    
    public static Panel GetSingleton(string id) //Metodo statico che restituisce un pannello dato un ID.
    {
        if (Singleton.panels.ContainsKey(id))
        {
            return Singleton.panels[id];
        }
        return null;
    }
    
    public static void Open(string id) //Metodo statico per aprire un pannello dato un ID.
    {
        var panel = GetSingleton(id);
        if (panel != null)
        {
            panel.Open();
        }
    }
    
    public static void Close(string id) //Metodo statico per chiudere un pannello dato un ID.
    {
        var panel = GetSingleton(id);
        if (panel != null)
        {
            panel.Close();
        }
    }
    
    public static bool IsOpen(string id) //Metodo statico per verificare se un pannello è aperto, dato un ID.
    {
        if (Singleton.panels.ContainsKey(id))
        {
            return Singleton.panels[id].IsOpen;
        }
        return false;
    }
    
    public static void CloseAll() //Metodo statico per chiudere tutti i pannelli nella scena.
    {
        foreach (var panel in Singleton.panels)
        {
            if (panel.Value != null)
            {
                panel.Value.Close();
            }
        }
    }
    
}