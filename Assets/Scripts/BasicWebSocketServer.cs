using System.IO;
using System;
using UnityEngine;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.Collections.Generic;

// Clase que se adjunta a un GameObject en Unity para iniciar el servidor WebSocket.
public class BasicWebSocketServer : MonoBehaviour
{
    // Instancia del servidor WebSocket.
    private WebSocketServer wss;

    // Se ejecuta al iniciar la escena.
    void Start()
    {
        if (!IsPortInUse(7777))
        {
            // Crear un servidor WebSocket que escucha en el puerto 7777.
            wss = new WebSocketServer(7777);

            // Añadir un servicio en la ruta "/" que utiliza el comportamiento EchoBehavior.
            wss.AddWebSocketService<ChatBehavior>("/");

        // Iniciar el servidor.
            wss.Start();
            Debug.Log("Servidor WebSocket iniciado en ws://127.0.0.1:7777/");
        }
        else
        {
            Debug.Log("El servidor ya está en funcionamiento. Conéctese al servidor existente.");
        }
    }

    // Se ejecuta cuando el objeto se destruye (por ejemplo, al cerrar la aplicación o cambiar de escena).
    void OnDestroy()
    {
        // Si el servidor está activo, se detiene de forma limpia.
        if (wss != null)
        {
            wss.Stop();
            wss = null;
            Debug.Log("Servidor WebSocket detenido.");
        }
    }
    private bool IsPortInUse(int port)
    {
        bool inUse = false;
        System.Net.NetworkInformation.IPGlobalProperties ipProperties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();
        var connections = ipProperties.GetActiveTcpListeners();
        foreach (var connection in connections)
        {
            if (connection.Port == port)
            {
                inUse = true;
                break;
            }
        }
        return inUse;
    }
}

// Comportamiento básico del servicio WebSocket: simplemente devuelve el mensaje recibido.
public class ChatBehavior : WebSocketBehavior
{
    private static int clientCounter = 0;

    private static readonly List<string> coloresDisponibles = new List<string>
    {
        "#FF5733", "#33FF57", "#3357FF", "#FF33A1", "#33FFA1",
        "#A133FF", "#FF8C33", "#33FF8C", "#8C33FF", "#FF338C",
        "#338CFF", "#8CFF33", "#FF3333", "#33FF33", "#3333FF",
        "#FF8333", "#33FF83", "#8333FF", "#FF3383", "#3383FF"
    };

    private static readonly Dictionary<string, string> usuariosColores = new Dictionary<string, string>();

    private static readonly List<ChatBehavior> clientesConectados = new List<ChatBehavior>();

    //MENSAJES DEL XAT  [OPCIONAL]
    private static readonly List<string> mensajes = new List<string>();

    //QUIERO QUE POR CADA CONVERSACION SE ABRA UN NUEVO ARCHIVO DE XAT APRA ELLO PON UN NUMERO DE 4 CIFRAS RANDOM AL GENERAR EL ARCHIVO
    private static readonly System.Random random = new System.Random();
    private static readonly string rutaArchivo = @"..\Logs\mensajes_chat_" + random.Next(1000, 9999) + ".txt";

    // ID y Color del cliente actual
    private string clientId;
    private string colorAsignado;
    protected override void OnOpen()
    {
        clientCounter++;
        clientId = clientCounter.ToString();
        //haz que pille un color random
        colorAsignado = coloresDisponibles[UnityEngine.Random.Range(0, coloresDisponibles.Count)];

        usuariosColores.Add(clientId, colorAsignado);
        clientesConectados.Add(this);

        //MENSAJES EN LA UI QUE INDIQUEN CUANDO UN CLIENTE SE CONECTA  [OPCIONAL]
        Sessions.Broadcast($"<color={colorAsignado}><b>Cliente-{clientId}</b></color> joined the game.");

    }
    protected override void OnClose(CloseEventArgs e)
    {
        usuariosColores.Remove(clientId);
        clientesConectados.Remove(this);

        //MENSAJES EN LA UI QUE INDIQUEN CUANDO UN CLIENTE SE DESCONECTA  [OPCIONAL]
        Sessions.Broadcast($"El Cliente{clientId} se ha desconectado.");
    }
    // Se invoca cuando se recibe un mensaje desde un cliente.
    protected override void OnMessage(MessageEventArgs e)
    {
        String mensaje = $"<color={colorAsignado}><b>Cliente{clientId}: </b></color>" + e.Data;
        mensajes.Add(mensaje);
        GuardarMensajeEnArchivo(mensaje);
        Sessions.Broadcast(mensaje);
    }
    private void GuardarMensajeEnArchivo(string mensaje)
    {
        try
        {
            // Abre el archivo en modo Append y escribe el mensaje con un salto de línea
            using StreamWriter sw = new StreamWriter(rutaArchivo, true);
            sw.WriteLine(mensaje);
        }
        catch (Exception ex)
        {
            Send($"Error al guardar el mensaje en el archivo: {ex.Message}");
        }
    }
}
