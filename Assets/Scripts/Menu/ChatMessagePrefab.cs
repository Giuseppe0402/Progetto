using TMPro;
using UnityEngine;

public class ChatMessagePrefab : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameText = null;
    [SerializeField] private TextMeshProUGUI messageText = null;

    public void SetMessage(string playerName, string message)
    {
        playerNameText.text = playerName; // Nome del giocatore
        messageText.text = message; // Testo del messaggio
    }
}
