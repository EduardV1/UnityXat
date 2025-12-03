using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using NativeWebSocket;

public class ChatManager : MonoBehaviour
{
    [Header("Configuració WebSocket")]
    [SerializeField]
    private string websocketUrl = "ws://localhost:8080"; 

    private WebSocket websocket;
    private string nick;

    // UI Toolkit
    private VisualElement root;
    private VisualElement loginPanel;
    private VisualElement chatPanel;

    private TextField nickField;
    private Button joinButton;

    private ScrollView messagesScroll;
    private TextField messageField;
    private Button sendButton;

    private void Awake()
    {
        // Obtenim el root del UIDocument
        var uiDocument = GetComponent<UIDocument>();
        root = uiDocument.rootVisualElement;

        // Login
        loginPanel = root.Q<VisualElement>("LoginPanel");
        chatPanel = root.Q<VisualElement>("ChatPanel");

        nickField = root.Q<TextField>("NickField");
        joinButton = root.Q<Button>("JoinButton");

        // Chat
        messagesScroll = root.Q<ScrollView>("MessagesScroll");
        messageField = root.Q<TextField>("MessageField");
        sendButton = root.Q<Button>("SendButton");

        // Estat inicial: només login visible
        ShowLoginPanel();

        // Esdeveniments de botons
        joinButton.clicked += OnJoinClicked;
        sendButton.clicked += OnSendClicked;
    }

    private void ShowLoginPanel()
    {
        loginPanel.style.display = DisplayStyle.Flex;
        chatPanel.style.display = DisplayStyle.None;
    }

    private void ShowChatPanel()
    {
        loginPanel.style.display = DisplayStyle.None;
        chatPanel.style.display = DisplayStyle.Flex;
    }

    private async void OnJoinClicked()
    {
        string enteredNick = nickField.value?.Trim();

        if (string.IsNullOrEmpty(enteredNick))
        {
            Debug.LogWarning("Has d'introduir un nick.");
            return;
        }

        nick = enteredNick;
        Debug.Log("Nick establert: " + nick);

        // Connectem al servidor WebSocket
        // await ConnectWebSocket();

        _ = ConnectWebSocket();
        // Un cop connectat, mostrem la sala de xat
        ShowChatPanel();

        // Opcional: enviar un missatge de "s'ha unit"
        await Task.Delay(300);
        await SendRawMessage($"*** {nick} s'ha unit al xat ***");
    }

    private async Task ConnectWebSocket()
    {
        websocket = new WebSocket(websocketUrl);

        websocket.OnOpen += () =>
        {
            Debug.Log("WebSocket connectat!");
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError("WebSocket error: " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("WebSocket tancat!");
        };

        websocket.OnMessage += (bytes) =>
        {
            string message = Encoding.UTF8.GetString(bytes);
            Debug.Log("Missatge rebut: " + message);
            AddMessageToChat(message);
        };

        await websocket.Connect();
    }


    private async void OnSendClicked()
    {
        string text = messageField.value?.Trim();
        if (string.IsNullOrEmpty(text))
            return;

        // Format: "nick: missatge"
        string fullMessage = $"{nick}: {text}";
        await SendRawMessage(fullMessage);

        // Buidem el camp
        messageField.value = "";
    }

    private async Task SendRawMessage(string message)
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.SendText(message);
            Debug.Log("Missatge enviat: " + message);
        }
        else
        {
            Debug.LogWarning("No es pot enviar, WebSocket no obert.");
        }
    }

    private void AddMessageToChat(string text)
    {
        var label = new Label(text);
        messagesScroll.Add(label);

        // Desplaça automàticament cap avall
        messagesScroll.ScrollTo(label);
    }

    private void Update()
    {
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }
    }
}
