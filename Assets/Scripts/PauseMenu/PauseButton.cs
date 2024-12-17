using UnityEngine;

public class PauseButton : MonoBehaviour
{
    // Riferimento al bottone e al pannello
    public GameObject pauseButton;
    public GameObject pausePanel;
    public GameObject resumeButton;
    public GameObject settingsButton;
    public GameObject settingsPanel;

    [SerializeField] private GameObject codePanel;

    private bool isPaused = false; // Stato del gioco

    private void Update()
    {
        // Controlla se il tasto Esc è stato premuto
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Non aprire il menu di pausa se il CodePanel è attivo o è stato chiuso in questo frame
            if (CombinationLock.IsCodePanelActive || CombinationLock.WasCodePanelClosedThisFrame)
                return;

            if (isPaused)
            {
                OnResumeButtonClick();
            }
            else
            {
                OnPauseButtonClick();
            }
        }
    }

    // Funzione da chiamare per aprire il menu di pausa
    public void OnPauseButtonClick()
    {
        pauseButton.SetActive(false);
        pausePanel.SetActive(true);
        isPaused = true; // Imposta lo stato del gioco come in pausa
        Time.timeScale = 0f; // Ferma il tempo di gioco

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Funzione da chiamare per tornare al gioco
    public void OnResumeButtonClick()
    {
        pausePanel.SetActive(false);
        pauseButton.SetActive(true);
        isPaused = false; // Imposta lo stato del gioco come non in pausa
        Time.timeScale = 1f; // Riprendi il tempo di gioco

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnSettingsButtonClick()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void OnCloseSettingsButtonClick()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }

    public void OnQuitButtonClicked()
    {
        // Chiama il metodo ExitSession della SessionManager
        SessionManager.Singleton.ExitSession();
    }
}