using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class AuthenticationMenu : Panel
{
    //Riferimenti agli elementi UI per la gestione dell'autenticazione.
    [SerializeField] private TMP_InputField usernameInput = null;
    [SerializeField] private TMP_InputField passwordInput = null;
    [SerializeField] private Button signinButton = null;
    [SerializeField] private Button signupButton = null;
    [SerializeField] private Button anonymousButton = null;
    [SerializeField] private Button quitButton = null;

    public override void Initialize() //Metodo di inizializzazione del pannello. Aggiunge i listener ai bottoni per eseguire le azioni corrispondenti.
    {
        if (IsInitialized)
        {
            return;
        }
        anonymousButton.onClick.AddListener(AnonymousSignIn);
        signinButton.onClick.AddListener(SignIn);
        signupButton.onClick.AddListener(SignUp);
        quitButton.onClick.AddListener(Quit);
        base.Initialize();
    }

    public override void Open() //Metodo che apre il pannello di autenticazione e resetta i campi di input.
    {
        usernameInput.text = "";
        passwordInput.text = "";
        base.Open();
    }

    private void AnonymousSignIn() //Metodo che gestisce il login anonimo.
    {
        MenuManager.Singleton.SignInAnonymouslyAsync();
    }

    private void SignIn() //Metodo che gestisce il login con nome utente e password.
    {
        string user = usernameInput.text.Trim();
        string pass = passwordInput.text.Trim();
        if (string.IsNullOrEmpty(user) == false && string.IsNullOrEmpty(pass) == false)
        {
            MenuManager.Singleton.SignInWithUsernameAndPasswordAsync(user, pass);
        }
    }

    private void SignUp() //Metodo che gestisce la registrazione di un nuovo utente.
    {
        string user = usernameInput.text.Trim();
        string pass = passwordInput.text.Trim();
        if (string.IsNullOrEmpty(user) == false && string.IsNullOrEmpty(pass) == false)
        {
            if (IsPasswordValid(pass))
            {
                MenuManager.Singleton.SignUpWithUsernameAndPasswordAsync(user, pass);
            }
            else
            {
                ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
                panel.Open(ErrorMenu.Action.None, "La password non soddisfa i requisiti. Inserisci almeno 1 lettera maiuscola, 1 lettera minuscola, 1 cifra e 1 simbolo. Deve avere un minimo di 8 e un massimo di 30 caratteri.", "OK");
            }
        }
    }

    private void Quit()
    {
        ActionConfirmMenu panel = (ActionConfirmMenu)PanelManager.GetSingleton("action_confirm");
        panel.Open(QuitResult, "Sei sicuro di voler uscire?", "Si", "No");
    }

    private void QuitResult(ActionConfirmMenu.Result result)
    {
        if (result == ActionConfirmMenu.Result.Positive)
        {
            Application.Quit();
        }
    }

    private bool IsPasswordValid(string password) //Metodo per la validazione della password (Serve per il servizio che stiamo usando)
    {
        if (password.Length < 8 || password.Length > 30)
        {
            return false;
        }

        bool hasUppercase = false;
        bool hasLowercase = false;
        bool hasDigit = false;
        bool hasSymbol = false;

        foreach (char c in password)
        {
            if (char.IsUpper(c))
            {
                hasUppercase = true;
            }
            else if (char.IsLower(c))
            {
                hasLowercase = true;
            }
            else if (char.IsDigit(c))
            {
                hasDigit = true;
            }
            else if (!char.IsLetterOrDigit(c))
            {
                hasSymbol = true;
            }
        }
        return hasUppercase && hasLowercase && hasDigit && hasSymbol;
    }

}