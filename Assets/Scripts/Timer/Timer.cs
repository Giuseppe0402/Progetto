using Unity.Netcode;
using UnityEngine;
using TMPro;

public class Timer : NetworkBehaviour
{

    [SerializeField] private GameObject porta1;
    [SerializeField] private GameObject porta2;
    public float timeLimit = 300f; // Tempo di inizio in secondi (5 minuti)
    private float timeRemaining;
    public TextMeshProUGUI timerText; // Riferimento al testo del timer (ui globale)

    // Variabile di rete per sincronizzare il tempo
    private NetworkVariable<float> networkTime = new NetworkVariable<float>(300f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private bool timerActive = false; // Flag per verificare se il timer � attivo
    private bool gameEnded = false; // Flag per sapere se il gioco � finito

    void Start()
    {
        if (IsServer) // Solo il server gestisce il timer
        {
            timeRemaining = timeLimit;
        }
    }

    void Update()
    {
        if (IsServer && timerActive) // Solo il server pu� aggiornare il timer quando � attivo
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                networkTime.Value = timeRemaining; // Aggiorna la variabile sincronizzata
            }
            else
            {
                timeRemaining = 0;
                EndGame(false); // Quando il tempo scade
            }
        }

        CheckPorte();
        UpdateTimerDisplay(); // Aggiorna il display globale
    }

    void CheckPorte()
    {
        if (!porta1.activeSelf && !porta2.activeSelf) // Se entrambe le porte sono disattivate
        {
            EndGame(true);
        }
    }

    void StopTimer()
    {
        if (IsServer)
        {
            timerActive = false;
            Debug.Log("Timer fermato! Entrambe le porte sono chiuse.");
        }
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
        if (IsServer) // Solo il server pu� attivare il timer
        {
            timerActive = true;
        }
    }

    void EndGame(bool hasWon)
    {
        if (gameEnded) return; // Se il gioco � gi� finito, non fare nulla

        gameEnded = true; // Segna che il gioco � terminato

        StopTimer();
        if (hasWon)
        {
            // Mostra messaggio di vittoria
            timerText.text = "Hai vinto!";
            Debug.Log("Hai vinto! Entrambe le porte sono state aperte.");
        }
        else
        {
            // Mostra messaggio di sconfitta
            timerText.text = "Tempo scaduto! Hai perso!";
            Debug.Log("Tempo scaduto! Hai perso.");
        }
    }
}
