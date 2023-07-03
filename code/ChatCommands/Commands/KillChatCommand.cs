
namespace Home;

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
        ConsoleSystem.Run("kill");
    }
}
