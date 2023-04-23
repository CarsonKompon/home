using System;
using System.Collections.Generic;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;



namespace ArcadeZone
{
	public partial class NotificationPanelEntry : Panel
	{

		public Label Text { get; internal set; }
		public Panel ProgressBar { get; internal set; }

		public RealTimeSince TimeSinceBorn = 0;
		public float TimeLength = 15f;

		public NotificationPanelEntry()
        {
			Text = Add.Label( "", "text" );

			ProgressBar = Add.Panel("progress-bar");
        }

		public override void Tick()
		{
			base.Tick();

			ProgressBar.Style.Width = Length.Percent(100f - ((TimeSinceBorn / TimeLength) * 100f));

			if(TimeSinceBorn > TimeLength && !HasClass("hidden"))
			{
				Delete();
			}
		}

	}
}
