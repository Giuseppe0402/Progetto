using UnityEngine;
using Unity.Services.Friends;
using Unity.Services.Friends.Models;
using TMPro;
using UnityEngine.UI;
using System;


public class FriendsList : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText = null; public TextMeshProUGUI NameText { get { return nameText; } }
    [SerializeField] private Button removeButton = null;

    private string id = "";
    private string memberId = "";

    private void Start()
    {
        removeButton.onClick.AddListener(RemoveFriend);
    }

    public void Initialize(Relationship relationship)
    {
        memberId = relationship.Member.Id;
        id = relationship.Id;
        NameText.text = relationship.Member.Profile.Name;
    }

    private async void RemoveFriend()
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
            panel.Open(ErrorMenu.Action.None, "Operazione fallita.", "Riprova");
        }
    }
}
