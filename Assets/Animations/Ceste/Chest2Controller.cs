using UnityEngine;

public class Chest2Controller : MonoBehaviour
{
    private Animator animator;
    private bool isOpened = false; // Stato della cesta
    private bool playerNearby = false; // Controlla se il giocatore è vicino

    void Start()
    {
        // Otteniamo il componente Animator associato alla cesta
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator non trovato sul GameObject. Assicurati che il componente Animator sia presente.");
        }
    }

    void Update()
    {
        // Rileva l'interazione con il tasto "E" solo se il giocatore è vicino e la cesta non è ancora aperta
        if (Input.GetKeyDown(KeyCode.E) && playerNearby && !isOpened)
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        if (animator != null)
        {
            animator.Play("Chest2Opening"); // Avvia l'animazione di apertura
            isOpened = true; // Imposta lo stato come aperto
            // Imposta la transizione all'animazione "ChestOpened" dopo l'apertura
            Invoke("SetChestOpened", animator.GetCurrentAnimatorStateInfo(0).length);
        }
    }

    private void SetChestOpened()
    {
        if (animator != null)
        {
            animator.Play("Chest2Opened"); // Passa all'animazione di "aperto" persistente
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Controlla che il giocatore sia entrato nel trigger
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) // Controlla che il giocatore sia uscito dal trigger
        {
            playerNearby = false;
        }
    }
}

