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
	[ClientInput] public Vector3 PlacingPosition { get; set; } = Vector3.Zero;
	[ClientInput] public Rotation PlacingRotation { get; set; } = Rotation.Identity;
	[ClientInput] public float PlacingAngle { get; set; } = 0f;
	[ClientInput] public Entity MovingEntity { get; set; } = null;
	public bool CanPlace { get; set; } = false;

	public void TryPlace()
	{
		if(Placing == "") return;
		if(!CanPlace)
		{
			TryPickup();
			return;
		}

		ConsoleSystem.Run("home_try_place");
		StopPlacing();
	}

	public void TryPickup()
	{
		if(MovingEntity == null)
		{
			FinishPlacing();
			return;
		}
		
		ConsoleSystem.Run("home_try_pickup");
	}

	public void SetPlacing(HomePlaceable placeable)
	{
		Game.AssertClient();
		Placing = placeable.Id;
		PlacingGuide.StartPlacing(placeable);
		MovingEntity = null;
		CanPlace = true;
	}

	public void SetPlacing(Entity ent)
	{
		Game.AssertClient();
		PlaceableComponent component = ent.Components.Get<PlaceableComponent>();
		Placing = component.PlaceableId;
		HomePlaceable placeable = HomePlaceable.Find(component.PlaceableId);
		SetPlacing(placeable);
		MovingEntity = ent;
		CanPlace = true;
	}

	[ClientRpc]
	public void FinishPlacing()
	{
		Placing = "";
		PlacingGuide.StopPlacing();
		MovingEntity = null;
	}

	public void StopPlacing()
	{
		Game.AssertClient();
		PlacingGuide.StopPlacing();
		CanPlace = false;
	}

}