using UnityEngine;

public class Trigger : MonoBehaviour
{
    [SerializeField] private bool activatesTimer = false; // Specifica se questo trigger attiva il timer
    [SerializeField] private GameObject porta;
    [SerializeField] private AudioSource audioSource; // Componente audio
    private bool hasPlayedAudio = false; // Variabile per tracciare se il suono è già stato riprodotto
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Assicurati che il giocatore abbia il tag "Player"
        {
            if (activatesTimer) // Controlla se questo trigger è configurato per attivare il timer
            {
                Timer timer = FindFirstObjectByType<Timer>(); // Trova lo script del timer nella scena
                if (timer != null)
                {
                    timer.ActivateTimer(); // Attiva il timer
                }
            }

            porta.SetActive(true);
            InteractableObject interactable = porta.GetComponent<InteractableObject>();
            if (interactable != null)
            {
                interactable.SetInteractable(false); // Disabilita le interazioni
            }

            if (audioSource != null && !audioSource.isPlaying && !hasPlayedAudio)
            {
                audioSource.Play();
                hasPlayedAudio = true; // Imposta la variabile per non riprodurre più il suono
            }
        }
    }
}