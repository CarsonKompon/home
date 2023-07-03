using System;
using System.Collections.Generic;
using Sandbox;


namespace Home;

[ChatCommand]
public class RtdChatCommand : ChatCommandAttribute
{
    public RtdChatCommand()
    {
        Name = "Rtd";
        Description = "Rolls the dice";
        Arguments = new List<ChatArgument>();
        Arguments.Add(new ChatArgument()
        {
            Name = "amount",
            Description = "The amount of sides on the dice",
            Type = typeof(int),
            Default = "6",
            Optional = true
        });
    }

    public override void Run(IClient client, string[] arguments)
    {
        int sides = int.Parse(arguments[0]);
        Random random = new Random();
        int i = random.Next(1, sides);
        HomeChatBox.AddChatEntry(To.Everyone, null, client.Name + " rolled a " + i.ToString() + " on a " + sides.ToString() + " sided die");
    }
}