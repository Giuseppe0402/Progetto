using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Unity.Services.Authentication;
using UnityEngine.UI;
using Unity.Services.Friends;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public class MainMenu : Panel
{
    //Riferimenti agli elementi UI per la gestione del menu principale.
    [SerializeField] public TextMeshProUGUI nameText = null;
    [SerializeField] private Button logoutButton = null;
    [SerializeField] private Button friendsButton = null;
    [SerializeField] private Button renameButton = null;
    [SerializeField] private Button customizationButton = null;
    [SerializeField] private Button lobbyButton = null;
    [SerializeField] private Button quitButton = null;

    //Variabili per la gestione del servizio amici e delle lobby.
    private bool isFriendsServiceInitialized = false;
    private List<string> joinedLobbyIds = new List<string>();

    public override void Initialize() //Metodo di inizializzazione del pannello. Aggiunge i listener ai bottoni per eseguire le azioni corrispondenti.
    {
        if (IsInitialized)
        {
            return;
        }
        logoutButton.onClick.AddListener(SignOut);
        friendsButton.onClick.AddListener(Friends);
        renameButton.onClick.AddListener(RenamePlayer);
        customizationButton.onClick.AddListener(Customization);
        lobbyButton.onClick.AddListener(Lobby);
        quitButton.onClick.AddListener(Quit);
        base.Initialize();
    }

    public override void Open() //Metodo che apre il pannello del menu principale.
    {
        friendsButton.interactable = isFriendsServiceInitialized; //Disabilita o abilita il bottone degli amici in base alla disponibilità del servizio amici.
        UpdatePlayerNameUI();
        if (isFriendsServiceInitialized == false)
        {
            InitializeFriendsServiceAsync();
        }
        base.Open();
    }

    private async void Lobby() //Metodo per entrare in una lobby esistente.
    {
        PanelManager.Open("loading");
        try
        {
            var lobbyIds = await LobbyService.Instance.GetJoinedLobbiesAsync(); //Questo ci dà una lista di stringhe contenenti tutte le lobby in cui siamo entrati
            joinedLobbyIds = lobbyIds; //Questo try catch spesso fallirà perché c'è un limite con i servizi che Unity ci dà e per questo andiamo a salvare questo dato dentro uno nostro che sappiamo essere al sicuro
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        Lobby lobby = null;
        if (joinedLobbyIds.Count > 0) //Se l'utente è connesso a una o più lobby, prova a ottenere i dettagli dell'ultima lobby a cui l'utente è connesso.
        {
            try
            {
                lobby = await LobbyService.Instance.GetLobbyAsync(joinedLobbyIds.Last());
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        if (lobby == null) //Se la lobby non è stata trovata, cerca nel pannello delle lobby esistenti.
        {
            LobbyMenu panel = (LobbyMenu)PanelManager.GetSingleton("lobby");
            if (panel.JoinedLobby != null && joinedLobbyIds.Count > 0 && panel.JoinedLobby.Id == joinedLobbyIds.Last())
            {
                lobby = panel.JoinedLobby;
            }
        }

        if (lobby != null) //Se una lobby è stata trovata, apri il pannello della lobby. Altrimenti, mostra il pannello di ricerca delle lobby.
        {
            LobbyMenu panel = (LobbyMenu)PanelManager.GetSingleton("lobby");
            panel.Open(lobby);
        }
        else
        {
            PanelManager.Open("lobby_search");
        }

        PanelManager.Close("loading");
    }

    private void Customization() //Metodo che apre il pannello di personalizzazione.
    {
        PanelManager.Open("customization");
    }

    private async void InitializeFriendsServiceAsync() //Metodo asincrono per inizializzare il servizio amici.
    {
        try
        {
            await FriendsService.Instance.InitializeAsync();
            isFriendsServiceInitialized = true;
            friendsButton.interactable = true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void SignOut() //Metodo che gestisce il logout.
    {
        ActionConfirmMenu panel = (ActionConfirmMenu)PanelManager.GetSingleton("action_confirm");
        panel.Open(SignOutResult, "Sei sicuro di voler effettuare il logout?", "Si", "No");
    }

    private void SignOutResult(ActionConfirmMenu.Result result) //Metodo che gestisce il risultato della conferma di logout.
    {
        if (result == ActionConfirmMenu.Result.Positive) //Se l'utente conferma il logout, esegui il logout e resetta il servizio amici.
        {
            MenuManager.Singleton.SignOut();
            isFriendsServiceInitialized = false;
        }
    }

    private void Quit()
    {
        ActionConfirmMenu panel = (ActionConfirmMenu)PanelManager.GetSingleton("action_confirm");
        panel.Open(QuitResult, "Sei sicuro di voler uscire?", "Si", "No");
    }

    private void QuitResult(ActionConfirmMenu.Result result)
    {
        if (result == ActionConfirmMenu.Result.Positive)
        {
            Application.Quit();
        }
    }

    private void UpdatePlayerNameUI() //Metodo che aggiorna il nome del giocatore nell'interfaccia utente.
    {
        nameText.text = AuthenticationService.Instance.PlayerName;
    }

    private void Friends() //Metodo che apre il pannello degli amici.
    {
        PanelManager.Open("friends");
    }

    private void RenamePlayer() //Metodo che apre il pannello per rinominare il giocatore.
    {
        GetInputMenu panel = (GetInputMenu)PanelManager.GetSingleton("input");
        panel.Open(RenamePlayerConfirm, GetInputMenu.Type.String, 20, "Inserisci un nome per il tuo account.", "Invia", "Cancella");
    }

    private async void RenamePlayerConfirm(string input) //Metodo che gestisce la conferma del nuovo nome del giocatore.
    {
        renameButton.interactable = false;
        try
        {
            await AuthenticationService.Instance.UpdatePlayerNameAsync(input);
            UpdatePlayerNameUI();
        }
        catch (Exception)
        {
            ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
            panel.Open(ErrorMenu.Action.None, "Errore durante il cambio del nome.", "OK");
        }
        renameButton.interactable = true;
    }

}