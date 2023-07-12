using System.Linq;
using System;
using System.Collections.Generic;
using Sandbox;


namespace Home;

[ChatCommand]
public class KickChatCommand : ChatCommandAttribute
{
    public KickChatCommand()
    {
        Name = "Msg";
        Description = "Send a private message";
        Arguments = new List<ChatArgument>();
        Arguments.Add(new ChatArgument()
        {
            Name = "recipient",
            Description = "The recipient of the message",
            Type = typeof(string)
        });
        Arguments.Add(new ChatArgument()
        {
            Name = "reason",
            Description = "The message to send",
            Type = typeof(string)
        });
    }

    public override void Run(IClient client, string[] arguments)
    {
        string recipient = arguments[0];
        string message = "";
        for (int i = 1; i < arguments.Length; i++)
        {
            message += arguments[i] + " ";
        }
        IClient recipientClient = Game.Clients.FirstOrDefault(x => x.Name.ToLower().Contains(recipient.ToLower()), null);
        if (recipientClient == null)
        {
            HomeChatBox.AddChatEntry(To.Single(client), null, "Could not find a player with that name", null, "yellow");
            return;
        }
        var chatMsg = recipientClient.Name + " was kicked";
        if(message != "") chatMsg += " for \"" + message + "\"";
        recipientClient.Kick();
        HomeChatBox.AddChatEntry(To.Everyone, null, chatMsg, null, "yellow");
    }
}