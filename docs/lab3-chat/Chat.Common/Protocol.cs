namespace Chat.Common;

public static class MessageTypes
{
    public const string Join = "join";

    public const string Text = "text";

    public const string Private = "private";

    public const string Ok = "ok";
    public const string Error = "error";

    public const string Notice = "system";

    public const string Users = "users";

    public const string History = "history";
}

public static class ChatDefaults
{
    public const int DefaultPort = 5000;
    public const int MaxNickLength = 20;
    public const int MaxTextLength = 2000;
    public const int HistorySize = 20;
}
