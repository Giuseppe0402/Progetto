using UnityEngine;
using System.Collections; // Necessario per IEnumerator

public class JailWallController : MonoBehaviour
{
    // Riferimento all'Animator del JailWall
    private Animator animator;

    void Start()
    {
        // Otteniamo il componente Animator
        animator = GetComponent<Animator>();

        // Verifica che l'Animator sia valido
        if (animator == null)
        {
            Debug.LogError("Animator non trovato sull'oggetto.");
            return;
        }

        // Iniziamo l'animazione con "SbarreAlzate"
        animator.Play("SbarreAlzate");

        // Avviamo la sequenza di animazioni
        StartCoroutine(PlayAnimationsSequence());
    }

    private IEnumerator PlayAnimationsSequence()
    {
        // Aspettiamo che l'animazione "SbarreAlzate" termini
        yield return new WaitForSeconds(GetClipLength("SbarreAlzate"));

        // Riproduciamo l'animazione "SbarreAbbassate"
        animator.Play("SbarreAbbassate");

        // Aspettiamo che l'animazione "SbarreAbbassate" termini
        yield return new WaitForSeconds(GetClipLength("SbarreAbbassate"));

        // Passiamo all'ultimo stato "SbarreGiu" e rimaniamo lì
        animator.Play("SbarreGiu");
    }

    // Funzione per ottenere la durata di una clip di animazione
    private float GetClipLength(string clipName)
    {
        // Otteniamo l'array delle clip dall'AnimatorController
        RuntimeAnimatorController controller = animator.runtimeAnimatorController;
        if (controller == null)
        {
            Debug.LogError("RuntimeAnimatorController non configurato per l'Animator.");
            return 0f;
        }

        foreach (AnimationClip clip in controller.animationClips)
        {
            if (clip.name == clipName)
            {
                return clip.length; // Restituisce la lunghezza della clip
            }
        }

        Debug.LogError($"Clip di animazione '{clipName}' non trovata.");
        return 0f; // Restituisce 0 se la clip non è trovata
    }
}

