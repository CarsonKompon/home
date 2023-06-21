using System.Net.WebSockets;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Home.Util;

namespace Home;

public class PlayerData
{
	public long SteamId { get; set; }
	public long Money { get; set; }
	public List<StashEntry> Stash { get; set; }

	public PlayerData()
	{
		Money = 0;
		Stash = new List<StashEntry>();
	}

	public PlayerData(long steamId) : this()
	{
		SteamId = steamId;
	}

	public void Save()
	{
		Game.AssertClient();
		Log.Info("🏠: Saving player data...");
		string steamId = Game.LocalClient.SteamId.ToString();
		if(!FileSystem.Data.DirectoryExists(steamId))
		{
			FileSystem.Data.CreateDirectory(steamId);
		}
		FileSystem.Data.WriteJson(steamId + "/player.json", this);
		Log.Info("🏠: Player data saved!");
	}	
}

public partial class HomePlayer
{
    [Net, Change] public long Money { get; set; }
	[Net] public IList<StashEntry> Stash { get; set; }
	public List<RoomLayout> RoomLayouts = new List<RoomLayout>();

	[ConVar.ClientData] public string HomeUploadData { get; set; } = "";

	[ClientRpc]
	public void SavePlayerDataClientRpc()
	{
		if(!FileSystem.Data.DirectoryExists(Client.SteamId.ToString()))
		{
			FileSystem.Data.CreateDirectory(Client.SteamId.ToString());
		}
		
		if(Stash == null) return;

		// Save player data to client data
		FileSystem.Data.WriteJson(Client.SteamId.ToString() + "/player.json", new PlayerData(Client.SteamId)
		{
			Money = Money,
			Stash = Stash.ToList()
		});
		
	}

	[ClientRpc]
	public void LoadPlayerDataClientRpc()
	{
		if(Game.LocalPawn is not HomePlayer player) return;

		foreach(HomePlaceable placeable in HomePlaceable.All)
		{
			if(string.IsNullOrEmpty(placeable.ThumbnailOverride) && !string.IsNullOrEmpty(placeable.Model))
			{
				placeable.Texture = SceneHelper.CreateModelThumbnail(placeable.Model);
			}
			else
			{
				placeable.Texture = Texture.Load(placeable.ThumbnailOverride);
			}
		}

		foreach(HomePlayermodel playermodel in HomePlayermodel.All)
		{
			if(string.IsNullOrEmpty(playermodel.ThumbnailOverride) && !string.IsNullOrEmpty(playermodel.Model))
			{
				playermodel.Texture = SceneHelper.CreateModelThumbnail(playermodel.Model);
			}
			else
			{
				playermodel.Texture = Texture.Load(playermodel.ThumbnailOverride);
			}
		}

		// Load player data from client data
		HomeUploadData = FileSystem.Data.ReadAllText(Client.SteamId.ToString() + "/player.json");
		if(HomeUploadData == null)
		{
			HomeUploadData = JsonSerializer.Serialize(new PlayerData(Client.SteamId));
		}

		// Load local layouts
		long steamId = Game.LocalClient.SteamId;
		if(!FileSystem.Data.DirectoryExists(steamId.ToString()))
		{
			FileSystem.Data.CreateDirectory(steamId.ToString());
		}
		if(!FileSystem.Data.DirectoryExists(steamId + "/layouts"))
		{
			FileSystem.Data.CreateDirectory(steamId + "/layouts");
		}
		if(!FileSystem.Data.DirectoryExists(steamId + "/layouts/" + Game.Server.MapIdent))
		{
			FileSystem.Data.CreateDirectory(steamId + "/layouts/" + Game.Server.MapIdent);
		}

		// TODO: Remove this early compatibility code
		if(Game.Server.MapIdent.Contains("home_staycation_strip"))
		{
			foreach(string file in FileSystem.Data.FindFile(steamId + "/layouts", "*.json"))
			{
				RoomLayout layout = FileSystem.Data.ReadJson<RoomLayout>(steamId + "/layouts/" + file);
				FileSystem.Data.DeleteFile(steamId + "/layouts/" + file);
				FileSystem.Data.WriteJson(steamId + "/layouts/" + Game.Server.MapIdent + "/" + file, layout);
			}
		}
		foreach(string file in FileSystem.Data.FindFile(steamId + "/layouts/" + Game.Server.MapIdent, "*.json"))
		{
			RoomLayout layout = FileSystem.Data.ReadJson<RoomLayout>(steamId + "/layouts/" + Game.Server.MapIdent + "/" + file);
			player.RoomLayouts.Add(layout);
		}

		ConsoleSystem.Run("home_player_data_loaded", Client.SteamId.ToString());
	}

