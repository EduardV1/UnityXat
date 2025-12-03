using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NativeWebSocket;

public class WebSocketClient : MonoBehaviour {

    [Header("Configuració del WebSocket")]
    [SerializeField]
    private string websocketUrl = "ws://localhost:8080"; 

    [Header("Tests des de l'editor")]
    [SerializeField]
    private string testMessage = "Hola des de Unity!";

    private WebSocket websocket;

    async void Start() {
        await ConnectWebSocket();
    }

    private async Task ConnectWebSocket() {

        websocket = new WebSocket(websocketUrl);

        websocket.OnOpen += () => {
            Debug.Log("WebSocket connectat!");
        };

        websocket.OnError += (e) => {
            Debug.LogError("WebSocket error: " + e);
        };

        websocket.OnClose += (e) => {
            Debug.Log("WebSocket tancat!");
        };

        websocket.OnMessage += (bytes) => {
            string message = Encoding.UTF8.GetString(bytes);
            Debug.Log("Missatge rebut: " + message);
        };

        await websocket.Connect();
    }

    public async void SendWebSocketMessage(string message) {
        if (websocket != null && websocket.State == WebSocketState.Open) {
            await websocket.SendText(message);
            Debug.Log("Missatge enviat: " + message);
        }
        else {
            Debug.LogWarning("No es pot enviar: connexió no oberta.");
        }
    }

    void Update() {
        if (websocket != null) {
            websocket.DispatchMessageQueue();
        }
    }

    private async void OnApplicationQuit() {
        if (websocket != null) {
            await websocket.Close();
        }
    }

    public void SendSerializedMessage() {
        if (string.IsNullOrEmpty(testMessage)) {
            Debug.LogWarning("testMessage està buit.");
            return;
        }
        SendWebSocketMessage(testMessage);
    }

    [ContextMenu("WebSocket/Enviar missatge de prova")]
    private void ContextSendTestMessage() {
        SendSerializedMessage();
    }
}
