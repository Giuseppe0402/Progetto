using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class ErrorMenu : Panel
{
    //Riferimenti agli elementi UI del pannello di errore.
    [SerializeField] private TextMeshProUGUI errorText = null;
    [SerializeField] private TextMeshProUGUI buttonText = null;
    [SerializeField] private Button actionButton = null;

    public enum Action //Enum che definisce le possibili azioni che il bottone può eseguire.
    {
        None = 0, StartService = 1, SignIn = 2, OpenAuthMenu = 3
    }
    
    private Action action = Action.None; //Azione corrente che il bottone deve eseguire.

    public override void Initialize() //Metodo di inizializzazione del pannello di errore.
    {
        if (IsInitialized)
        {
            return;
        }
        actionButton.onClick.AddListener(ButtonAction); //Aggiunge un listener al bottone per eseguire l'azione quando viene cliccato.
        base.Initialize();
    }

    public override void Open() //Metodo per aprire il pannello di errore, resetta l'azione.
    {
        action = Action.None;
        base.Open();
    }
    
    public void Open(Action action, string error, string button) //Metodo che apre il pannello di errore e imposta i dettagli (azione, errore, e testo del bottone)
    {
        Open();
        this.action = action;
        if (string.IsNullOrEmpty(error) == false)
        {
            errorText.text = error;
        }
        if (string.IsNullOrEmpty(button) == false)
        {
            buttonText.text = button;
        }
    }
    
    private void ButtonAction() //Metodo che viene chiamato quando l'utente clicca sul bottone. Esegue l'azione associata.
    {
        Close();
        switch (action)
        {
            case Action.StartService:
                MenuManager.Singleton.StartClientService();
                break;
            case Action.SignIn:
                MenuManager.Singleton.SignInAnonymouslyAsync();
                break;
            case Action.OpenAuthMenu:
                PanelManager.CloseAll();
                PanelManager.Open("auth");
                break;
        }
    }
    
}