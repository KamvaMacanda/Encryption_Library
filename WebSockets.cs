using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Encryption_Library
{
    public class WebSockets
    {

        public class ChatMessage
        {
            public string? Type { get; set; }
            public string? Sender { get; set; }
            public string? Receiver { get; set; }
            public string? Payload { get; set; } // e.g. your encrypted message
        }

        public class WebSocketsApplication
        {
            public static readonly Dictionary<string, WebSocket> Users = new();
            private static readonly object UsersLock = new();

            private readonly HttpListener listener = new();

            public WebSocketsApplication(int port)
            {
                listener.Prefixes.Add($"http://localhost:{port}/");
            }

            public async Task StartAsync()
            {
                listener.Start();
                Console.WriteLine("Chat server running...");

                while (true)
                {
                    HttpListenerContext context = await listener.GetContextAsync();

                    if (context.Request.IsWebSocketRequest)
                    {
                        _ = HandleConnectionAsync(context);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        context.Response.Close();
                    }
                }
            }

            private async Task HandleConnectionAsync(HttpListenerContext context)
            {
                HttpListenerWebSocketContext wsContext = await context.AcceptWebSocketAsync(null);
                WebSocket socket = wsContext.WebSocket;

                Console.WriteLine("New connection: " + context.Request.RemoteEndPoint);

                var buffer = new byte[4096];

                try
                {
                    while (socket.State == WebSocketState.Open)
                    {
                        string? message = await ReceiveMessageAsync(socket, buffer);
                        if (message == null) break;

                        OnMessage(socket, message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    RemoveUser(socket);
                    Console.WriteLine("Connection closed.");
                }
            }

            private static async Task<string?> ReceiveMessageAsync(WebSocket socket, byte[] buffer)
            {
                using var ms = new MemoryStream();

                while (true)
                {
                    WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        return null;
                    }

                    ms.Write(buffer, 0, result.Count);
                    if (result.EndOfMessage) break;
                }

                return Encoding.UTF8.GetString(ms.ToArray());
            }

            private void OnMessage(WebSocket conn, string message)
            {
                ChatMessage? msg = JsonSerializer.Deserialize<ChatMessage>(message, JsonOptions);
                if (msg?.Type == null || msg.Sender == null) return;

                if (msg.Type == "login")
                {
                    lock (UsersLock) { Users[msg.Sender] = conn; }
                    return;
                }

                if (msg.Type == "chat")
                {
                    WebSocket? receiverSocket;
                    lock (UsersLock) { Users.TryGetValue(msg.Receiver ?? "", out receiverSocket); }

                    if (receiverSocket != null)
                        SendRaw(receiverSocket, message);
                    else
                        Console.WriteLine("Receiver not online: " + msg.Receiver);
                }
            }

            private static void SendRaw(WebSocket socket, string message)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(message);
                _ = socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }

            private static void RemoveUser(WebSocket socket)
            {
                lock (UsersLock)
                {
                    string? key = Users.FirstOrDefault(kvp => kvp.Value == socket).Key;
                    if (key != null) Users.Remove(key);
                }
            }

            private static readonly JsonSerializerOptions JsonOptions = new()
            {
                PropertyNameCaseInsensitive = true
            };

            public static async Task Main(string[] args)
            {
                var server = new WebSocketsApplication(8887);
                await server.StartAsync();
            }
        }
    }