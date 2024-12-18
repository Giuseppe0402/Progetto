using Unity.Netcode;
using UnityEngine;
using TMPro;

public class Timer : NetworkBehaviour
{
    public float timeLimit = 300f; // Tempo di inizio in secondi (5 minuti)
    private float timeRemaining;
    public TextMeshProUGUI timerText; // Riferimento al testo del timer (ui globale)

    // Variabile di rete per sincronizzare il tempo
    private NetworkVariable<float> networkTime = new NetworkVariable<float>(300f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private bool timerActive = false; // Flag per verificare se il timer è attivo

    void Start()
    {
        if (IsServer) // Solo il server gestisce il timer
        {
            timeRemaining = timeLimit;
        }
    }

    void Update()
    {
        if (IsServer && timerActive) // Solo il server può aggiornare il timer quando è attivo
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                networkTime.Value = timeRemaining; // Aggiorna la variabile sincronizzata
            }
            else
            {
                timeRemaining = 0;
                EndGame(); // Quando il tempo scade
            }
        }

        UpdateTimerDisplay(); // Aggiorna il display globale
    }

    void UpdateTimerDisplay()
    {
        // Mostra il tempo rimanente (minuti:secondi)
        int minutes = Mathf.FloorToInt(networkTime.Value / 60);
        int seconds = Mathf.FloorToInt(networkTime.Value % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void ActivateTimer()
    {
        if (IsServer) // Solo il server può attivare il timer
        {
            timerActive = true;
        }
    }

    void EndGame()
    {
        // Azioni da fare quando il timer scade (fine del gioco)
        Debug.Log("Tempo scaduto! Fine del gioco.");
        // Puoi aggiungere il codice per terminare il gioco o mostrare un pannello di game over.
    }
}
