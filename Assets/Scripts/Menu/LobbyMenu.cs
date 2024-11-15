using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine.UI;

public class LobbyMenu : Panel
{
    //Riferimenti agli oggetti UI
    [SerializeField] private LobbyPlayerItem lobbyPlayerItemPrefab = null; //Prefab per visualizzare i giocatori nella lobby
    [SerializeField] private RectTransform lobbyPlayersContainer = null;
    [SerializeField] public TextMeshProUGUI nameText = null;
    [SerializeField] private Button closeButton = null;
    [SerializeField] private Button leaveButton = null;
    [SerializeField] private Button readyButton = null;
    [SerializeField] private Button startButton = null;

    //Variabili per gestire lo stato della lobby
    private Lobby lobby = null; public Lobby JoinedLobby { get { return lobby; } } //Riferimento alla lobby corrente
    private float updateTimer = 0;
    private float heartbeatPeriod = 15;
    private bool sendingHeartbeat = false;
    private ILobbyEvents events = null;
    private bool isReady = false;
    private bool isHost = false;
    private string eventsLobbyId = "";
    private bool isStarted = false;
    private bool isJoining = false;
    
    public override void Initialize() //Metodo per inizializzare la lobby e assegnare i vari listener ai pulsanti corrispondenti.
    {
        if (IsInitialized)
        {
            return;
        }
        ClearPlayersList();
        closeButton.onClick.AddListener(ClosePanel);
        leaveButton.onClick.AddListener(LeaveLobby);
        readyButton.onClick.AddListener(SwitchReady);
        startButton.onClick.AddListener(StartGame);
        base.Initialize();
    }
    
    private async void StartGame() //Metodo per avviare il gioco (solo per l'host)
    {
        PanelManager.Open("loading");
        try
        {   //Crea una nuova allocazione Relay per gestire il collegamento (servizio per la connessione multiplayer)
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(lobby.MaxPlayers);
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>(); //Ottiene il componente di trasporto di rete
            var data = new RelayServerData(allocation, "dtls"); //Crea i dati del server di Relay
            transport.SetRelayServerData(data); //Imposta i dati del server di Relay sul trasporto
            string code = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId); // Ottiene il codice di join per il gioco

            //Imposta la sessione come Host e salva il codice di join
            SessionManager.role = SessionManager.Role.Host;
            SessionManager.joinCode = code;
            SessionManager.lobbyID = lobby.Id;
            
            SetLobbyStarting();

            //Apre il pannello che avvia il gioco
            StartingSessionMenu panel = (StartingSessionMenu)PanelManager.GetSingleton("start");
            heartbeatPeriod = 5;
            await UnsubscribeToEventsAsync();
            panel.StartGameByLobby(lobby, false);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        PanelManager.Close("loading");
    }

