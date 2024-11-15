using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Services.CloudSave;
using Unity.Services.CloudSave.Models.Data.Player;
using UnityEngine.UI;
using SaveOptions = Unity.Services.CloudSave.Models.Data.Player.SaveOptions;

public class CustomizationMenu : Panel
{
    //Riferimenti agli oggetti UI
    [SerializeField] public TextMeshProUGUI characterText = null;
    [SerializeField] private Button characterButton = null;
    [SerializeField] private Button colorButton = null;
    [SerializeField] private Button closeButton = null;
    [SerializeField] private Button saveButton = null;

    //Variabili per salvare i dati di personalizzazione
    private int savedColor = 0;
    private int savedCharacter = 0;

    //Variabili per la selezione attuale di colore e personaggio
    private int color = 0;
    private int character = 0;

    //Liste di possibili opzioni per personaggi e colori
    private string[] characters = { "Uomo", "Donna"};
    private Color[] colors = { Color.green, Color.red, Color.blue, Color.magenta, Color.cyan };

    public override void Initialize() //Metodo di inizializzazione e aggiunge i vari listener ai bottoni corrispondenti
    {
        if (IsInitialized)
        {
            return;
        }
        closeButton.onClick.AddListener(ClosePanel);
        characterButton.onClick.AddListener(ChangeCharacter);
        colorButton.onClick.AddListener(ChangeColor);
        saveButton.onClick.AddListener(Save);
        base.Initialize();
    }
    
    public override void Open() //Metodo per aprire il menu
    {
        base.Open();
        LoadData(); 
    }
    
    private async void LoadData() //Metodo per caricare i dati salvati
    {
        //Inizializza i valori
        characterText.text = "";
        characterButton.interactable = false;
        colorButton.interactable = false;
        saveButton.interactable = false;
        character = 0;
        color = 0;
        savedCharacter = 0;
        savedColor = 0;
        try
        {
            var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "character" }, new LoadOptions(new PublicReadAccessClassOptions())); //Carica i dati dal servizio di CloudSave
            if (playerData.TryGetValue("character", out var characterData)) //Verifica se i dati per il personaggio sono disponibili
            {
                var data = characterData.Value.GetAs<Dictionary<string, object>>();
                savedCharacter = int.Parse(data["type"].ToString());
                savedColor = int.Parse(data["color_index"].ToString());
                character = savedCharacter;
                color = savedColor;
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
        characterButton.interactable = true;
        colorButton.interactable = true;
        ApplyData();
    }

    private async void Save() //Metodo per salvare i dati personalizzati
    {
        saveButton.interactable = false;
        characterButton.interactable = false;
        colorButton.interactable = false;
        try
        {
            var playerData = new Dictionary<string, object> //Crea un dizionario con i dati da salvare
            {
                { "type", character },
                { "color", "#" + ColorUtility.ToHtmlStringRGBA(colors[color]) },
                { "color_index", color }
            };
            var data = new Dictionary<string, object> { { "character", playerData } };
            await CloudSaveService.Instance.Data.Player.SaveAsync(data, new SaveOptions(new PublicWriteAccessClassOptions())); //Salva i dati nel cloud
            savedCharacter = character;
            savedColor = color;
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            saveButton.interactable = true;
        }
        characterButton.interactable = true;
        colorButton.interactable = true;
    }

    private void ChangeCharacter() //Metodo per cambiare il personaggio
    {
        character++;
        if (character >= characters.Length)
        {
            character = 0;
        }
        ApplyData();
    }

    private void ChangeColor() //Metodo per cambiare il colore
    {
        color++;
        if (color >= colors.Length)
        {
            color = 0;
        }
        ApplyData();
    }
    
    private void ApplyData() //Metodo per applicare i dati aggiornati (personaggio e colore)
    {
        characterText.text = characters[character];
        characterText.color = colors[color];
        saveButton.interactable = character != savedCharacter || color != savedColor;
    }
    
    private void ClosePanel() //Metodo per chiudere il menu
    {
        Close();
    }
    
}