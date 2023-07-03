namespace Home
{
	public partial class HomeChatSettingsEntry : Panel
	{
		public Label Name { get; internal set; }
		public Panel Control { get; internal set; }

		public HomeChatSettingsEntry(string name, Panel control)
        {
            Name = Add.Label( name, "name" );
            Control = control;
            AddChild(Control);
        }

	}
}
