using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;



namespace Home
{
	public partial class HomeChatCommandPanelEntry : Panel
	{
		public Label Command { get; internal set; }
		public Label Arguments { get; internal set; }

		public HomeChatCommandPanelEntry(ChatCommandAttribute command)
		{
			Command = Add.Label( "/" + command.Name.ToLower(), "command" );
			string argString = command.GetArgumentTemplate();
			if(argString != "")
			{
				Arguments = Add.Label( argString, "arguments" );
			}
		}

	}
}
