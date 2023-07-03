namespace Home;

public class ChatArgument
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Type Type { get; set; }
    public string Default { get; set; }
    public bool Optional { get; set; }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ChatCommandAttribute : LibraryAttribute
{
    public virtual new string Name { get; set; }
    public virtual List<ChatArgument> Arguments { get; set; }
    public virtual new string Description { get; set; }

    public ChatCommandAttribute()
    {
        Name = "help";
        Description = "Get a list of all chat commands";
        Arguments = new List<ChatArgument>();
    }

    public ChatCommandAttribute(string name, string description)
    {
        Name = name;
        Description = description;
        Arguments = new List<ChatArgument>();
    }

    public string GetArgumentTemplate()
    {
        string template = "";
        foreach (ChatArgument argument in Arguments)
        {
            if (argument.Optional)
            {
                template += $"[{argument.Name}] ";
            }
            else
            {
                template += $"<{argument.Name}> ";
            }
        }
        return template;
    }

    public virtual void Run(IClient client)
    {
        string commandString = "Chat Commands:";

        foreach (ChatCommandAttribute command in HomeGame.Current.ChatCommands)
        {
            commandString += "\n/" + command.Name.ToLower() + " - " + command.Description;
        }

        HomeChatBox.AddChatEntry(To.Single(client), null, commandString);
    }

    public virtual void Run(IClient client, string[] arguments)
    {
    }

    public static void Parse(IClient client, string command)
    {
        // Remove the slash at the beginning
        if(command.StartsWith("/"))
        {
            command = command.Substring(1);
        }

        // Split the command into its parts
        string[] parts = command.Split(" ");

        // Find the command
        ChatCommandAttribute chatCommand = null;
        foreach(ChatCommandAttribute cmd in HomeGame.Current.ChatCommands)
        {
            if(cmd != null && cmd.Name.ToLower() == parts[0].ToLower())
            {
                chatCommand = cmd;
            }
        }

        // If the command doesn't exist, return
        if(chatCommand == null) return;
        
        // If the command has no arguments, run it
        if(chatCommand.Arguments.Count == 0)
        {
            chatCommand.Run(client);
            return;
        }

        // If the command has arguments, check if the number of arguments is correct
        if(parts.Length - 1 != chatCommand.Arguments.Count)
        {
            Log.Info("🏠: Incorrect number of arguments");
            return;
        }

        // If the number of arguments is correct, run the command
        chatCommand.Run(client, parts[1..]);
    }
}