	[ConCmd.Server("home_player_data_loaded")]
	public static void OnPlayerDataLoaded(string steamIdString)
	{
		long steamId = long.Parse(steamIdString);
		IClient client = Game.Clients.FirstOrDefault(c => c.SteamId == steamId);
		PlayerData data = JsonSerializer.Deserialize<PlayerData>(client.GetClientData<string>("HomeUploadData"));
		if(data == null)
		{
			data = new PlayerData(steamId);
		}
		if(client.Pawn is HomePlayer player)
		{
			for(int i = 0; i < data.Stash.Count; i++)
			{
				if(data.Stash[i].Amount <= 0 || data.Stash[i].GetPlaceable() == null)
				{
					data.Stash.RemoveAt(i);
					i--;
				}
				else
				{
					data.Stash[i].Used = 0;
				}
			}

			Log.Info("🏠: Player data loaded!");
			player.Money = data.Money;
			player.Stash = data.Stash;
		}
	}

	public bool HasMoney(long amount)
	{
		return Money >= amount;
	}

	public bool GiveMoney(long amount)
	{
		Money += amount;
		SavePlayerDataClientRpc(To.Single(this.Client));
		return true;
	}

	public bool TakeMoney(long amount)
	{
		if(!HasMoney(amount))
			return false;
		Money -= amount;
		SavePlayerDataClientRpc(To.Single(this.Client));
		return true;
	}

	public void OnMoneyChanged(long oldMoney, long newMoney)
	{
		if(!Game.IsClient) return;
		if(Client.SteamId != Game.LocalClient.SteamId) return;
		if(HomeGUI.Current == null) return;
		var change = HomeGUI.Current.MoneyChangesPanel.AddChild<MoneyChanged>();
		change.SetAmount(newMoney - oldMoney);
	}

	public bool HasPlaceable(string id, int amount = 1)
	{
		return Stash.Any(s => s.Id == id && s.Amount >= amount);
	}

	public bool CanUsePlaceable(string id, int amount = 1)
	{
		return Stash.Any(s => s.Id == id && s.Amount - s.Used >= amount);
	}

	public void GivePlaceable(string id, long amount = 1)
	{
		if(Stash.Any(s => s.Id == id))
		{
			Stash.First(s => s.Id == id).Amount += (int)amount;
		}
		else
		{
			Stash.Add(new StashEntry(this.Client.SteamId, id, (int)amount));
		}
		SavePlayerDataClientRpc(To.Single(this.Client));
	}

	public bool TakePlaceable(string id, int amount = 1)
	{
		if(!HasPlaceable(id, amount))
			return false;
		Stash.First(s => s.Id == id).Amount -= (int)amount;
		SavePlayerDataClientRpc(To.Single(this.Client));
		return true;
	}

	public bool UsePlaceable(string id, int amount = 1)
	{
		if(!CanUsePlaceable(id, amount))
			return false;
		//Stash.First(s => s.Id == id).Used += amount;
		return true;
	}

	[ClientRpc]
	public void UnusePlaceable(string id, int amount = 1)
	{
		if(!Stash.Any(s => s.Id == id && s.Used >= amount)) return;
		var thing = Stash.First(s => s.Id == id);
		thing.Used -= amount;
		if(thing.Used < 0) thing.Used = 0;
	}

}

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
		return HomePlaceable.All.FirstOrDefault(p => p.Id == Id);
	}
}