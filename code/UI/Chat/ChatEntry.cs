using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;



namespace ArcadeZone
{
	public partial class AZChatEntry : Panel
	{
		public Label NameLabel { get; internal set; }
		public Label Message { get; internal set; }
		public Image Avatar { get; internal set; }

		public RealTimeSince TimeSinceBorn = 0;

		public AZChatEntry()
		{
			Avatar = Add.Image();
			NameLabel = Add.Label( "Name", "name" );
			Message = Add.Label( "Message", "message" );
		}

		public override void Tick()
		{
			base.Tick();

			if ( TimeSinceBorn > 10 && !HasClass("hidden"))
			{
				AddClass("hidden");
			}
		}
	}
}
