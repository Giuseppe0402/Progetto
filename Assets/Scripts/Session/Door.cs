using UnityEngine;
using Unity.Netcode;

public class Door : NetworkBehaviour, IInteractable
{
    [SerializeField] private Sprite requiredItemSprite; // Sprite della chiave richiesta
    [SerializeField] private string requiredItemName = "Key"; // Nome della chiave

    [SerializeField] private AudioClip interactionSound; // Suono per l'interazione

    public void Interact(SessionPlayer player)
    {
        if (IsServer)
        {
            HandleInteraction(player);
        }
        else
        {
            Debug.Log($"Client: Tentativo di interazione con {gameObject.name}");
            RequestInteractServerRpc(player.OwnerClientId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestInteractServerRpc(ulong clientId)
    {
        Debug.Log($"Server: Ricevuto richiesta di interazione dal client {clientId} con {gameObject.name}");

        // Ottieni il PlayerObject associato al client
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            var player = client.PlayerObject.GetComponent<SessionPlayer>();
            if (player != null)
            {
                HandleInteraction(player);
            }
            else
            {
                Debug.LogError($"Server: PlayerObject per il client {clientId} non trovato.");
            }
        }
    }

    private void HandleInteraction(SessionPlayer player)
    {
        // Ottieni il componente Inventory del giocatore
        var inventory = player.GetComponentInChildren<Inventory>();
        if (inventory == null)
        {
            Debug.LogError("Il giocatore non ha un componente Inventory.");
            return;
        }

        // Controlla se il giocatore possiede l'oggetto richiesto
        if (inventory.HasItem(requiredItemSprite))
        {
            Debug.Log($"Giocatore {player.name} ha l'oggetto richiesto {requiredItemName}. Rimuovi la porta.");
            inventory.RemoveItem(requiredItemSprite); // Rimuovi l'oggetto dall'inventario
            AudioManager.PlayClip(interactionSound);
            NotifyClientsDoorOpenClientRpc(); // Notifica i client per rimuovere la porta
        }
        else
        {
            Debug.LogWarning($"Giocatore {player.name} non ha l'oggetto richiesto {requiredItemName}.");
        }
    }

    [ClientRpc]
    private void NotifyClientsDoorOpenClientRpc()
    {
        Debug.Log($"La porta {gameObject.name} si apre per tutti i client.");
        gameObject.SetActive(false); // Nascondi la porta per tutti
    }
}
