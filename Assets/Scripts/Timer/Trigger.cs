using UnityEngine;

public class Trigger : MonoBehaviour
{
    [SerializeField] private bool activatesTimer = false; // Specifica se questo trigger attiva il timer
    [SerializeField] private GameObject porta;
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
        }
    }
}