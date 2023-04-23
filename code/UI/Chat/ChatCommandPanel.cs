using System;
using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;



namespace ArcadeZone
{
	public partial class AZChatCommandPanel : Panel
	{

		public AZChatCommandPanel()
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

				foreach (var command in AZGame.Current.ChatCommands)
				{
					if(command.Name.ToLower().StartsWith(words[0].ToLower()))
					{
						AZChatCommandPanelEntry entry = new AZChatCommandPanelEntry(command);
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
