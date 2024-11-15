using System.Collections;
using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartingSessionMenu : Panel
{
    ////Riferimenti agli elementi UI
    [SerializeField] private TextMeshProUGUI statusText = null;
    [SerializeField] private Image progressImage = null;

    //Variabili per gestire lo stato del caricamento e l'attivazione della scena.
    private bool allowSceneActivation = false; public bool isConfirmed { get { return allowSceneActivation; } }
    private bool loading = false; public bool isLoading { get { return loading; } }

    //Quando l'host avvia il gioco, andremo a mettere loading su true per tutti i clients ma setteremo allowSceneActivation su false finché i relay allocations non sono terminate e abbiamo un joinCode


    public void StartGameByLobby(Lobby lobby, bool waitForConfirmation) //Metodo che avvia il gioco da un lobby esistente.
    {
        Open();
        statusText.text = "Il gioco sta caricando...";
        
        string map = lobby.Data["map"].Value;
        string sceneName = "SessionMap01";
        // ToDo: Mettere il nome della scena di gioco da caricare

        allowSceneActivation = !waitForConfirmation; //Imposta se la scena può essere attivata immediatamente o meno in base alla conferma dell'utente.
        if (loading == false)
        {
            loading = true;
            StartCoroutine(LoadAsyncScene(sceneName)); //Avvia la coroutine per caricare la scena in modo asincrono.
        }
    }

    public void StartGameConfirm() //Metodo che conferma l'avvio della scena.
    {
        allowSceneActivation = true;
    }
    
    public override void Open() //Metodo per aprire il pannello e inizializzare il progresso.
    {
        progressImage.fillAmount = 0;
        base.Open();
    }

    public override void Close() //Metodo per chiudere il pannello e resettare lo stato di caricamento.
    {
        base.Close();
        loading = false;
        allowSceneActivation = false;
    }

    private IEnumerator LoadAsyncScene(string sceneName) //Coroutine che gestisce il caricamento asincrono della scena.
    {   //Inizia il caricamento asincrono della scena specificata.
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        operation.allowSceneActivation = false;
        while (operation.isDone == false) //Finché l'operazione di caricamento non è completata, aggiorna la barra di progresso.
        {
            progressImage.fillAmount = operation.progress;
            if (operation.progress >= 0.9f)
            {
                operation.allowSceneActivation = allowSceneActivation;
            }
            yield return null;
        }
    }
    
}