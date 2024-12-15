using System;
using UnityEngine;
using Unity.Services.Authentication;
using Unity.Services.Core;

public class MenuManager : MonoBehaviour
{
    //Variabili per gestire lo stato di inizializzazione del MenuManager e degli eventi.
    private bool initialized = false;
    private bool eventsInitialized = false;

    public string PlayerName { get; set; } // Proprietà per tenere traccia del nome del giocatore

    private static MenuManager singleton = null; //Istanza statica del MenuManager (Singleton).

    public static MenuManager Singleton  //Proprietà statica per ottenere l'istanza unica del MenuManager.
    {
        get
        {
            if (singleton == null)
            {
                singleton = FindFirstObjectByType<MenuManager>();
                singleton.Initialize();
            }
            return singleton; 
        }
    }

    private void Initialize() //Funzione di inizializzazione del MenuManager.
    {
        if (initialized) { return; }
        initialized = true;
    }
    
    private void OnDestroy() //Metodo chiamato quando l'oggetto viene distrutto.
    {
        if (singleton == this)
        {
            singleton = null;
        }
    }

    private void Awake() //Funzione di inizializzazione chiamata al momento di creazione dell'oggetto.
    {
        Application.runInBackground = true; //Permette al gioco di continuare a funzionare in background
        StartClientService(); //Avvia il servizio di client
    } 

    public async void StartClientService() //Funzione asincrona per inizializzare e connettere il client ai servizi di Unity.
    {
        PanelManager.CloseAll();
        PanelManager.Open("loading");
        try
        {
            if (UnityServices.State != ServicesInitializationState.Initialized) //Inizializza i servizi di Unity se non sono già stati inizializzati
            {
                var options = new InitializationOptions();
                options.SetProfile("default_profile");
                await UnityServices.InitializeAsync();
            }
            
            if (!eventsInitialized) //Se gli eventi non sono stati ancora inizializzati, setup degli eventi
            {
                SetupEvents();
            }

            if (AuthenticationService.Instance.SessionTokenExists)  //Se c'è già una sessione attiva, esegue il login automaticamente
            {
                SignInAnonymouslyAsync(); //Se abbiamo effettuato l'accesso con qualche provider, una volta che il token di sessione è stato creato usando quel provider, il login in maniera anonima andrà automaticamente ad effettuare l'accesso con quel provider
            }
            else
            {
                PanelManager.Open("auth");
            }
        }
        catch (Exception)
        {
            ShowError(ErrorMenu.Action.StartService, "Connessione alla reta fallita.", "Riprova");
        }
    }

    public async void SignInAnonymouslyAsync() //Funzione asincrona per effettuare il login anonimo con i servizi di autenticazione.
    {
        PanelManager.Open("loading");
        try
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        catch (AuthenticationException)
        {
            ShowError(ErrorMenu.Action.OpenAuthMenu, "Login fallito.", "OK");
        }
        catch (RequestFailedException)
        {
            ShowError(ErrorMenu.Action.SignIn, "Connessione alla reta fallita.", "Riprova");
        }
    }
    
    public async void SignInWithUsernameAndPasswordAsync(string username, string password) //Funzione asincrona per effettuare il login con username e password.
    {
        PanelManager.Open("loading");
        try
        {
            await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
        }
        catch (Exception ex) when (ex.Message.Contains("Invalid username or password"))
        {
            // Gestione errore per credenziali errate
            ShowError(ErrorMenu.Action.OpenAuthMenu, "Username o Password errati", "OK");
        }
        catch (RequestFailedException)
        {
            // Gestione errore di rete
            ShowError(ErrorMenu.Action.OpenAuthMenu, "Connessione alla rete fallita.", "Riprova");
        }
        
    }
    
    public async void SignUpWithUsernameAndPasswordAsync(string username, string password) //Funzione asincrona per registrare un nuovo utente con username e password.
    {
        PanelManager.Open("loading");
        try
        {
            await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
        }
        catch (AuthenticationException ex) when (ex.Message.Contains("username already exists"))
        {
            // Gestione errore per utente già esistente
            ShowError(ErrorMenu.Action.OpenAuthMenu, "L'utente esiste già.", "OK");
        }
        catch (Exception ex) when (ex.Message.Contains("Invalid username or password"))
        {
            // Gestione errore per credenziali non valide
            ShowError(ErrorMenu.Action.OpenAuthMenu, "Errore durante la registrazione.", "OK");
        }
        catch (RequestFailedException)
        {
            // Gestione errore di rete
            ShowError(ErrorMenu.Action.OpenAuthMenu, "Connessione alla rete fallita.", "Riprova");
        }
    }
    
    public void SignOut() //Funzione per effettuare il logout dell'utente.
    {
        AuthenticationService.Instance.SignOut();
        PanelManager.CloseAll();
        PanelManager.Open("auth");
    }
    
    private void SetupEvents() //Metodo per configurare gli eventi di autenticazione (login, logout, sessione scaduta).
    {
        eventsInitialized = true;
        AuthenticationService.Instance.SignedIn += () =>
        {
            SignInConfirmAsync();
        };

        AuthenticationService.Instance.SignedOut += () =>
        {
            PanelManager.CloseAll();
            PanelManager.Open("auth");
        };
        
        AuthenticationService.Instance.Expired += () =>
        {
            SignInAnonymouslyAsync();
        };
    }
    
    private void ShowError(ErrorMenu.Action action = ErrorMenu.Action.None, string error = "", string button = "") //Funzione per mostrare un pannello di errore con un messaggio e un bottone.
    {
        PanelManager.Close("loading");
        ErrorMenu panel = (ErrorMenu)PanelManager.GetSingleton("error");
        panel.Open(action, error, button);
    }
    
    private async void SignInConfirmAsync() //Funzione asincrona per confermare il login dopo il successo.
    {
        try
        {
            if (string.IsNullOrEmpty(AuthenticationService.Instance.PlayerName))
            {
                await AuthenticationService.Instance.UpdatePlayerNameAsync("Player");
            }
            PanelManager.CloseAll();
            PanelManager.Open("main");
        }
        catch (Exception e) 
        {
            Debug.LogException(e);
        }
    }
    
}