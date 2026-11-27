using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Chat.Common;

namespace Chat.Client;

//Сервер не пустил в чат
public class LoginRejectedException : Exception
{
    public LoginRejectedException(string message) : base(message) { }
}


public class ChatClient
{
    private static readonly TimeSpan ConnectTimeout = TimeSpan.FromSeconds(5);

    private readonly BlockingCollection<string> _outgoing = new(); // очередь на отправку
    private TcpClient? _tcp;
    private NetworkStream? _stream;
    private StreamReader? _reader;
    private int _disconnectedRaised;
    private volatile bool _closedByUser;

    public event Action<ChatMessage>? MessageReceived;

    public event Action<string>? Disconnected;

    public string Nick { get; private set; } = "";

    public async Task ConnectAsync(string host, int port, string nick)
    {
        var tcp = new TcpClient { NoDelay = true };
        try
        {
            await tcp.ConnectAsync(host, port).WaitAsync(ConnectTimeout).ConfigureAwait(false);

            NetworkStream stream = tcp.GetStream();
            var reader = new StreamReader(stream, new UTF8Encoding(false));

            
            byte[] join = Encoding.UTF8.GetBytes(ChatMessage.Create(MessageTypes.Join, nick, "").ToJson() + "\n");
            await stream.WriteAsync(join, 0, join.Length).WaitAsync(ConnectTimeout).ConfigureAwait(false);

            
            string? line = await reader.ReadLineAsync().WaitAsync(ConnectTimeout).ConfigureAwait(false);
            if (line == null) throw new IOException("Сервер закрыл соединение.");

            ChatMessage reply = ChatMessage.FromJson(line);
            if (reply.Type == MessageTypes.Error) throw new LoginRejectedException(reply.Text);
            if (reply.Type != MessageTypes.Ok) throw new IOException("Неожиданный ответ сервера.");

            tcp.SendTimeout = 5000;
            _tcp = tcp;
            _stream = stream;
            _reader = reader;
            Nick = nick;
        }
        catch
        {
            tcp.Dispose();
            throw;
        }
    }

    public void StartReading()
    {
        new Thread(ReadLoop) { IsBackground = true }.Start();
        new Thread(WriteLoop) { IsBackground = true }.Start();
    }

    //Сообщение всем.
    public void SendText(string text) =>
        Enqueue(new ChatMessage(MessageTypes.Text, Nick, text, DateTime.UtcNow, null, null));

    //Личное сообщение 
    public void SendPrivate(string toNick, string text) =>
        Enqueue(new ChatMessage(MessageTypes.Private, Nick, text, DateTime.UtcNow, toNick, null));

    //сообщение в очереди; (окно не ждёт сеть).
    private void Enqueue(ChatMessage message)
    {
        try
        {
            _outgoing.Add(message.ToJson() + "\n");
        }
        catch (InvalidOperationException)
        {
            // Уже закрыто — отправлять некуда.
        }
    }

    //читает строки от сервера и сообщает юзерам.
    private void ReadLoop()
    {
        try
        {
            string? line;
            while ((line = _reader!.ReadLine()) != null)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;

                ChatMessage message;
                try
                {
                    message = ChatMessage.FromJson(line);
                }
                catch (JsonException)
                {
                    continue; 
                }
                MessageReceived?.Invoke(message);
            }
        }
        catch (IOException) { }
        catch (SocketException) { }
        catch (ObjectDisposedException) { }

        OnConnectionLost();
    }

    private void WriteLoop()
    {
        try
        {
            foreach (string line in _outgoing.GetConsumingEnumerable())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(line);
                _stream!.Write(bytes, 0, bytes.Length);
            }
        }
        catch (IOException) { OnConnectionLost(); }
        catch (SocketException) { OnConnectionLost(); }
        catch (ObjectDisposedException) { OnConnectionLost(); }
    }

    private void OnConnectionLost()
    {
        if (_closedByUser) return;
        if (Interlocked.Exchange(ref _disconnectedRaised, 1) == 1) return;

        CloseSocket();
        Disconnected?.Invoke("Соединение с сервером потеряно.");
    }

    //Юзер сам выходит из чата.
    public void Close()
    {
        _closedByUser = true;
        _outgoing.CompleteAdding();
        CloseSocket();
    }

    private void CloseSocket()
    {
        try { _tcp?.Client.Shutdown(SocketShutdown.Both); } catch (SocketException) { } catch (ObjectDisposedException) { }
        try { _tcp?.Close(); } catch (SocketException) { }
    }
}
