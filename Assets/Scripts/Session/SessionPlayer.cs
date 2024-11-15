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
    
    private void Awake() //Metodo chiamato all'inizio della vita dell'oggetto per inizializzare il controller.
    {
        controller = GetComponent<CharacterController>();
    }
    
    private void Update() //Metodo chiamato ad ogni frame per gestire il movimento del giocatore.
    {
        if (IsOwner)
        {
            Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            controller.Move(new Vector3(moveInput.x, 0, moveInput.y) * Time.deltaTime * moveSpeed);
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