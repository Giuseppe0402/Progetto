using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

public class LobbyListItem : MonoBehaviour
{
    //Riferimenti agli oggetti UI per visualizzare i dati della lobby
    [SerializeField] public TextMeshProUGUI nameText = null;
    [SerializeField] public TextMeshProUGUI playersText = null;
    [SerializeField] public TextMeshProUGUI hostText = null;
    [SerializeField] public TextMeshProUGUI mapText = null;
    [SerializeField] public TextMeshProUGUI languageText = null;
    [SerializeField] private Button joinButton = null;

    private Lobby lobby = null; //Riferimento alla lobby attuale

    private void Start() //Metodo chiamato all'avvio dell'oggetto, aggiunge il listener al pulsante "Join"
    {
        joinButton.onClick.AddListener(Join);
    }
    
    public void Initialize(Lobby lobby) //Metodo per inizializzare l'oggetto con i dati di una lobby
    {
        this.lobby = lobby;
        nameText.text = lobby.Name;
        playersText.text = lobby.Players.Count.ToString() + "/" + lobby.MaxPlayers.ToString();
        for (int i = 0; i < lobby.Players.Count; i++) //Cerca il nome dell'host confrontando gli ID dei giocatori con l'ID dell'host
        {
            if (lobby.Players[i].Id == lobby.HostId)
            {
                hostText.text = lobby.Players[i].Data["name"].Value;
                break;
            }
        }
        mapText.text = lobby.Data["map"].Value;
        languageText.text = lobby.Data["language"].Value;
    }

    private void Join() //Metodo chiamato quando l'utente preme il pulsante "Join"
    {
        LobbyMenu panel = (LobbyMenu)PanelManager.GetSingleton("lobby");
        panel.JoinLobby(lobby.Id);
    }
    
}