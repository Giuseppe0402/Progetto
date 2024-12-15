using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

public class LobbyListItem : MonoBehaviour
{
    //Riferimenti agli oggetti UI per visualizzare i dati della lobby
    [SerializeField] private TextMeshProUGUI nameText = null; public TextMeshProUGUI NameText { get { return nameText; } }
    [SerializeField] private TextMeshProUGUI playersText = null; public TextMeshProUGUI PlayersText { get { return playersText; } }
    [SerializeField] private TextMeshProUGUI hostText = null; public TextMeshProUGUI HostText { get { return hostText; } }
    [SerializeField] private TextMeshProUGUI mapText = null; public TextMeshProUGUI MapText { get { return mapText; } }
    [SerializeField] private TextMeshProUGUI languageText = null; public TextMeshProUGUI LanguageText { get { return languageText; } }

    [SerializeField] private Button joinButton = null;

    private Lobby lobby = null; //Riferimento alla lobby attuale

    private void Start() //Metodo chiamato all'avvio dell'oggetto, aggiunge il listener al pulsante "Join"
    {
        joinButton.onClick.AddListener(Join);
    }
    
    public void Initialize(Lobby lobby) //Metodo per inizializzare l'oggetto con i dati di una lobby
    {
        this.lobby = lobby;
        NameText.text = lobby.Name;
        PlayersText.text = lobby.Players.Count.ToString() + "/" + lobby.MaxPlayers.ToString();
        for (int i = 0; i < lobby.Players.Count; i++) //Cerca il nome dell'host confrontando gli ID dei giocatori con l'ID dell'host
        {
            if (lobby.Players[i].Id == lobby.HostId)
            {
                HostText.text = lobby.Players[i].Data["name"].Value;
                break;
            }
        }
        MapText.text = lobby.Data["map"].Value;
        LanguageText.text = lobby.Data["language"].Value;
    }

    private void Join() //Metodo chiamato quando l'utente preme il pulsante "Join"
    {
        LobbyMenu panel = (LobbyMenu)PanelManager.GetSingleton("lobby");
        panel.JoinLobby(lobby.Id);
    }
    
}