using Unity.Netcode;
using UnityEngine;

public class InteractableObject : NetworkBehaviour, IInteractable
{
    [SerializeField] private Sprite itemIcon; // Icona per l'inventario
    [SerializeField] private string requiredCombination = "1712";
    [SerializeField] private bool requiresCombination = false;
    [SerializeField] private bool shouldDropSprite = false;
    private Outline outline; // Riferimento al componente Outline
    private bool isInteractable = true; // Flag per gestire l'interazione
    [SerializeField] private GameObject door; // La porta che deve essere aperta

    [SerializeField] private AudioClip interactionSound; // Suono per l'interazione

    private void Awake()
    {
        outline = GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = false; // Disabilita l'effetto all'inizio
        }
        else
        {
            Debug.LogWarning($"{gameObject.name} non ha un componente Outline.");
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Abilita l'effetto Outline
            SetOutline(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Disabilita l'effetto Outline
            SetOutline(false);
        }
    }

    private void SetOutline(bool enable)
    {
        if (outline != null)
        {
            outline.enabled = enable;
        }
    }

    public void Interact(SessionPlayer player)
    {
        if (!isInteractable) return; // Ignora se l'interazione non � permessa

        if (IsServer)
        {
            // Gestiamo direttamente l'interazione sul server
            HandleInteraction(player);
        }
        else
        {
            // Il client richiede l'interazione
            Debug.Log($"Client: Tentativo di interazione con {gameObject.name}");
            RequestInteractServerRpc(player.OwnerClientId);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    private void RequestInteractServerRpc(ulong clientId)
    {
        Debug.Log($"Server: Ricevuto richiesta di interazione dal client {clientId} con {gameObject.name}");

        // Verifica che il client sia connesso
        if (SessionManager.Singleton != null)
        {
            // Ottieni il PlayerObject associato al client
            var player = SessionManager.Singleton.GetPlayerByClientId(clientId);
            if (player != null)
            {
                Debug.Log($"Server: Interazione confermata con {gameObject.name} da parte del client {clientId}");
                HandleInteraction(player);
            }
            else
            {
                Debug.LogError($"Server: PlayerObject per il client {clientId} non trovato.");
            }
        }
        else
        {
            Debug.LogError($"Server: SessionManager non inizializzato.");
        }
    }

    private void HandleInteraction(SessionPlayer player)
    {
        if (!isInteractable) return;

        // Se la torcia viene interagita, fai sparire la porta
        if (door != null)
        {
            // Richiama una funzione server RPC per gestire la sincronizzazione
            ToggleDoorServerRpc(false); // Disabilita la porta
        }
        else
        {
            Debug.LogWarning("Nessuna porta associata all'oggetto.");
        }

        if (requiresCombination)
        {
            if (!string.IsNullOrEmpty(requiredCombination))
            {
                Debug.Log($"Mostra pannello combinazione per {player.name}");

                CombinationLock combinationLock = FindFirstObjectByType<CombinationLock>();
                if (combinationLock != null)
                {
                    combinationLock.CombinationCorrectEvent += OnCorrectCombination;
                    combinationLock.ShowPanelForClientRpc(player.OwnerClientId, requiredCombination, gameObject);
                }
                else
                {
                    Debug.LogError("CombinationPanel non trovato nella scena.");
                }
            }
            else
            {
                Debug.LogWarning($"L'oggetto {gameObject.name} richiede una combinazione, ma non � impostata.");
            }
        }
        else
        {
            Debug.Log($"{player.name} ha interagito con {gameObject.name} senza combinazione.");

            // Logica normale per aggiungere direttamente l'oggetto all'inventario
            Inventory inventory = FindFirstObjectByType<Inventory>();
            if (inventory != null && shouldDropSprite)
            {
                bool added = inventory.AddToInventory(gameObject, itemIcon);
                if (added)
                {
                    Debug.Log($"{gameObject.name} aggiunto all'inventario.");
                    NotifyClientInventoryUpdateClientRpc(player.OwnerClientId, gameObject.GetComponent<NetworkObject>().NetworkObjectId, itemIcon.name);
                }
                else
                {
                    Debug.LogWarning("Inventario pieno!");
                }
            }
            AudioManager.PlayClip(interactionSound);
            NotifyClientsObjectTakenClientRpc();
        }
    }


    private void OnCorrectCombination(GameObject target)
    {
        if (target == gameObject)
        {
            Debug.Log($"Combinazione corretta per {gameObject.name}");
            CombinationLock combinationPanel = FindFirstObjectByType<CombinationLock>();
            if (combinationPanel != null)
            {
                combinationPanel.CombinationCorrectEvent -= OnCorrectCombination;
            }

            // Aggiungi l'oggetto all'inventario
            Inventory inventory = FindFirstObjectByType<Inventory>();
            if (inventory != null)
            {
                bool added = inventory.AddToInventory(gameObject, itemIcon);
                if (added)
                {
                    Debug.Log($"{gameObject.name} aggiunto all'inventario.");
                    NotifyClientsObjectTakenClientRpc();
                }
                else
                {
                    Debug.LogWarning("Inventario pieno!");
                }
            }
        }
    }

    [ClientRpc]
    private void NotifyClientsObjectTakenClientRpc()
    {
        Debug.Log($"Oggetto {gameObject.name} rimosso dalla scena per tutti i client.");
        gameObject.SetActive(false);  // Nascondi l'oggetto per tutti
    }

    [ClientRpc]
    private void NotifyClientInventoryUpdateClientRpc(ulong targetClientId, ulong itemNetworkObjectId, string itemIconName)
    {
        if (NetworkManager.Singleton.LocalClientId == targetClientId) // Solo il client corretto esegue l'aggiornamento
        {
            NetworkObject itemNetworkObject = NetworkManager.Singleton.SpawnManager.SpawnedObjects[itemNetworkObjectId];
            if (itemNetworkObject != null)
            {
                GameObject item = itemNetworkObject.gameObject;
                Sprite itemIcon = Resources.Load<Sprite>(itemIconName);
                Inventory inventory = FindFirstObjectByType<Inventory>(); // Trova l'inventario personale
                if (inventory != null)
                {
                    inventory.AddToInventory(item, itemIcon);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ToggleDoorServerRpc(bool state)
    {
        // Aggiorna lo stato della porta sul server
        if (door != null)
        {
            door.SetActive(state);
            Debug.Log($"Server: Stato porta impostato su {state}.");

            // Notifica i client
            ToggleDoorClientRpc(state);
        }
        else
        {
            Debug.LogWarning("Server: Nessuna porta associata per sincronizzazione.");
        }
    }

    [ClientRpc]
    private void ToggleDoorClientRpc(bool state)
    {
        // Aggiorna lo stato della porta sui client
        if (door != null)
        {
            door.SetActive(state);
            Debug.Log($"Client: Stato porta sincronizzato su {state}.");
        }
        else
        {
            Debug.LogWarning("Client: Nessuna porta associata per aggiornamento.");
        }
    }

    public void SetInteractable(bool state)
    {
        isInteractable = state;
    }

}