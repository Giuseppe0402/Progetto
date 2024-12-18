using UnityEngine;
using Unity.Netcode;

public class Footsteps : NetworkBehaviour
{
    [SerializeField] private AudioSource footstepsSound;
    private bool isMoving = false;

    private void Start()
    {
        // Assicurati che il suono non venga riprodotto all'inizio
        footstepsSound.Stop();
    }

    private void Update()
    {
        // Esegui il codice solo per il giocatore locale
        if (!IsOwner) return;

        bool isMovingNow = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);

        if (isMovingNow && !isMoving)
        {
            isMoving = true;
            PlayFootstepsServerRpc();
        }
        else if (!isMovingNow && isMoving)
        {
            // Il giocatore si ferma
            isMoving = false;
            StopFootstepsServerRpc();
        }
    }

    [ServerRpc]
    private void PlayFootstepsServerRpc()
    {
        PlayFootstepsClientRpc(); // Notifica a tutti i client di riprodurre il suono
    }

    [ServerRpc]
    private void StopFootstepsServerRpc()
    {
        StopFootstepsClientRpc(); // Notifica a tutti i client di fermare il suono
    }

    [ClientRpc]
    private void PlayFootstepsClientRpc()
    {
        if (!footstepsSound.isPlaying)
        {
            footstepsSound.Play(); // Riproduce il suono dei passi
        }
    }

    [ClientRpc]
    private void StopFootstepsClientRpc()
    {
        if (footstepsSound.isPlaying)
        {
            footstepsSound.Stop(); // Ferma il suono dei passi
        }
    }

}
