using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Friends;

public class FriendsMenu : Panel
{
    //Riferimenti agli oggetti UI
    [SerializeField] private FriendsListItem friendsListItemPrefab = null;
    [SerializeField] private FriendRequestReceivedItem friendRequestReceivedItemPrefab = null;
    [SerializeField] private FriendRequestSentItem friendRequestSentItemPrefab = null;
    [SerializeField] private RectTransform friendsListContainer = null;
    [SerializeField] private Button friendsButton = null;
    [SerializeField] private Button friendRequestsReceivedButton = null;
    [SerializeField] private Button friendRequestsSentButton = null;
    [SerializeField] private Button closeButton = null;

    public override void Initialize() //Metodo che inizializza il pannello.
    {
        if (IsInitialized)
        {
            return;
        }
        friendsButton.onClick.AddListener(LoadFriendsList);
        friendRequestsReceivedButton.onClick.AddListener(LoadReceivedFriendRequests);
        friendRequestsSentButton.onClick.AddListener(LoadSentFriendRequests);
        closeButton.onClick.AddListener(ClosePanel);
        ClearFriendsList();
        base.Initialize();
    }
    
    public override void Open() //Metodo che apre il pannello del menu amici.
    {
        base.Open();
        LoadFriendsList();
    }
    
    private void LoadFriendsList() //Metodo per caricare la lista degli amici.
    {
        friendsButton.interactable = false;
        friendRequestsReceivedButton.interactable = true;
        friendRequestsSentButton.interactable = true;
        if (FriendsService.Instance.Friends != null) //Se la lista degli amici non è vuota, crea un oggetto per ogni amico e lo aggiunge alla UI.
        {
            ClearFriendsList();
            for (int i = 0; i < FriendsService.Instance.Friends.Count; i++)
            {
                FriendsListItem item = Instantiate(friendsListItemPrefab, friendsListContainer);
                item.Initialize(FriendsService.Instance.Friends[i]);
            }
        }
    }

    private void LoadReceivedFriendRequests() //Metodo per caricare le richieste di amicizia ricevute.
    {
        friendsButton.interactable = true;
        friendRequestsReceivedButton.interactable = false;
        friendRequestsSentButton.interactable = true;
        ClearFriendsList();
        if (FriendsService.Instance.IncomingFriendRequests != null) //Se ci sono richieste di amicizia ricevute, crea un elemento per ciascuna.
        {
            for (int i = 0; i < FriendsService.Instance.IncomingFriendRequests.Count; i++)
            {
                FriendRequestReceivedItem receivedItem = Instantiate(friendRequestReceivedItemPrefab, friendsListContainer);
                receivedItem.Initialize(FriendsService.Instance.IncomingFriendRequests[i]);
            }
        }
    }

    private void LoadSentFriendRequests() //Metodo per caricare le richieste di amicizia inviate.
    {
        friendsButton.interactable = true;
        friendRequestsReceivedButton.interactable = true;
        friendRequestsSentButton.interactable = false;
        ClearFriendsList();
        if (FriendsService.Instance.OutgoingFriendRequests != null) //Se ci sono richieste di amicizia inviate, crea un elemento per ciascuna.
        {
            for (int i = 0; i < FriendsService.Instance.OutgoingFriendRequests.Count; i++)
            {
                FriendRequestSentItem receivedItem = Instantiate(friendRequestSentItemPrefab, friendsListContainer);
                receivedItem.Initialize(FriendsService.Instance.OutgoingFriendRequests[i]);
            }
        }
    }

    private void ClosePanel() //Metodo che chiude il pannello del menu amici.
    {
        Close();
    }
    
    private void ClearFriendsList() //Metodo che pulisce la lista degli amici, rimuovendo gli oggetti esistenti.
    {
        FriendsListItem[] items = friendsListContainer.GetComponentsInChildren<FriendsListItem>();
        if (items != null)
        {
            for (int i = 0; i < items.Length; i++)
            {
                Destroy(items[i].gameObject); //Rimuove tutti gli oggetti della lista amici.
            }
        }
        FriendRequestReceivedItem[] received = friendsListContainer.GetComponentsInChildren<FriendRequestReceivedItem>();
        if (received != null)
        {
            for (int i = 0; i < received.Length; i++)
            {
                Destroy(received[i].gameObject); //Rimuove tutte le richieste di amicizia ricevute.
            }
        }
        FriendRequestSentItem[] sent = friendsListContainer.GetComponentsInChildren<FriendRequestSentItem>();
        if (sent != null)
        {
            for (int i = 0; i < sent.Length; i++)
            {
                Destroy(sent[i].gameObject); //Rimuove tutte le richieste di amicizia inviate.
            }
        }
    }
    
}