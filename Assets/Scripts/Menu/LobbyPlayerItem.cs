using System;
using UnityEngine;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

public class LobbyPlayerItem : MonoBehaviour
{
    //Riferimenti agli oggetti UI per visualizzare i dettagli del giocatore
    [SerializeField] public TextMeshProUGUI nameText = null;
    [SerializeField] public TextMeshProUGUI roleText = null;
    [SerializeField] public TextMeshProUGUI statusText = null;
    [SerializeField] private Button kickButton = null;
    [SerializeField] private Button selectButton = null;

    private string lobbyId = ""; //ID della lobby a cui appartiene il giocatore
    private Player player = null; //Riferimento al giocatore

    private void Start() //Metodo chiamato all'inizio per configurare i pulsanti
    {
        kickButton.onClick.AddListener(Kick);
        selectButton.onClick.AddListener(Clicked);
    }

    public void Initialize(Player player, string lobbyId, string hostId) //Metodo per inizializzare l'oggetto con i dati del giocatore e della lobby
    {
        this.player = player;
        this.lobbyId = lobbyId;
        nameText.text = player.Data["name"].Value;
        roleText.text = player.Id == hostId ? "Host" : "Member";
        bool isReady = player.Data["ready"].Value == "1";
        statusText.text = isReady ? "Ready" : "Not Ready";
        kickButton.gameObject.SetActive(player.Id != hostId && AuthenticationService.Instance.PlayerId == hostId);
        //Il pulsante di espulsione è visibile solo se il giocatore non è l'host e l'utente corrente è l'host
    }

    private async void Kick() //Metodo chiamato quando il pulsante "Kick" viene premuto
    {
        kickButton.interactable = false;
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(lobbyId, player.Id);
            Destroy(gameObject);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        kickButton.interactable = true;
    }

    private void Clicked() //Metodo chiamato quando il pulsante "Select" viene premuto
    {
        PlayerProfileMenu panel = (PlayerProfileMenu)PanelManager.GetSingleton("player_profile");
        panel.Open(player.Id, player.Data["name"].Value);
    }

}