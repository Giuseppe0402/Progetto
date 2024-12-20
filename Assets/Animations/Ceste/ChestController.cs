using UnityEngine;

public class ChestController : MonoBehaviour
{
    private Animator animator;
    private bool isOpened = false; // Stato della cesta

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
        // Rileva l'interazione con il tasto "E"
        if (Input.GetKeyDown(KeyCode.E) && !isOpened)
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        if (animator != null)
        {
            animator.Play("ChestOpening"); // Avvia l'animazione di apertura
            isOpened = true; // Imposta lo stato come aperto
            // Imposta la transizione all'animazione "ChestOpened" dopo l'apertura
            Invoke("SetChestOpened", animator.GetCurrentAnimatorStateInfo(0).length);
        }
    }

    private void SetChestOpened()
    {
        if (animator != null)
        {
            animator.Play("ChestOpened"); // Passa all'animazione di "aperto" persistente
        }
    }
}
