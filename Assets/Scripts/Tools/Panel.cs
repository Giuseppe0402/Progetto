using UnityEngine;

public class Panel : MonoBehaviour
{

    [SerializeField] private string id = ""; public string ID { get { return id; } } //Identificatore univoco di un panel
    [SerializeField] private RectTransform container = null; //Riferimento al contenitore del panel per gestirlo

    private bool initialized = false; public bool IsInitialized { get { return initialized; } } //Stato di inizializzazione del panel
    private bool isOpen = false; public bool IsOpen { get { return isOpen; } } //Stato di apertura del panel
    private Canvas canvas = null; public Canvas Canvas { get { return canvas; } set { canvas = value; } } //Riferimento al Canvas per gestire la gerarchia
    
    public virtual void Awake() //Chiamata automaticamente quando viene creato il Panel e lo inizializza
    {
        Initialize();
    }

    public virtual void Initialize()  //Inizializzazione del panel
    {
        if (initialized) { return; }
        initialized = true;
        Close();
    }

    public virtual void Open() //Funzione per aprire il panel
    {
        if (initialized == false) { Initialize(); }
        transform.SetAsLastSibling(); //Facendo così qualsiasi panel che apriremo andrà in primo piano nella scena
        container.gameObject.SetActive(true);
        isOpen = true;
    }

    public virtual void Close() //Funzione per chiudere il panel
    {
        if (initialized == false) { Initialize(); }
        container.gameObject.SetActive(false);
        isOpen = false;
    }
    
}