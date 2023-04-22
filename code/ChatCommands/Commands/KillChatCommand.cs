using System;
using System.Collections.Generic;
using Sandbox;


namespace ArcadeZone;

[ChatCommand]
public class KillChatCommand : ChatCommandAttribute
{
    public KillChatCommand()
    {
        Name = "Kill";
        Description = "Commits suicide";
        Arguments = new List<ChatArgument>();
    }

    public override void Run(IClient client)
    {
        client.SendCommandToClient("kill");
    }
}