    private async void SetLobbyStarting() //Imposta lo stato della lobby come "avviando" informando tutti i clients
    {
        try
        {   //Aggiorna la lobby impostando il flag "started"
            UpdateLobbyOptions options = new UpdateLobbyOptions();
            options.Data = new Dictionary<string, DataObject>();
            options.Data.Add("started", new DataObject(visibility: DataObject.VisibilityOptions.Public, value: "1"));
            lobby = await LobbyService.Instance.UpdateLobbyAsync(lobby.Id, options);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void CheckStartGameStatus() //Metodo per verificare lo stato del gioco e avviare la sessione se necessario
    {
        StartingSessionMenu panel = (StartingSessionMenu)PanelManager.GetSingleton("start");
        isStarted = lobby.Data.ContainsKey("started");
        string joinCode = lobby.Data.ContainsKey("join_code") ? lobby.Data["join_code"].Value : null;
        if(panel.isLoading == false && isStarted) //Se il gioco è avviato e il pannello non sta caricando, avvia il gioco
        {
            panel.StartGameByLobby(lobby, true); //waitForConfirmation è su true perché questo è il client, quindi dovrà aspettare l'host
        }

        if(isJoining == false && panel.isLoading && string.IsNullOrEmpty(joinCode) == false && panel.isConfirmed == false) //Se il gioco è in fase di caricamento e il codice di join è disponibile, unisciti alla partita
        {
            panel.StartGameByLobby(lobby, true);
            JoinGame(joinCode);
        }
    }
    
    private async void JoinGame(string joinCode) //Metodo per i clients per unirsi alla partita utilizzando il codice di join
    {
        if (string.IsNullOrEmpty(joinCode) == false)
        {
            isJoining = true;
            PanelManager.Open("loading");
            try
            {   //Cerca di ottenere un'allocazione per il join del gioco
                JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
                var data = new RelayServerData(allocation, "dtls");
                transport.SetRelayServerData(data);

                //Imposta la sessione come Client
                SessionManager.role = SessionManager.Role.Client;
                SessionManager.joinCode = joinCode;
                SessionManager.lobbyID = lobby.Id;
                
                StartingSessionMenu panel = (StartingSessionMenu)PanelManager.GetSingleton("start");
                await UnsubscribeToEventsAsync();
                panel.StartGameConfirm();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                await Leave();
                isJoining = false;
                PanelManager.Close("start");
            }
            PanelManager.Close("loading");
        }
    }
    
    private void Update() //Metodo Update per il controllo periodico della lobby
    {
        if (lobby == null)
        {
            return;
        }
        
        if (isHost == false && isJoining == false)
        {
            CheckStartGameStatus();
        }
        
        if (lobby.HostId == AuthenticationService.Instance.PlayerId && sendingHeartbeat == false)
        {
            updateTimer += Time.deltaTime;
            if (updateTimer < heartbeatPeriod)
            {
                return;
            }
            updateTimer = 0;
            HeartbeatLobbyAsync();
        }
    }
    
    private async void HeartbeatLobbyAsync() //Metodo per inviare un "heartbeat" per mantenere attiva la connessione con la lobby
    {
        sendingHeartbeat = true;
        try
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        sendingHeartbeat = false;
    }
    
    public void Open(Lobby lobby) //Metodo per aprire la lobby con un determinato oggetto lobby
    {
        if (eventsLobbyId != lobby.Id)
        {
            _= SubscribeToEventsAsync(lobby.Id);
        }
        this.lobby = lobby;
        nameText.text = lobby.Name;
        CheckStartGameStatus();
        startButton.gameObject.SetActive(false);
        isHost = false;
        LoadPlayers();
        Open();
    }
    
    private void LoadPlayers() //Metodo per caricare la lista dei giocatori e aggiornare lo stato del pulsante "Start"
    {
        ClearPlayersList();
        bool isEveryoneReady = true;
        bool youAreMember = false;
        for (int i = 0; i < lobby.Players.Count; i++)
        {
            bool ready = lobby.Players[i].Data["ready"].Value == "1";
            LobbyPlayerItem item = Instantiate(lobbyPlayerItemPrefab, lobbyPlayersContainer);
            item.Initialize(lobby.Players[i], lobby.Id, lobby.HostId);
            if (lobby.Players[i].Id == AuthenticationService.Instance.PlayerId)
            {
                youAreMember = true;
                isReady = ready;
                isHost = lobby.Players[i].Id == lobby.HostId;
            }
            if (ready == false)
            {
                isEveryoneReady = false;
            }
        }
        startButton.gameObject.SetActive(isHost);
        if (isHost)
        {
            startButton.interactable = isEveryoneReady;
        }
        if (youAreMember == false)
        {
            Close();
        }
    }
    
    public async void CreateLobby(string lobbyName, int maxPlayers, bool isPrivate, string map, string language) //Metodo per creare una nuova lobby
    {
        PanelManager.Open("loading");
        try
        {
            CreateLobbyOptions options = new CreateLobbyOptions();
            options.IsPrivate = isPrivate;
            options.Data = new Dictionary<string, DataObject>()
            {
                { "map", new DataObject(visibility: DataObject.VisibilityOptions.Public, value: map) },
                { "language", new DataObject(visibility: DataObject.VisibilityOptions.Public, value: language) },
            };
            
            //Questi sono i dati specifici dell'utente che sta creando la lobby
            options.Player = new Player();
            options.Player.Data = new Dictionary<string, PlayerDataObject>();
            options.Player.Data.Add("name", new PlayerDataObject(visibility: PlayerDataObject.VisibilityOptions.Public, value: AuthenticationService.Instance.PlayerName));
            options.Player.Data.Add("ready", new PlayerDataObject(visibility: PlayerDataObject.VisibilityOptions.Public, value: "0"));

            lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            PanelManager.Close("lobby_search");
            Open(lobby);
        }
        catch (Exception e)
        {
            ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
            panel.Open(ErrorMenu.Action.None, "Failed to create the lobby.", "OK");
            Debug.LogException(e);
        }
        PanelManager.Close("loading");
    }
    
    public async void JoinLobby(string id) //Metodo per unirsi ad una lobby con un ID specifico
    {
        PanelManager.Open("loading");
        try
        {
            JoinLobbyByIdOptions options = new JoinLobbyByIdOptions();
            
            //Questi sono i dati dei giocatori che si stanno unendo alla lobby
            options.Player = new Player();
            options.Player.Data = new Dictionary<string, PlayerDataObject>();
            options.Player.Data.Add("name", new PlayerDataObject(visibility: PlayerDataObject.VisibilityOptions.Public, value: AuthenticationService.Instance.PlayerName));
            options.Player.Data.Add("ready", new PlayerDataObject(visibility: PlayerDataObject.VisibilityOptions.Public, value: "0"));
            
            lobby = await LobbyService.Instance.JoinLobbyByIdAsync(id, options);
            
            Open(lobby);
            PanelManager.Close("lobby_search");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        PanelManager.Close("loading");
    }
    
    public async void UpdateLobby(string lobbyId, string lobbyName, int maxPlayers, bool isPrivate, string map, string language) //Metodo per aggiornare una lobby esistente
    {
        PanelManager.Open("loading");
        try
        {
            UpdateLobbyOptions options = new UpdateLobbyOptions();
            options.IsPrivate = isPrivate;
            options.Name = lobbyName;
            options.MaxPlayers = maxPlayers;
            options.Data = new Dictionary<string, DataObject>()
            {
                { "map", new DataObject(visibility: DataObject.VisibilityOptions.Public, value: map) },
                { "language", new DataObject(visibility: DataObject.VisibilityOptions.Public, value: language) },
            };
            lobby = await LobbyService.Instance.UpdateLobbyAsync(lobbyId, options);
            Open(lobby);
        }
        catch (Exception)
        {
            ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
            panel.Open(ErrorMenu.Action.None, "Failed to change the lobby host.", "OK");
        }
        PanelManager.Close("loading");
    }
    
    private void LeaveLobby() //Metodo per abbandonare la lobby corrente
    {
        _= Leave();
    }
    
    private async Task Leave() //Metodo asincrono per lasciare la lobby
    {
        PanelManager.Open("loading");
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(lobby.Id, AuthenticationService.Instance.PlayerId);
            lobby = null;
            Close();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        PanelManager.Close("loading");
    }
    
    private async void SwitchReady() //Metodo per cambiare lo stato di prontezza del giocatore
    {
        readyButton.interactable = false;
        try
        {
            UpdatePlayerOptions options = new UpdatePlayerOptions();
            options.Data = new Dictionary<string, PlayerDataObject>();
            options.Data.Add("ready", new PlayerDataObject(visibility: PlayerDataObject.VisibilityOptions.Public, value: isReady ? "0" : "1"));
            lobby = await LobbyService.Instance.UpdatePlayerAsync(lobby.Id, AuthenticationService.Instance.PlayerId, options);
            LoadPlayers();
        }
        catch (Exception e)
        {
            Debug.LogException(e); 
        }
        readyButton.interactable = true;
    }
    
    private void ClearPlayersList() //Metodo per pulire la lista dei giocatori visualizzati
    {
        LobbyPlayerItem[] items = lobbyPlayersContainer.GetComponentsInChildren<LobbyPlayerItem>();
        if (items != null)
        {
            for (int i = 0; i < items.Length; i++)
            {
                Destroy(items[i].gameObject);
            }
        }
    }
    
    private void ClosePanel() //Metodo per chiudere il pannello
    {
        Close();
    }
    
    private async Task<bool> SubscribeToEventsAsync(string id) //Metodo per sottoscriversi agli eventi della lobby
    {
        try 
        {
            var callbacks = new LobbyEventCallbacks();
            callbacks.LobbyChanged += OnChanged;
            callbacks.LobbyEventConnectionStateChanged += OnConnectionChanged;
            callbacks.KickedFromLobby += OnKicked;
            events = await Lobbies.Instance.SubscribeToLobbyEventsAsync(id, callbacks);
            eventsLobbyId = lobby.Id;
            return true;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        return false;
    }
    
    private async Task UnsubscribeToEventsAsync() //Metodo per annullare la sottoscrizione dagli eventi della lobby
    {
        try
        {
            if (events != null)
            {
                await events.UnsubscribeAsync();
                events = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    
    private void OnKicked() //Metodo che gestisce l'espulsione dalla lobby
    {
        if (IsOpen)
        {
            Close();
        }
        lobby = null;
        events = null;
        isStarted = false;
        isJoining = false;
    }
    
    private void OnChanged(ILobbyChanges changes) //Metodo che gestisce i cambiamenti alla lobby
    {
        if (changes.LobbyDeleted)
        {
            if (IsOpen)
            {
                Close();
            }
            lobby = null;
            events = null;
            isStarted = false;
            isJoining = false;
        }
        else
        {
            changes.ApplyToLobby(lobby);
            CheckStartGameStatus();
            if (IsOpen)
            {
                LoadPlayers();
            }
        }
    }

    private void OnConnectionChanged(LobbyEventConnectionState state) // Metodo che gestisce i cambiamenti di stato della connessione
    {
        switch (state)
        {
            case LobbyEventConnectionState.Unsubscribed:
                break;
            case LobbyEventConnectionState.Error:
                if (lobby != null)
                {

                }
                break;
            case LobbyEventConnectionState.Subscribing:
                break;
            case LobbyEventConnectionState.Subscribed:
                break;
            case LobbyEventConnectionState.Unsynced:
                break;

        }
    }


}