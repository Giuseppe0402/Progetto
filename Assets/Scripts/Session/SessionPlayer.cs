using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class SessionPlayer : NetworkBehaviour
{
    //Variabili serializzate per impostare la velocità di movimento del giocatore e il renderer del mesh.
    [SerializeField] private SkinnedMeshRenderer meshRenderer = null;
    [SerializeField] private GameObject canvasPrefab;  // Prefab del Canvas da assegnare dinamicamente
    private Canvas playerCanvas;  // Riferimento al Canvas del giocatore
    [SerializeField] private GameObject inventoryPrefab;  // Prefab dell'Inventario
    [SerializeField] private CharacterController characterController;   

    //Variabili private per la gestione del controller del personaggio, dell'ID e del colore del giocatore.
    private CharacterController controller = null;
    private string _colorHex = "";
    private string _id = "";

    private float velocita;
    private float velocitaWalk = 5f;
    private float velocitaRun = 10f;
    [SerializeField] private Transform TerraCheck;
    private float distanzaTerra = 1f;
    [SerializeField] private LayerMask TerraMask; //componente che ci dice quando il player tocca il terreno (momento nel quale "disattivo" la forza di gravità)
    private bool toccaTerra;
    private Vector3 velocitay;
    private float gravita = -9.8f;
    private float altezzaSalto = 1f;

    private void Awake() //Metodo chiamato all'inizio della vita dell'oggetto per inizializzare il controller.
    {
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        if (IsOwner)
        {
            CreatePlayerCanvas();
            playerCanvas.gameObject.SetActive(true);
            NotifyCombinationLock();  // Se serve per la logica del puzzle
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            HandleHorizontalMovement();
            HandleGravityAndJump();
            HandleInteraction();
        }
    }

    private void HandleHorizontalMovement()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 movimento = transform.right * x + transform.forward * z;
        if (movimento != Vector3.zero)
        {
            velocita = velocitaWalk;

            if (Input.GetKey(KeyCode.LeftShift))
            {
                velocita = velocitaRun;
            }

            characterController.Move(movimento * Time.deltaTime * velocita);
        }
    }

    private void HandleGravityAndJump()
    {
        toccaTerra = Physics.CheckSphere(TerraCheck.position, distanzaTerra, TerraMask); //funzione che ritorna "true" se collide altrimenti "false"    

        if (toccaTerra && velocitay.y < 0)
        {
            velocitay.y = -2f;
        }

        if (Input.GetButtonDown("Jump") && toccaTerra)
        {
            velocitay.y = Mathf.Sqrt(gravita * altezzaSalto * -2f);
        }

        velocitay.y += gravita * Time.deltaTime;
        controller.Move(velocitay * Time.deltaTime);
    }

    public void HandleInteraction()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            Ray ray = new Ray(transform.position + Vector3.up * 0.5f, transform.forward);
            if (Physics.Raycast(ray, out RaycastHit hit, 2f))
            {
                IInteractable interactable = hit.collider.GetComponent<IInteractable>();
                if (interactable != null)
                {
                    Debug.Log($"Tentativo di interazione con: {hit.collider.name}");
                    interactable.Interact(this);

                    CombinationLock combinationLock = hit.collider.GetComponent<CombinationLock>();
                    if (combinationLock != null)
                    {
                        Debug.Log("Chiamando ShowPanel");
                        combinationLock.ShowPanel("1234", hit.collider.gameObject);
                    }
                }
                else
                {
                    Debug.LogWarning("Nessun componente IInteractable trovato sull'oggetto colpito.");
                }
            }
            else
            {
                Debug.LogWarning("Raycast non ha colpito nulla.");
            }
        }
    }

    private void CreatePlayerCanvas()
    {
        if (canvasPrefab != null)
        {
            GameObject canvasObject = Instantiate(canvasPrefab, transform);
            playerCanvas = canvasObject.GetComponent<Canvas>();

            if (playerCanvas == null)
            {
                Debug.LogError("Canvas prefab non contiene un componente Canvas.");
                return;
            }

            playerCanvas.gameObject.SetActive(false);  // Disabilita fino a quando serve

            if (inventoryPrefab != null)
            {
                GameObject inventoryObject = Instantiate(inventoryPrefab, transform);
                Inventory inventory = inventoryObject.GetComponent<Inventory>();

                List<Image> inventorySlots = new List<Image>(playerCanvas.GetComponentsInChildren<Image>());
                inventory.InitializeInventory(inventorySlots);
            }
            else
            {
                Debug.LogError("Inventory Prefab non assegnato al SessionPlayer.");
            }
        }
        else
        {
            Debug.LogError("Canvas Prefab non assegnato al SessionPlayer.");
        }
    }

    private void NotifyCombinationLock()
    {
        CombinationLock combinationLock = FindFirstObjectByType<CombinationLock>();

        if (combinationLock != null)
        {
            // Trova dinamicamente il pannello, l'input e il bottone
            GameObject codePanel = playerCanvas.transform.Find("CodePanel")?.gameObject;
            TMP_InputField codeInput = codePanel?.transform.Find("CodeInput")?.GetComponent<TMP_InputField>();
            Button codeButton = codePanel?.transform.Find("CodeButton")?.GetComponent<Button>();

            // Verifica la presenza dei componenti necessari
            if (codePanel != null && codeInput != null && codeButton != null)
            {
                combinationLock.Panel = codePanel;  // Assegna il panel
                combinationLock.CombinationInput = codeInput;  // Assegna l'input
                combinationLock.ConfirmButton = codeButton;
                Debug.Log("Riferimenti di CombinationLock impostati dinamicamente.");
            }
            else
            {
                Debug.LogError("Uno o più componenti necessari non sono stati trovati in CodePanel.");
            }
        }
        else
        {
            Debug.LogError("CombinationLock non trovato.");
        }
    }


    [Rpc(SendTo.Everyone)] //Metodo chiamato tramite RPC per applicare l'ID e il colore del giocatore. Destinato a tutti i client.
    public void ApplyDataRpc(string id, string colorHex)
    {
        ColorUtility.TryParseHtmlString(colorHex, out Color color);
        meshRenderer.material.color = color;
        _colorHex = colorHex;
        _id = id;
    }
    
    public void ApplyDataRpc() //Metodo che applica i dati RPC utilizzando i valori correnti di `_id` e `_colorHex`.
    {
        ApplyDataRpc(_id, _colorHex);
    }

}