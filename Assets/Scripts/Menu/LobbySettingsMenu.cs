using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine.UI;

public class LobbySettingsMenu : Panel
{
    //Riferimenti agli oggetti UI
    [SerializeField] private Button confirmButton = null;
    [SerializeField] private Button cancelButton = null;
    [SerializeField] private TMP_InputField nameInput = null;
    [SerializeField] private TMP_InputField maxPlayersInput = null;
    [SerializeField] private TMP_Dropdown visibilityDropdown = null;
    [SerializeField] private TMP_Dropdown mapDropdown = null;
    [SerializeField] private TMP_Dropdown languageDropdown = null;

    //Riferimento alla lobby
    private Lobby lobby = null;
    
    public override void Initialize() //Metodo di inizializzazione che assegna i vari listener e configura i campi di input
    {
        if (IsInitialized)
        {
            return;
        }
        confirmButton.onClick.AddListener(Confirm);
        cancelButton.onClick.AddListener(Cancel);
        nameInput.contentType = TMP_InputField.ContentType.Standard;
        maxPlayersInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        nameInput.characterLimit = 20;
        maxPlayersInput.characterLimit = 4;
        base.Initialize();
    }
    
    public void Open(Lobby lobby) //Metodo per aprire il menu delle impostazioni e caricare i dati della lobby
    {
        this.lobby = lobby;
        if (lobby == null) //Se la lobby è nulla, impostiamo valori nulli.
        {
            nameInput.name = "";
            maxPlayersInput.name = "5";
            visibilityDropdown.SetValueWithoutNotify(0); 
            mapDropdown.SetValueWithoutNotify(0); 
            languageDropdown.SetValueWithoutNotify(0); 
        } 
        else
        {   //Se la lobby esiste, carichiamo i dati dalla lobby esistente.
            nameInput.name = lobby.Name;
            maxPlayersInput.name = lobby.MaxPlayers.ToString();
            visibilityDropdown.SetValueWithoutNotify(lobby.IsPrivate ? 1 : 0);
            for (int i = 0; i < visibilityDropdown.options.Count; i++)
            {
                if ((lobby.IsPrivate && visibilityDropdown.options[i].text.ToLower() == "private") || (lobby.IsPrivate == false && visibilityDropdown.options[i].text.ToLower() == "public"))
                {
                    visibilityDropdown.SetValueWithoutNotify(i);
                    break;
                }
            }
           
            
            if (lobby.Data.ContainsKey("map"))
            {   //Carichiamo la mappa selezionata
                var gameMap = lobby.Data["map"].Value.ToLower();
                for (int i = 0; i < mapDropdown.options.Count; i++)
                {
                    if (mapDropdown.options[i].text.ToLower() == gameMap)
                    {
                        mapDropdown.SetValueWithoutNotify(i);
                        break;
                    }
                }
            }
            
            if (lobby.Data.ContainsKey("language"))
            {   //Carichiamo la lingua selezionata
                var language = lobby.Data["language"].Value.ToLower();
                for (int i = 0; i < languageDropdown.options.Count; i++)
                {
                    if (languageDropdown.options[i].text.ToLower() == language)
                    {
                        languageDropdown.SetValueWithoutNotify(i);
                        break;
                    }
                }
            }
        }
        Open();
    }

    private void Confirm() //Metodo che viene chiamato quando l'utente conferma le modifiche
    {
        string lobbyName = nameInput.text.Trim();
        int maxPlayer = 0;
        int.TryParse(maxPlayersInput.text.Trim(), out maxPlayer);
        bool isPrivate = visibilityDropdown.captionText.text.Trim().ToLower() == "private" ? true : false;
        string map = mapDropdown.captionText.text.Trim();
        string language = languageDropdown.captionText.text.Trim();
        if (maxPlayer > 0 && string.IsNullOrEmpty(lobbyName) == false)
        {
            LobbyMenu panel = (LobbyMenu)PanelManager.GetSingleton("lobby");
            if (lobby == null)
            {
                panel.CreateLobby(lobbyName, maxPlayer, isPrivate, map, language);
            }
            else
            {
                panel.UpdateLobby(lobby.Id, lobbyName, maxPlayer, isPrivate, map, language);
            }
            Close();
        }
    }
    
    private void Cancel() //Metodo che viene chiamato quando l'utente cancella le modifiche
    {
        Close();
    }
    
    public override void Close() //Metodo per chiudere il menu e azzerare i riferimenti alla lobby.
    {
        base.Close();
        lobby = null;
    }
    
}