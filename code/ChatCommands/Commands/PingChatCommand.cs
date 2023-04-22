using System;
using System.Collections.Generic;
using Sandbox;


namespace ArcadeZone;

[ChatCommand]
public class PingChatCommand : ChatCommandAttribute
{
    public PingChatCommand()
    {
        Name = "Ping";
        Description = "Checks your current ping against the server";
        Arguments = new List<ChatArgument>();
    }

    public override void Run(IClient client)
    {
        AZChatBox.AddChatEntry(To.Single(client), null, "Pong! Your current ping is " + client.Ping + "ms");
    }
}