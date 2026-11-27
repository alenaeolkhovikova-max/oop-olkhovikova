using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Chat.Common;

namespace Chat.Server;

public class ChatServer
{
    private readonly int _port;
    private readonly MessageHistory _history;
    private readonly object _sync = new();
    private readonly object _logSync = new();
    private readonly Dictionary<string, ClientHandler> _clients = new(StringComparer.OrdinalIgnoreCase);

    private TcpListener? _listener;
    private volatile bool _stopped;

    public ChatServer(int port, MessageHistory history)
    {
        _port = port;
        _history = history;
    }

    public void Run()
    {
        _listener = new TcpListener(IPAddress.Any, _port);
        _listener.Start();
        Log($"Сервер запущен, жду клиентов на порту {_port}");

        while (!_stopped)
        {
            TcpClient tcp;
            try
            {
                tcp = _listener.AcceptTcpClient();
            }
            catch (SocketException) { break; }       
            catch (ObjectDisposedException) { break; }

            var handler = new ClientHandler(tcp, this);
            var thread = new Thread(handler.Run) { IsBackground = true };
            thread.Start();
        }
    }

    public void Stop()
    {
        _stopped = true;
        try { _listener?.Stop(); } catch (SocketException) { }

        List<ClientHandler> all;
        lock (_sync)
        {
            all = _clients.Values.ToList();
        }
        foreach (ClientHandler handler in all) handler.Close();
    }

    public bool TryJoin(ClientHandler handler, string nick, out string error)
    {
        error = "";
        lock (_sync)
        {
            if (_clients.ContainsKey(nick))
            {
                error = $"Ник «{nick}» уже занят. Выберите другой.";
                return false;
            }

            _clients[nick] = handler;
            handler.Nick = nick;

            handler.Send(ChatMessage.Create(MessageTypes.Ok, nick, "Добро пожаловать в чат!"));
            foreach (ChatMessage old in _history.Last())
                handler.Send(new ChatMessage(MessageTypes.History, old.Nick, old.Text, old.Time, null, null));
        }
        return true;
    }

    public void AnnounceJoin(ClientHandler handler)
    {
        Log($"{handler.Nick} вошёл ({handler.Address})");
        Broadcast(ChatMessage.Create(MessageTypes.Notice, "", $"{handler.Nick} вошёл в чат"));
        BroadcastUsers();
    }

    public void Leave(ClientHandler handler)
    {
        bool removed = false;
        lock (_sync)
        {
            if (handler.Nick != null
                && _clients.TryGetValue(handler.Nick, out ClientHandler? current)
                && ReferenceEquals(current, handler))
            {
                _clients.Remove(handler.Nick);
                removed = true;
            }
        }

        if (!removed) return; 

        Log($"{handler.Nick} вышел");
        Broadcast(ChatMessage.Create(MessageTypes.Notice, "", $"{handler.Nick} вышел из чата"));
        BroadcastUsers();
    }

    public void BroadcastText(ClientHandler from, string text)
    {
        var message = new ChatMessage(MessageTypes.Text, from.Nick ?? "", text, DateTime.UtcNow, null, null);
        List<ClientHandler> targets;
        lock (_sync)
        {

            _history.Add(message);
            targets = _clients.Values.ToList();
        }
        Log($"{message.Nick}: {text}");
        foreach (ClientHandler target in targets) target.Send(message);
    }

    public void SendPrivate(ClientHandler from, string toNick, string text)
    {
        ClientHandler? target;
        lock (_sync)
        {
            _clients.TryGetValue(toNick, out target);
        }

        if (target == null)
        {
            from.Send(ChatMessage.Create(MessageTypes.Error, "", $"Пользователь «{toNick}» сейчас не в сети."));
            return;
        }
        if (ReferenceEquals(target, from))
        {
            from.Send(ChatMessage.Create(MessageTypes.Error, "", "Нельзя отправить личное сообщение самому себе."));
            return;
        }

        var message = new ChatMessage(MessageTypes.Private, from.Nick ?? "", text, DateTime.UtcNow, target.Nick, null);
        target.Send(message);
        from.Send(message);
        Log($"{message.Nick} → {message.To} (лично)");
    }

    private void Broadcast(ChatMessage message)
    {
        List<ClientHandler> targets;
        lock (_sync)
        {
            targets = _clients.Values.ToList();
        }
        foreach (ClientHandler target in targets) target.Send(message);
    }

    private void BroadcastUsers()
    {
        List<string> names;
        lock (_sync)
        {
            names = _clients.Keys.OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase).ToList();
        }
        var message = new ChatMessage(MessageTypes.Users, "", "", DateTime.UtcNow, null, names);
        Broadcast(message);
    }

    
    public void Log(string text)
    {
        lock (_logSync)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {text}");
        }
    }
}
