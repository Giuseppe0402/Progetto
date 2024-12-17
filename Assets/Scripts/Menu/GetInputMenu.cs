using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GetInputMenu : Panel
{
    //Riferimenti agli oggetti UI
    [SerializeField] private TextMeshProUGUI messageText = null;
    [SerializeField] private TextMeshProUGUI buttonPositiveText = null;
    [SerializeField] private TextMeshProUGUI buttonNegativeText = null;
    [SerializeField] private Button positiveButton = null;
    [SerializeField] private Button negativeButton = null;
    [SerializeField] private TMP_InputField input = null;

    public delegate void Callback(string input); //Delegato per gestire l'input dell'utente
    private Callback callback = null; //Funzione di callback che riceve l'input dell'utente
    private Type type = Type.String; //Tipo di input previsto (default è stringa)

    public enum Type //Enum che definisce i tipi di input possibili
    {
        String = 1, Integer = 2, Float = 3
    }

    public override void Initialize() //Metodo di inizializzazione e assegna i vari listener ai rispettivi pulsanti
    {
        if (IsInitialized)
        {
            return;
        }
        positiveButton.onClick.AddListener(Positive);
        negativeButton.onClick.AddListener(Negative);
        base.Initialize();
    }

    public void Open(Callback callback, Type type, uint maxLength, string message = "Quanto?", string buttonPositive = "Conferma", string buttonNegative = "Cancella") //Metodo per aprire il menu di input
    {
        Open();
        this.type = type;
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
        input.SetTextWithoutNotify(""); //Pulisce il campo di input e imposta la lunghezza massima
        input.characterLimit = (int)maxLength;
        switch (type) //Imposta il tipo di contenuto che l'utente può inserire nel campo di input
        {
            case Type.Integer: input.contentType = TMP_InputField.ContentType.IntegerNumber; break;
            case Type.Float: input.contentType = TMP_InputField.ContentType.DecimalNumber; break;
            default: input.contentType = TMP_InputField.ContentType.Standard; break;
        }
    }

    private void Positive() //Metodo che viene chiamato quando l'utente preme il pulsante "Positivo"
    {
        string value = input.text.Trim();
        if (string.IsNullOrEmpty(value) == false)
        {
            if (callback != null)
            {
                callback.Invoke(value); //Se c'è una callback, invoca la callback passando il valore dell'input
            }
            Close();
        }
    }

    private void Negative() //Metodo che viene chiamato quando l'utente preme il pulsante "Negativo"
    {
        Close();
    }

}