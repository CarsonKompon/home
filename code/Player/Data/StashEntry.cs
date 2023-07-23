using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Sandbox;

namespace Home;

public partial class StashEntry : BaseNetworkable
{
	[Net] public long OwnerId { get; set; }
	[Net] public string Id { get; set; }
	[Net] public int Amount { get; set; }
	[Net] public int Used { get; set; }

	public StashEntry()
	{
		Id = "";
		Amount = 0;
		Used = 0;
	}

	public StashEntry(long owner, string id, int amount) : this()
	{
		OwnerId = owner;
		Id = id;
		Amount = amount;
	}

	public HomePlaceable GetPlaceable()
	{
		return HomePlaceable.All.FirstOrDefault(p => p.ResourceName == Id);
	}
}