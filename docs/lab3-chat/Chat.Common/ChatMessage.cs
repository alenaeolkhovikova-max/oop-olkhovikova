using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Chat.Common;

public record ChatMessage(
    string Type,
    string Nick,
    string Text,
    DateTime Time,
    string? To,
    List<string>? Users)
{
    private static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web);

    public string ToJson() => JsonSerializer.Serialize(this, Options);

    public static ChatMessage FromJson(string line) =>
        JsonSerializer.Deserialize<ChatMessage>(line, Options)
        ?? throw new JsonException("Пустое сообщение");

    public static ChatMessage Create(string type, string nick, string text) =>
        new(type, nick, text, DateTime.UtcNow, null, null);
}
