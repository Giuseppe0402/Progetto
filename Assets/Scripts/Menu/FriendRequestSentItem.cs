using UnityEngine;
using TMPro;
using Unity.Services.Friends;
using Unity.Services.Friends.Models;
using UnityEngine.UI;
using System;

public class FriendRequestSentItem : MonoBehaviour
{
    //Riferimenti agli oggetti UI
    [SerializeField] public TextMeshProUGUI nameText = null;
    [SerializeField] private Button deleteButton = null;

    //Variabili private per memorizzare gli ID della relazione.
    private string id = "";
    private string memberId = "";
    
    private void Start() //Metodo che viene chiamato all'avvio del GameObject e aggiunge un listener al bottone corrispondente.
    {
        deleteButton.onClick.AddListener(DeleteRequest);
    }
    
    public void Initialize(Relationship relationship) //Metodo che inizializza l'elemento della lista con i dati di una richiesta di amicizia inviata.
    {
        memberId = relationship.Member.Id;
        id = relationship.Id;
        nameText.text = relationship.Member.Profile.Name;
    }
    
    private async void DeleteRequest() //Metodo asincrono per eliminare una richiesta di amicizia inviata.
    {
        deleteButton.interactable = false;
        try
        {
            await FriendsService.Instance.DeleteOutgoingFriendRequestAsync(id);
            Destroy(gameObject);
        }
        catch (Exception)
        {
            deleteButton.interactable = true;
            ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
            panel.Open(ErrorMenu.Action.None, "Failed to delete friend request.", "OK");
        }
    }
    
}