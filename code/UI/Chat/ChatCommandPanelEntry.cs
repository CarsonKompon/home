using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;



namespace ArcadeZone
{
	public partial class AZChatCommandPanelEntry : Panel
	{
		public Label Command { get; internal set; }
		public Label Arguments { get; internal set; }

		public AZChatCommandPanelEntry(ChatCommandAttribute command)
		{
			Command = Add.Label( command.Name, "command" );
			Arguments = Add.Label( command.GetArgumentTemplate(), "arguments" );
		}

	}
}
