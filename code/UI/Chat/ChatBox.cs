using System.Drawing;
using Sandbox;
using Sandbox.Diagnostics;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace Home
{

	[StyleSheet]
	public partial class HomeChatBox : Panel
	{
		public static HomeChatBox Current;

		public Panel Canvas { get; protected set; }
		public Panel InputCanvas { get; protected set; }

		public TextEntry Input { get; protected set; }
		public HomeChatCommandPanel CommandPanel { get; protected set; }

		public Button BtnSettings { get; protected set; }

		public HomeChatSettings Settings { get; protected set; }

		private int MessageCount = 0;
		public bool MessageSounds = true;

		public HomeChatBox()
		{
			Current = this;

			StyleSheet.Load( "/ui/chat/ChatBox.scss" );

			Canvas = Add.Panel( "chat_canvas" );

			CommandPanel = AddChild<HomeChatCommandPanel>();

			InputCanvas = Add.Panel( "input_canvas" );

			Input = InputCanvas.Add.TextEntry( "" );
			Input.AddEventListener( "onsubmit", () => Submit() );
			Input.AddEventListener( "onblur", () => Close() );
			Input.AcceptsFocus = true;
			Input.AllowEmojiReplace = true;

			BtnSettings = InputCanvas.Add.Button( "⚙️", "btn-settings");
			BtnSettings.AddEventListener( "onclick", OnBtnSettings );

			Settings = AddChild<HomeChatSettings>();
		}

		void Open()
		{
			AddClass( "open" );
			Input.Focus();
		}

		void Close()
		{
			RemoveClass( "open" );
			Input.Blur();
		}

		public override void Tick()
		{
			base.Tick();

			if ( Sandbox.Input.Pressed( "chat" ) )
			{
				Open();
			}

			if(HasClass("open"))
			{
				CommandPanel.Update(Input.Text);
			}
		}

		void Submit()
		{
			Close();

			var msg = Input.Text.Trim();
			Input.Text = "";

			if ( string.IsNullOrWhiteSpace( msg ) )
				return;

			SayMessage( msg );
		}

		public void AddEntry( string name, string message, string avatar, string color = "", string nameColor = "")
		{
			var e = Canvas.AddChild<HomeChatEntry>();
			Canvas.SetChildIndex(e, 0);

			e.Message.Text = message;
			e.NameLabel.Text = name + ":";
			e.Avatar.SetTexture( avatar );

			e.SetClass( "noname", string.IsNullOrEmpty( name ) );
			e.SetClass( "noavatar", string.IsNullOrEmpty( avatar ) );

			if(color == "rainbow")
			{
				e.Message.AddClass("rainbow");
			}
			else
			{
				if(color == "") e.Message.Style.FontColor = Color.White;
				else e.Message.Style.FontColor = color;
			}

			if(nameColor == "rainbow")
			{
				e.NameLabel.AddClass("rainbow");
			}
			else
			{
				if(color == "") e.NameLabel.Style.FontColor = "#c0fb2e";
				else e.NameLabel.Style.FontColor = nameColor;
			}

			if(Canvas.ChildrenCount > 128)
			{
				Canvas.GetChild(Canvas.ChildrenCount - 1).Delete();
			}

			if(MessageSounds)
			{
				Audio.Play( "ui.chat.message" + ((MessageCount  % 2) + 1).ToString());
			}

			MessageCount++;
		}

		[ConCmd.Admin("home_announce", Help = "Announces a message to all players in chat")]
		public static void Announce( string message, string color = "yellow" )
		{
			AddChatEntry( To.Everyone, null, message, null, color);
		}


		[ClientRpc]
		public static void AddChatEntry( string name, string message, string avatar = null, string color = "", string nameColor = "" )
		{
			Current?.AddEntry( name, message, avatar, color, nameColor );

			// Only log clientside if we're not the listen server host
			if ( !Game.IsListenServer )
			{
				Log.Info( $"{name}: {message}" );
			}
		}

		[ConCmd.Server]
		public static void SayMessage( string message )
		{
			Assert.NotNull( ConsoleSystem.Caller );

			// todo - reject more stuff
			if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
				return;

			Log.Info( $">> {ConsoleSystem.Caller?.Name ?? "Server"}: {message}" );

			// Check for command
			if( message.StartsWith( "/" ) )
			{
				ChatCommandAttribute.Parse( ConsoleSystem.Caller, message );
				return;
			}
      
	  		if(ConsoleSystem.Caller?.Pawn is HomePlayer player && player.HasOwnerPermissions())
			{
				AddChatEntry( To.Everyone, ConsoleSystem.Caller?.Name ?? "Server", message, $"avatar:{ConsoleSystem.Caller?.SteamId}", "", "rainbow");
			}
			else
			{
				AddChatEntry( To.Everyone, ConsoleSystem.Caller?.Name ?? "Server", message, $"avatar:{ConsoleSystem.Caller?.SteamId}" );
			}
		}

		protected override void OnAfterTreeRender( bool firstTime )
		{
			base.OnAfterTreeRender( firstTime );

			Input.AcceptsFocus = true;
			Input.AllowEmojiReplace = true;

			Current = this;
		}

		void OnBtnSettings()
		{
			Settings.SetClass( "open", !Settings.HasClass( "open" ) );
			if(Settings.HasClass( "open" ))
			{
				Audio.Play("ui.navigate.forward");
			}
			else
			{
				Audio.Play("ui.navigate.back");
			}
		}
	}
}
