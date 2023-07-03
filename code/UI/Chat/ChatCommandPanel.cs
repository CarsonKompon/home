namespace Home
{
	public partial class HomeChatCommandPanel : Panel
	{

		public HomeChatCommandPanel()
        {
        }

		private void Show()
		{
			SetClass("open", true);
		}

		private void Hide()
		{
			SetClass("open", false);
			DeleteChildren();
		}

		public void Update(string text)
		{
			if(text.StartsWith("/"))
			{
				Show();
				DeleteChildren();

				string[] words = text.Substring(1).Split(' ');

				foreach (var command in HomeGame.Current.ChatCommands)
				{
					if(command.Name.ToLower().StartsWith(words[0].ToLower()))
					{
						HomeChatCommandPanelEntry entry = new HomeChatCommandPanelEntry(command);
						AddChild(entry);
					}
				}
			}
			else if(HasClass("open"))
			{
				Hide();
			}
		}

	}
}
