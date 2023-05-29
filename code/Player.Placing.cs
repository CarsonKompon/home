using System.Net.WebSockets;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Home;

public partial class HomePlayer
{
	[ClientInput] public string Placing { get; set; } = "";
    [ClientInput] public string PlacingModel { get; set; } = "";
	[ClientInput] public Vector3 PlacingPosition { get; set; } = Vector3.Zero;
	[ClientInput] public float PlacingRotation { get; set; } = 0f;
	public bool CanPlace { get; set; } = false;

	public void TryPlace()
	{
		if(Placing == "") return;
		if(!CanPlace) return;

		ConsoleSystem.Run("home_try_place");
	}

}