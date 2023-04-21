using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;



namespace ArcadeZone
{
	public partial class AZChatSettingsEntry : Panel
	{
		public Label Name { get; internal set; }
		public Panel Control { get; internal set; }

		public AZChatSettingsEntry(string name, Panel control)
        {
            Name = Add.Label( name, "name" );
            Control = control;
            AddChild(Control);
        }

	}
}
