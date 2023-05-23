using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;



namespace Home
{
	public partial class HomeChatEntry : Panel
	{
		public Label NameLabel { get; internal set; }
		public Label Message { get; internal set; }
		public Image Avatar { get; internal set; }

		public RealTimeSince TimeSinceBorn = 0;

		public HomeChatEntry()
		{
			Avatar = Add.Image();
			NameLabel = Add.Label( "Name", "name" );
			Message = Add.Label( "Message", "message" );
		}

		public override void Tick()
		{
			base.Tick();

			if ( TimeSinceBorn > 10 && !HasClass("fade") )
			{
				AddClass("fade");
			}
		}
	}
}
