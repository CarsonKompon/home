using System.Linq;
using System;
using System.Collections.Generic;
using Sandbox;


namespace Home;

[ChatCommand]
public class GiveMoneyChatCommand : ChatCommandAttribute
{
    public GiveMoneyChatCommand()
    {
        Name = "GiveMoney";
        Description = "Gives money to a player";
        Arguments = new List<ChatArgument>();
        Arguments.Add(new ChatArgument()
        {
            Name = "amount",
            Description = "The amount of money to give",
            Type = typeof(int)
        });
        Arguments.Add(new ChatArgument()
        {
            Name = "user",
            Description = "The name of the user to give money to",
            Type = typeof(string),
            Default = null,            
            Optional = true
        });
    }

    public override void Run(IClient client, string[] arguments)
    {
        int amount = Convert.ToInt32(arguments[0]);
        string recipient = client.Name;
        if(arguments.Length > 1 && arguments[1] != null)
        {
            recipient = arguments[1];
        }
        IClient recipientClient = Game.Clients.FirstOrDefault(x => x.Name.ToLower().Contains(recipient.ToLower()), null);
        if (recipientClient == null || recipientClient.Pawn is not HomePlayer recipientPlayer)
        {
            HomeChatBox.AddChatEntry(To.Single(client), null, "Could not find a player with that name", null, "yellow");
            return;
        }
        recipientPlayer.GiveMoney(amount);
    }


    public override bool HasPermission(IClient client)
    {
        if(client.Pawn is not HomePlayer player) return false;
        return player.HasAdminPermissions();
    }
}