using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CombinationLock : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    public GameObject Panel { get { return panel; } set { panel = value; } }  // Getter e setter

    [SerializeField] private TMP_InputField combinationInput;
    public TMP_InputField CombinationInput { get { return combinationInput; } set { combinationInput = value; } }  // Getter e setter

    [SerializeField] private Button confirmButton;
    public Button ConfirmButton
    {
        get { return confirmButton; }
        set
        {
            confirmButton = value;
            Debug.Log($"ConfirmButton impostato su {confirmButton.name}");
        }
    }

    private string correctCombination = "1712"; // Combinazione corretta
    private GameObject targetObject; // Oggetto con cui stiamo interagendo

    public delegate void OnCombinationCorrect(GameObject target);
    public event OnCombinationCorrect CombinationCorrectEvent;

    public static bool IsCodePanelActive { get; private set; } = false;
    public static bool WasCodePanelClosedThisFrame { get; private set; } = false;

    [SerializeField] private AudioClip openPanelSound;
    [SerializeField] private AudioClip correctCombinationSound;
    [SerializeField] private AudioClip incorrectCombinationSound;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("AudioSource non trovato! Aggiungine uno a questo GameObject.");
        }

        // Se il panel è null, prova a trovarlo dinamicamente
        if (panel == null)
        {
            panel = transform.Find("CodePanel")?.gameObject;
            if (panel == null)
            {
                Debug.LogError("CodePanel non trovato dinamicamente!");
                return;  // Evita ulteriori errori se il panel non è stato trovato
            }
        }

        // Verifica se il confirmButton è nullo e assegnalo dinamicamente
        if (confirmButton == null)
        {
            confirmButton = panel?.transform.Find("CodeButton")?.GetComponent<Button>();
            if (confirmButton == null)
            {
                Debug.LogError("ConfirmButton non trovato dinamicamente.");
                return;
            }
        }

        if (combinationInput == null)
        {
            combinationInput = transform.Find("CodeInput")?.GetComponent<TMP_InputField>(); ;
            if (combinationInput == null)
            {
                Debug.LogError("CodeInput non trovato dinamicamente!");
                return;  // Evita ulteriori errori se il panel non è stato trovato
            }
        }
    }

    private void Update()
    {
        WasCodePanelClosedThisFrame = false; // Resetta il flag all'inizio di ogni frame

        if (Input.GetKeyDown(KeyCode.Escape) && panel.activeSelf)
        {
            HidePanel();
            WasCodePanelClosedThisFrame = true; // Imposta il flag
        }
    }

    [ClientRpc]
    public void ShowPanelForClientRpc(ulong clientId, string combination, GameObject target)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId) // Solo il client corretto esegue l'aggiornamento
        {
            ShowPanel(combination, target);
        }
    }


    // Mostra il pannello e imposta la combinazione corretta
    public void ShowPanel(string combination, GameObject target)
    {
        IsCodePanelActive = true;
        correctCombination = combination;
        targetObject = target;  // Assicurati che targetObject venga impostato correttamente
        combinationInput.text = string.Empty;
        panel.SetActive(true);  // Mostra il pannello
        Time.timeScale = 0f;

        // Riproduci il suono di apertura del pannello
        PlaySound(openPanelSound);

        // Imposta il cursore sbloccato e visibile
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(CheckCombination);    

        // Verifica che combinationInput e targetObject non siano nulli
        if (combinationInput == null)
        {
            Debug.LogError("CombinationInput è nullo!");
        }
        if (targetObject == null)
        {
            Debug.LogError("TargetObject è nullo!");
        }

        Debug.Log("Pannello mostrato.");
    }

    // Nasconde il pannello
    public void HidePanel()
    {
        IsCodePanelActive = false;
        panel.SetActive(false);
        Time.timeScale = 1f;
        // Imposta il cursore bloccato e invisibile
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Controlla la combinazione inserita
    public void CheckCombination()
    {
        if (combinationInput == null || targetObject == null)
        {
            Debug.LogError("Riferimenti mancanti: CombinationInput o TargetObject!");
            return;
        }

        if (string.IsNullOrEmpty(correctCombination))
        {
            Debug.LogError("La combinazione corretta non è stata impostata!");
            return;
        }

        if (combinationInput.text == "1712")
        {
            Debug.Log("Combinazione corretta!");
            PlaySound(correctCombinationSound);
            CombinationCorrectEvent?.Invoke(targetObject);
            ShowSuccessFeedback();
            HidePanel();
        }
        else
        {
            Debug.LogWarning("Combinazione errata.");
            ShowFailureFeedback();
            PlaySound(incorrectCombinationSound);
        }
    }

    private void ShowSuccessFeedback()
    {
        if (panel != null)
        {
            panel.GetComponent<Image>().color = Color.green;  // Esempio: cambia il colore del pannello in verde
        }
    }

    private void ShowFailureFeedback()
    {
        if (panel != null)
        {
            panel.GetComponent<Image>().color = Color.red;  // Esempio: cambia il colore del pannello in rosso
        }
    }

    private void PlaySound(AudioClip clip)
    {
        AudioManager.PlayClip(clip);
    }
}