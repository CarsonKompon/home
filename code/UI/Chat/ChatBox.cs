using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

namespace ArcadeZone
{
	public partial class AZChatBox : Panel
	{
		public static AZChatBox Current;

		public Panel Canvas { get; protected set; }
		public Panel InputCanvas { get; protected set; }

		public TextEntry Input { get; protected set; }

		public Button BtnSettings { get; protected set; }

		public AZChatSettings Settings { get; protected set; }

		private int MessageCount = 0;

		public AZChatBox()
		{
			Current = this;

			StyleSheet.Load( "/ui/chat/ChatBox.scss" );

			Canvas = Add.Panel( "chat_canvas" );

			InputCanvas = Add.Panel( "input_canvas" );

			Input = InputCanvas.Add.TextEntry( "" );
			Input.AddEventListener( "onsubmit", () => Submit() );
			Input.AddEventListener( "onblur", () => Close() );
			Input.AcceptsFocus = true;
			Input.AllowEmojiReplace = true;

			BtnSettings = InputCanvas.Add.Button( "⚙️", "btn-settings");
			BtnSettings.AddEventListener( "onclick", OnBtnSettings );

			Settings = AddChild<AZChatSettings>();
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
		}

		void Submit()
		{
			Close();

			var msg = Input.Text.Trim();
			Input.Text = "";

			if ( string.IsNullOrWhiteSpace( msg ) )
				return;

			Say( msg );
		}

		public void AddEntry( string name, string message, string avatar, string lobbyState = null )
		{
			var e = Canvas.AddChild<AZChatEntry>();
			Canvas.SetChildIndex(e, 0);

			e.Message.Text = message;
			e.NameLabel.Text = name + ":";
			e.Avatar.SetTexture( avatar );

			e.SetClass( "noname", string.IsNullOrEmpty( name ) );
			e.SetClass( "noavatar", string.IsNullOrEmpty( avatar ) );

			if ( lobbyState == "ready" || lobbyState == "staging" )
			{
				e.SetClass( "is-lobby", true );
			}

			if(Canvas.ChildrenCount > 128)
			{
				Canvas.GetChild(Canvas.ChildrenCount - 1).Delete();
			}

			Audio.Play( "ui.chat.message" + ((MessageCount  % 2) + 1).ToString());

			MessageCount++;
		}


		[ConCmd.Client( "chat_add", CanBeCalledFromServer = true )]
		public static void AddChatEntry( string name, string message, string avatar = null, string lobbyState = null )
		{
			Current?.AddEntry( name, message, avatar, lobbyState );

			// Only log clientside if we're not the listen server host
			if ( !Game.IsListenServer )
			{
				Log.Info( $"{name}: {message}" );
			}
		}

		[ConCmd.Client( "chat_addinfo", CanBeCalledFromServer = true )]
		public static void AddInformation( string message, string avatar = null )
		{
			Current?.AddEntry( null, message, avatar );
		}

		[ConCmd.Server( "say" )]
		public static void Say( string message )
		{
			// todo - reject more stuff
			if ( message.Contains( '\n' ) || message.Contains( '\r' ) )
				return;

			Log.Info( $"{ConsoleSystem.Caller}: {message}" );
      
			AddChatEntry( To.Everyone, ConsoleSystem.Caller?.Name ?? "Server", message, $"avatar:{ConsoleSystem.Caller?.SteamId}" );
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
