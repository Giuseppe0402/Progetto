using UnityEngine;
using TMPro;
using Unity.Services.Friends;
using Unity.Services.Friends.Models;
using UnityEngine.UI;
using System;

public class FriendRequestReceivedItem : MonoBehaviour
{
    //Riferimenti agli oggetti UI
    [SerializeField] private TextMeshProUGUI nameText = null; public TextMeshProUGUI NameText { get { return nameText; } }
    [SerializeField] private Button acceptButton = null;
    [SerializeField] private Button rejectButton = null;

    //Variabili private per memorizzare gli ID della relazione.
    private string id = "";
    private string memberId = "";

    private void Start() //Metodo chiamato all'avvio del GameObject e aggiunge i listener ai bottoni corrispondenti
    {
        acceptButton.onClick.AddListener(Accept);
        rejectButton.onClick.AddListener(Reject);
    }
    
    public void Initialize(Relationship relationship) //Metodo che inizializza l'elemento della lista con i dati di una richiesta di amicizia ricevuta.
    {
        memberId = relationship.Member.Id;
        id = relationship.Id;
        NameText.text = relationship.Member.Profile.Name;
    }
    
    private async void Accept() //Metodo asincrono per accettare una richiesta di amicizia.
    {
        acceptButton.interactable = false;
        rejectButton.interactable = false;
        try
        {
            await FriendsService.Instance.AddFriendAsync(memberId);
            Destroy(gameObject);
        }
        catch (Exception)
        {
            acceptButton.interactable = true;
            rejectButton.interactable = true;
            ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
            panel.Open(ErrorMenu.Action.None, "Failed to accept the request.", "OK");
        }
    }
    
    private async void Reject() //Metodo asincrono per rifiutare una richiesta di amicizia.
    {
        acceptButton.interactable = false;
        rejectButton.interactable = false;
        try
        {
            await FriendsService.Instance.DeleteIncomingFriendRequestAsync(memberId);
            Destroy(gameObject);
        }
        catch (Exception)
        {
            acceptButton.interactable = true;
            rejectButton.interactable = true;
            ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
            panel.Open(ErrorMenu.Action.None, "Failed to reject the request.", "OK");
        }
    }
    
}