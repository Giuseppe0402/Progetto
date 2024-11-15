using System;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Friends;
using TMPro;
using UnityEngine.UI;

public class PlayerProfileMenu : Panel
{
    //Riferimenti agli elementi UI per la gestione del profilo del giocatore.
    [SerializeField] private TextMeshProUGUI nameText = null;
    [SerializeField] private Button closeButton = null;
    [SerializeField] private Button addFriendButton = null;
    [SerializeField] private Button removeFriendButton = null;
     
    private string _id = null; //Variabile per memorizzare l'ID del giocatore che si sta visualizzando.

    public override void Initialize() //Metodo di inizializzazione del pannello.
    {
        if (IsInitialized)
        {
            return;
        }
        closeButton.onClick.AddListener(ClosePanel);
        addFriendButton.onClick.AddListener(AddFriend);
        removeFriendButton.onClick.AddListener(RemoveFriend);
        base.Initialize();
    }
    
    public override void Open() //Metodo che apre il pannello del profilo del giocatore.
    {
        HideAllButtons();
        base.Open();
        if (string.IsNullOrEmpty(_id))
        {
            _id = AuthenticationService.Instance.PlayerId;
        }
        nameText.text = "";
    }

    public void Open(string playerId, string playerName) //Metodo che apre il pannello con l'ID e il nome di un giocatore specifico.
    {
        _id = playerId;
        Open();
        SetupUI(playerId, playerName); 
    }
    
    private void SetupUI(string id, string playerName) //Metodo che imposta l'interfaccia utente con i dettagli del giocatore.
    {
        nameText.text = playerName;
        if (id != AuthenticationService.Instance.PlayerId)
        {
            bool isFriend = IsFriend(id); //Verifica se il giocatore è già nella lista amici.
            addFriendButton.gameObject.SetActive(isFriend == false); //Mostra il bottone per aggiungere l'amico se non è già un amico.
            addFriendButton.interactable = IsSentFriendRequest(id) == false; //Disabilita il bottone se la richiesta di amicizia è già stata inviata.
            removeFriendButton.gameObject.SetActive(isFriend); //Mostra il bottone per rimuovere l'amico se è già un amico.
        }
    }
    
    private async void AddFriend() //Metodo asincrono per aggiungere un amico.
    {
        addFriendButton.interactable = false;
        try
        {
            await FriendsService.Instance.AddFriendAsync(_id);
            ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
            panel.Open(ErrorMenu.Action.None, "Richiesta di amicizia inviata con successo.", "OK");
        }
        catch (Exception e)
        {
            ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
            panel.Open(ErrorMenu.Action.None, "Errore nell'invio della richiesta di amicizia.", "OK");
            Debug.LogException(e);
            addFriendButton.interactable = true;
        }
    }

    private async void RemoveFriend() //Metodo asincrono per rimuovere un amico.
    {
        removeFriendButton.interactable = false;
        try
        {
            await FriendsService.Instance.DeleteFriendAsync(_id);
            addFriendButton.gameObject.SetActive(true);
            removeFriendButton.gameObject.SetActive(false);
            ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
            panel.Open(ErrorMenu.Action.None, "Giocatore rimosso dalla tua lista di amici correttamente.", "OK");
        }
        catch (Exception)
        {
            ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
            panel.Open(ErrorMenu.Action.None, "Errore nella rimozione del giocatore dalla tua lista di amici.", "OK");
        }
        removeFriendButton.interactable = true;
    }
    
    private bool IsFriend(string id) //Metodo che verifica se il giocatore è nella lista amici.
    {
        for (int i = 0; i < FriendsService.Instance.Friends.Count; i++)
        {
            if (FriendsService.Instance.Friends[i].Member.Id == id)
            {
                return true;
            }
        }
        return false;
    }
    
    private bool IsSentFriendRequest(string id) //Metodo che verifica se una richiesta di amicizia è stata inviata a un giocatore.
    {
        for (int i = 0; i < FriendsService.Instance.OutgoingFriendRequests.Count; i++)
        {
            if (FriendsService.Instance.OutgoingFriendRequests[i].Member.Id == id)
            {
                return true;
            }
        }
        return false;
    }
    
    private void ClosePanel() //Metodo che chiude il pannello del profilo e resetta l'ID.
    {
        Close();
        _id = null;
    }
    
    private void HideAllButtons() //Metodo che nasconde tutti i bottoni del pannello.
    {
        addFriendButton.gameObject.SetActive(false);
        removeFriendButton.gameObject.SetActive(false);
        removeFriendButton.interactable = true;
        addFriendButton.interactable = true;
    }

}