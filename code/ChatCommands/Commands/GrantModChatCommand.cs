using System.Linq;
using System;
using System.Collections.Generic;
using Sandbox;


namespace Home;

[ChatCommand]
public class ModChatCommand : ChatCommandAttribute
{
    public ModChatCommand()
    {
        Name = "Mod";
        Description = "Grants moderator to a player";
        Arguments = new List<ChatArgument>();
        Arguments.Add(new ChatArgument()
        {
            Name = "user",
            Description = "The name of the user to grant moderator to",
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
        if (recipientClient == null || recipientClient.Pawn is not HomePlayer recipientPlayer)
        {
            HomeChatBox.AddChatEntry(To.Single(client), null, "Could not find a player with that name", null, "yellow");
            return;
        }
        recipientPlayer.IsModerator = true;
        HomeChatBox.AddChatEntry(To.Single(client), null, recipientClient.Name + " has been granted moderator permissions!", null, "yellow");
        HomeChatBox.AddChatEntry(To.Single(recipientClient), null, "You have been granted moderator permissions!", null, "yellow");
    }


    public override bool HasPermission(IClient client)
    {
        if(client.Pawn is not HomePlayer player) return false;
        return player.HasAdminPermissions();
    }
}