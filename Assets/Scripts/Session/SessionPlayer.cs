using Unity.Netcode;
using UnityEngine;

public class SessionPlayer : NetworkBehaviour
{
    //Variabili serializzate per impostare la velocità di movimento del giocatore e il renderer del mesh.
    [SerializeField] private float moveSpeed = 2;
    [SerializeField] private MeshRenderer meshRenderer = null;

    //Variabili private per la gestione del controller del personaggio, dell'ID e del colore del giocatore.
    private CharacterController controller = null;
    private string _colorHex = "";
    private string _id = "";
    
    private Rigidboy characterRigidbody;
    private PlayerInput playerInput;
    prvate PlayerInputActions playerInputActions;

    private void Awake() //Metodo chiamato all'inizio della vita dell'oggetto per inizializzare il controller.
    {
        characterRigidbody= GetComponent<Rigidboy>();
        playerInput= GetComponent<PlayerInput>();

        playerInputActions = new PlayerInputActions();
        PlayerInputActions.Player.Enable();
        PlayerInputActions.Player.Jump.performed += Jump; //buffer
        PlayerInputActions.Player.Movement.performed += Movement_performed; //buffer

        //controller = GetComponent<CharacterController>();
    }

    private void FixedUpdate() //metodo per continuare il movimento premendo un solo tasto
    {
        Vector2 inputVector= PlayerInputActions.Player.Movement.ReadValue<Vector2>();
        float speed = 5f;
        characterRigidbody.AddForce(new Vector3(inputVector.x, 0, inputVectory.y)*speed, ForceMode.Force);
    }

    private void Movement_performed (InputAction.CallbackContext context)
    {
        Debug.Log(context);
        Vector2 inputVector= context.ReadValue<Vector2>(); //vector2 perchè legge il movimento che avviene solo su 2 dimensioni
        float speed = 5f;
        characterRigidbody.AddForce(new Vector3(inputVector.x, 0, inputVectory.y)*speed, ForceMode.Force); //lo "0" probabilmente è l'asse z
    }

    private void Jump(InputAction.CallbackContext context) //da mettere in isOwner
    {
        Debug.Log(context);
        if(context.performed)
        {
           Debug.Log("Jump!" + context.phase) //messaggio di debug in console
           characterRigidbody.AddForce(Vector3.up *5f, ForceMode.impulse)
        }
    }
    

    /*private void Update() //Metodo chiamato ad ogni frame per gestire il movimento del giocatore.
    {
        PlayerInputActions
        if (IsOwner)
        {
            Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            controller.Move(new Vector3(moveInput.x, 0, moveInput.y) * Time.deltaTime * moveSpeed);
        }
    }*/
    

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