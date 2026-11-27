using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using Chat.Common;

namespace Chat.Server;

public class MessageHistory
{
    private readonly string _path;
    private readonly object _sync = new();
    private readonly List<ChatMessage> _last = new();

    public MessageHistory(string path)
    {
        _path = path;
        Load();
    }

    private void Load()
    {
        try
        {
            if (!File.Exists(_path)) return;
            foreach (string line in File.ReadLines(_path, Encoding.UTF8))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                try
                {
                    ChatMessage message = ChatMessage.FromJson(line);
                    if (message.Type == MessageTypes.Text) Remember(message);
                }
                catch (JsonException)
                {
       
                }
            }
        }
        catch (IOException) { }
        catch (System.UnauthorizedAccessException) { }
    }

    private void Remember(ChatMessage message)
    {
        _last.Add(message);
        if (_last.Count > ChatDefaults.HistorySize) _last.RemoveAt(0);
    }

    public void Add(ChatMessage message)
    {
        lock (_sync)
        {
            Remember(message);
            try
            {
                File.AppendAllText(_path, message.ToJson() + "\n", new UTF8Encoding(false));
            }
            catch (IOException) { }
            catch (System.UnauthorizedAccessException) { }
        }
    }

    public List<ChatMessage> Last()
    {
        lock (_sync)
        {
            return new List<ChatMessage>(_last);
        }
    }
}
