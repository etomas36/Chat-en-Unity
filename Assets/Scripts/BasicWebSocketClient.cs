using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WebSocketSharp;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public class BasicWebSocketClient : MonoBehaviour
{
    public string uri = "ws://127.0.0.1:7777/";
    private WebSocket ws;
    public TMP_Text chatDisplay;
    public TMP_InputField inputField;
    public Button sendButton;
    public ScrollRect scrollRect;
    private static readonly Queue<Action> _actionsToRun = new Queue<Action>();


    void Start()
    {
        chatDisplay.text = "";
        inputField.onSubmit.AddListener(delegate { SendMessageToServer(); });


        ws = new WebSocket(uri);

        ws.OnOpen += (sender, e) =>
        {

            Debug.Log("WebSocket conectado correctamente.");
        };

        ws.OnMessage += (sender, e) =>
        {
            // Comprueba si el mensaje no está vacío
            if (!string.IsNullOrEmpty(e.Data))
            {
                EnqueueUiAction(() => { 
                    chatDisplay.text += "\n" + e.Data; 
                });
            }
            else
            {
                Debug.LogWarning("Se recibió un mensaje vacío.");
            }
        };

        ws.OnError += (sender, e) =>
        {
            Debug.LogError("Error en el WebSocket: " + e.Message);
        };

        ws.OnClose += (sender, e) =>
        {
            Debug.Log("WebSocket cerrado. Código: " + e.Code + ", Razón: " + e.Reason);
        };

        // Conectar de forma asíncrona al servidor WebSocket
        ws.ConnectAsync();

        inputField.Select();
        inputField.ActivateInputField();
    }

    void Update()
    {
        if(_actionsToRun.Count > 0)
        {
            Action action;

            lock (_actionsToRun)
            {
                action = _actionsToRun.Dequeue();
            }
            action?.Invoke();
            ScrollToBottom();
        }
    }

    public void SendMessageToServer()
    {
        if (ws != null && ws.ReadyState == WebSocketState.Open)
        {
            if (!string.IsNullOrEmpty(inputField.text))
            {
                string message = inputField.text;
                inputField.text = "";
                inputField.ActivateInputField();
                ws.Send(message);
            }
        }
        else
        {
            Debug.LogError("No se puede enviar el mensaje. La conexión no está abierta.");
        }
    }

    void OnDestroy()
    {
        if (ws != null)
        {
            ws.Close();
            ws = null;
        }
    }


    private void EnqueueUiAction(Action action)
    {
        lock (_actionsToRun)
        {
            _actionsToRun.Enqueue(action);
        }
    }
    void ScrollToBottom()
    {
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 0f;
    }
}
