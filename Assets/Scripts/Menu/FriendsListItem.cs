using UnityEngine;
using TMPro;
using Unity.Services.Friends;
using Unity.Services.Friends.Models;
using UnityEngine.UI;
using System;

public class FriendsListItem : MonoBehaviour
{
    //Riferimenti agli oggetti UI
    [SerializeField] private TextMeshProUGUI nameText = null; public TextMeshProUGUI NameText { get { return nameText; } }
    [SerializeField] private Button removeButton = null;

    //Variabili private per memorizzare gli ID della relazione.
    private string id = ""; //Id della relazione
    private string memberId = ""; //Id dell'utente
    
    private void Start() //Metodo che viene chiamato all'avvio del GameObject e aggiunge un listener al bottone corrispondente.
    {
        removeButton.onClick.AddListener(RemoveFriend);
    }
    
    public void Initialize(Relationship relationship) //Metodo che inizializza l'elemento della lista con i dati di una relazione.
    {
        memberId = relationship.Member.Id;
        id = relationship.Id;
        NameText.text = relationship.Member.Profile.Name;
    }
    
    private async void RemoveFriend() //Metodo asincrono per rimuovere un amico dalla lista.
    {
        removeButton.interactable = false;
        try
        {
            await FriendsService.Instance.DeleteRelationshipAsync(id);
            Destroy(gameObject);
        }
        catch (Exception)
        {
            removeButton.interactable = true;
            ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
            panel.Open(ErrorMenu.Action.None, "Failed to remove friend.", "OK");
        }
    }
    
}