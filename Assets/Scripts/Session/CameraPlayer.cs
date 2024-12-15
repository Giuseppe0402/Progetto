using Unity.Netcode;
using UnityEngine;

public class CameraPlayer : MonoBehaviour
{
    [SerializeField] private Transform player; // Riferimento al giocatore
    private float sensibilità = 100f;
    private float rotazione;

    private Vector3 cameraOffset = new Vector3(0, 1.7f, 0); // Altezza della telecamera rispetto al giocatore

    private void Start()
    {
        if (!GetComponentInParent<NetworkBehaviour>().IsOwner)
        {
            gameObject.SetActive(false);
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Input per la rotazione
        float x = Input.GetAxis("Mouse X") * Time.deltaTime * sensibilità;
        float y = Input.GetAxis("Mouse Y") * Time.deltaTime * sensibilità;

        rotazione -= y;
        rotazione = Mathf.Clamp(rotazione, -60f, 60f);

        // Ruota la telecamera
        transform.localRotation = Quaternion.Euler(rotazione, 0, 0);
        player.Rotate(Vector3.up * x);

        // Aggiorna la posizione della telecamera in modo stabile
        Vector3 targetPosition = player.position + player.TransformDirection(cameraOffset);
        transform.position = targetPosition;
    }
}
