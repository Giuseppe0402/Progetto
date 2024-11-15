using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ActionConfirmMenu : Panel
{
    //Riferimenti agli oggetti UI
    [SerializeField] private TextMeshProUGUI messageText = null;
    [SerializeField] private TextMeshProUGUI buttonPositiveText = null;
    [SerializeField] private TextMeshProUGUI buttonNegativeText = null;
    [SerializeField] private Button positiveButton = null;
    [SerializeField] private Button negativeButton = null;

    //Delegato per gestire la risposta dell'utente
    public delegate void Callback(Result result);
    private Callback callback = null; //Funzione di callback che riceve il risultato dell'azione (Positivo o Negativo)


    public enum Result //Enum che rappresenta i possibili risultati dell'azione (Positivo o Negativo)
    {
        Positive = 1, Negative = 2
    }

    public override void Initialize() //Metodo di inizializzazione e assegna i vari listener ai bottoni corrispondenti.
    {
        if (IsInitialized)
        {
            return;
        }
        positiveButton.onClick.AddListener(Positive);
        negativeButton.onClick.AddListener(Negative);
        base.Initialize();
    }
    
    public void Open(Callback callback, string message = "Sei sicuro?", string buttonPositive = "Si", string buttonNegative = "No") //Metodo per aprire il menu di conferma
    {
        Open();
        this.callback = callback;
        if (string.IsNullOrEmpty(message) == false)
        {
            messageText.text = message;
        }
        if (string.IsNullOrEmpty(buttonPositive) == false)
        {
            buttonPositiveText.text = buttonPositive;
        }
        if (string.IsNullOrEmpty(buttonNegative) == false)
        {
            buttonNegativeText.text = buttonNegative;
        }
    }
    
    private void Positive() //Metodo che viene chiamato quando l'utente preme il pulsante "Positivo"
    {
        if (callback != null)
        {
            callback.Invoke(Result.Positive); //Invoca la callback passando il risultato positivo
        }
        Close();
    }
    
    private void Negative() //Metodo che viene chiamato quando l'utente preme il pulsante "Negativo"
    {
        if (callback != null)
        {
            callback.Invoke(Result.Negative); //Invoca la callback passando il risultato negativo
        }
        Close();
    }
    
}