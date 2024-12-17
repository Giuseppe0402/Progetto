using System;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models.Data.Player;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionManager : NetworkBehaviour
{
    
    [SerializeField] private NetworkPrefabsList charactersPrefab = null; //Prefab dei personaggi da istanziare nella sessione
    public static Role role = Role.Client; //Ruolo del giocatore, che può essere Client, Host o Server
    public static string joinCode = ""; //Codice di join della lobby
    public static string lobbyID = ""; //ID della lobby
    private Dictionary<ulong, SessionPlayer> connectedPlayers = new Dictionary<ulong, SessionPlayer>();

    public enum Role //Enum per definire i ruoli nel gioco
    {
        Client = 1, Host = 2, Server = 3
    }
    
    private static SessionManager singleton = null; //Istanza della classe SessionManager
    public static SessionManager Singleton
    {
        get
        {
            if (singleton == null)
            {
                singleton = FindFirstObjectByType<SessionManager>();
                singleton.Initialize();
            }
            return singleton; 
        }
    }
    
    private bool initialized = false; //Flag per controllare se la sessione è già stata inizializzata

    private void Initialize() //Metodo di inizializzazione
    {
        if (initialized) { return; }
        initialized = true;
    }
    
    public override void OnDestroy() //Metodo chiamato quando l'oggetto viene distrutto
    {
        if (singleton == this)
        {
            singleton = null;
        }
        base.OnDestroy();
    }

    private void Start() //Metodo di avvio della sessione
    {
        Initialize();
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected; //Aggiunge il listener per i nuovi client connessi
        if (role == Role.Client) 
        {
            NetworkManager.Singleton.StartClient();
        }
        else if (role == Role.Host)
        {
            NetworkManager.Singleton.StartHost();
            if (string.IsNullOrEmpty(joinCode) == false && string.IsNullOrEmpty(lobbyID) == false)
            {
                SetLobbyJoinCode(joinCode);
            }
        }
        else
        {
            NetworkManager.Singleton.StartServer();
        }
    }
    
    private void OnClientConnected(ulong id) //Callback chiamata sull'host quando un client si connette al server
    {
        if (NetworkManager.Singleton.IsServer)
        {
            RpcParams rpcParams = NetworkManager.Singleton.RpcTarget.Single(id, RpcTargetUse.Temp);
            InitializeRpc(rpcParams);
        }

    }

    [Rpc(SendTo.SpecifiedInParams)] //Middleman tra host e client
    private void InitializeRpc(RpcParams rpcParams) //Metodo RPC che inizializza il client
    {
        InitializeClient();
    }
    
    private async void InitializeClient() //Metodo asincrono per inizializzare i dati del client
    {
        int character = 0;
        string color = "";
        try
        {   //Carica i dati del giocatore dal servizio di salvataggio cloud
            var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "character" }, new LoadOptions(new PublicReadAccessClassOptions()));
            if (playerData.TryGetValue("character", out var characterData))
            {
                var data = characterData.Value.GetAs<Dictionary<string, object>>();
                character = int.Parse(data["type"].ToString());
                color = data["color"].ToString();
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        //Chiama l'RPC per istanziare il personaggio del client
        InstantiateCharacterRpc(character, AuthenticationService.Instance.PlayerId, color);
    }

    //Metodo RPC che istanzia un personaggio nel gioco
    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void InstantiateCharacterRpc(int character, string id, string color, RpcParams rpcParams = default)
    {
        //Ottiene la posizione dello spawn point
        Vector3 position = SessionSpawnPoints.Singleton.GetSpawnPositionOrdered();
        //Ottiene il prefab del personaggio selezionato
        var prefab = charactersPrefab.PrefabList[character].Prefab.GetComponent<NetworkObject>();
        //Istanzia e spawna il personaggio nella rete
        var networkObject = NetworkManager.Singleton.SpawnManager.InstantiateAndSpawn(prefab, rpcParams.Receive.SenderClientId, true, true, false, position, quaternion.identity);

        SessionPlayer player = networkObject.GetComponent<SessionPlayer>();
        player.ApplyDataRpc(id, color);
        connectedPlayers[rpcParams.Receive.SenderClientId] = player;
        SessionPlayer[] players = FindObjectsByType<SessionPlayer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (players != null)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != player) //Evita di applicare i dati al proprio personaggio
                {
                    players[i].ApplyDataRpc(); //Applica i dati a tutti gli altri giocatori
                }
            }
        }
    }
    
    private async void SetLobbyJoinCode(string code) //Metodo asincrono per impostare il codice di join della lobby
    {
        try
        {   //Crea le opzioni per aggiornare i dati della lobby
            UpdateLobbyOptions options = new UpdateLobbyOptions();
            options.Data = new Dictionary<string, DataObject>();
            options.Data.Add("join_code", new DataObject(visibility: DataObject.VisibilityOptions.Public, value: code));
            var lobby = await LobbyService.Instance.UpdateLobbyAsync(lobbyID, options);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (connectedPlayers.ContainsKey(clientId))
        {
            connectedPlayers.Remove(clientId);
            Debug.Log($"Server: PlayerObject rimosso per il client {clientId}");
        }
    }

    public SessionPlayer GetPlayerByClientId(ulong clientId)
    {
        if (connectedPlayers.TryGetValue(clientId, out var player))
        {
            return player;
        }
        return null;
    }

    public Inventory GetInventoryByClientId(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            var player = client.PlayerObject?.GetComponent<SessionPlayer>();
            return player?.GetComponentInChildren<Inventory>();
        }
        return null;
    }

    public async void ExitSession()
    {
        try
        {
            // Verifica se l'utente è autenticato
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                Debug.LogError("Utente non autenticato.");
                return;
            }

            string playerId = AuthenticationService.Instance.PlayerId;

            // Controlla se c'è un lobby ID valido
            if (!string.IsNullOrEmpty(lobbyID))  // Usa il lobbyID che hai già
            {
                // Rimuovi il giocatore dalla lobby se l'ID è valido
                await LobbyService.Instance.RemovePlayerAsync(lobbyID, playerId);
            }
            else
            {
                Debug.LogWarning("Nessun lobby attivo o valido per rimuovere il giocatore.");
            }

            // Chiudi la connessione
            NetworkManager.Singleton.Shutdown();

            // Disconnetti l'utente se è connesso
            if (AuthenticationService.Instance.IsSignedIn)
            {
                AuthenticationService.Instance.SignOut();
            }

            // Carica la scena di autenticazione o menu
            SceneManager.LoadScene("Menu");

        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Errore durante la rimozione dalla lobby: " + e);
        }
        catch (Exception e)
        {
            Debug.LogError("Errore durante l'uscita dalla sessione: " + e);
        }
    }

}