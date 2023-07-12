using System.Linq;
using System;
using System.Collections.Generic;
using Sandbox;


namespace Home;

[ChatCommand]
public class AnnounceChatCommand : ChatCommandAttribute
{
    public AnnounceChatCommand()
    {
        Name = "Announce";
        Description = "Announces to the server in chat";
        Arguments = new List<ChatArgument>();
        Arguments.Add(new ChatArgument()
        {
            Name = "message",
            Description = "The message to send",
            Type = typeof(string)
        });
    }

    public override void Run(IClient client, string[] arguments)
    {
        string message = "";
        for (int i = 0; i < arguments.Length; i++)
        {
            message += arguments[i] + " ";
        }
        string color = "white";
        // Check if the message contains quotes, if so parse the message within the quotes and set the color to the first word after the quotes
        if (message.Contains("\""))
        {
            string[] splitMessage = message.Split("\"");
            message = splitMessage[1];
            color = splitMessage[2].Split(" ")[1];
        }
        
        HomeChatBox.AddChatEntry(To.Everyone, null, message, null, color);
    }

    public override bool HasPermission(IClient client)
    {
        if(client.Pawn is not HomePlayer player) return false;
        return player.IsAdmin;
    }
}