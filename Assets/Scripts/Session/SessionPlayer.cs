using Unity.Netcode;
using UnityEngine;

public class SessionPlayer : NetworkBehaviour
{
<<<<<<< Updated upstream
    //Variabili serializzate per impostare la velocità di movimento del giocatore e il renderer del mesh.
=======
    //Variabili serializzate per impostare la velocit� di movimento del giocatore e il renderer del mesh.
>>>>>>> Stashed changes
    [SerializeField] private float moveSpeed = 2;
    [SerializeField] private MeshRenderer meshRenderer = null;

    //Variabili private per la gestione del controller del personaggio, dell'ID e del colore del giocatore.
    CharacterController controller;
    private string _colorHex = "";
    private string _id = "";
<<<<<<< Updated upstream
    
    private Rigidboy characterRigidbody;
    private PlayerInput playerInput;
    prvate PlayerInputActions playerInputActions;
=======
    float velocità;
    float velocità_walk = 5f;
    float velocità_run = 10f;
    public Transform TerraCheck;
    float distanzaTerra = 1f;
    public LayerMask TerraMask; //componente che ci dice quando il player tocca il terreno (momento nel quale "disattivo" la forza di gravità)
    bool toccaterra;
    Vector3 velocitày;
    float gravità = -9.8f;
    float altezzasalto = 1f;
>>>>>>> Stashed changes

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
            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");
            Vector3 movimento = transform.right * x + transform.forward * z; //direzione che daremo al player
 
            if(movimento!=Vector3.zero)
            {
                velocità = velocità_walk;

                if(Input.GetKey(KeyCode.LeftShift))
                {
                    velocità = velocità_run;
                } 
           
                controller.Move(movimento * Time.deltaTime * velocità);
            } 
            
            toccaterra = Physics.CheckSphere(TerraCheck.position, distanzaTerra, TerraMask); //funzione che ritorna "true" se collide altrimenti "false"    
            
            if(toccaterra && velocitày.y < 0)
            {
                velocitày.y = -10f;
            } 
       
            if(Input.GetButtonDown("Jump") && toccaterra)
            {
                velocitày.y = Mathf.Sqrt(gravità * altezzasalto * -2f);
            }
       
            velocitày.y += gravità * Time.deltaTime;
            controller.Move(velocitày * Time.deltaTime);
        }
<<<<<<< Updated upstream
    }*/
=======

    }
>>>>>>> Stashed changes
    

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