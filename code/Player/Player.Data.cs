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

public partial class PlayerData : BaseNetworkable
{
	[Net] public long SteamId { get; set; } = 0;
	[Net, Change] public long Money { get; set; } = 0;
	[Net] public float Height { get; set; } = 1f;
	[Net] public List<StashEntry> Stash { get; set; } = new();
	[Net] public List<int> Clothing { get; set; } = new();
	[Net] public List<AchievementProgress> Achievements {get; set;} = new();
	[Net] public List<HomeBadge> Badges {get; set;} = new();

	public PlayerData() {}

	public PlayerData(long steamId) : this()
	{
		SteamId = steamId;
	}

	// public void Load()
	// {
	// 	Game.AssertClient();
	// 	Log.Info("🏠: Loading player data file...");
	// 	string steamId = Game.LocalClient.SteamId.ToString();
	// 	if(!FileSystem.Data.DirectoryExists(steamId)) return;
	// 	LoadFromString(FileSystem.Data.ReadAllText(steamId + "/player.json"));
	// }

	public void LoadFromString(string jsonString)
	{
		var newData = JsonSerializer.Deserialize<PlayerData>(jsonString);

		Money = newData.Money;
		Stash = newData.Stash;
		Clothing = newData.Clothing;
		Height = newData.Height;
		Achievements = newData.Achievements;
		Badges = newData.Badges.Where(x => x.RequiresAuthority == false).ToList();

		HomePlayer.SetHeight(GetPlayer().NetworkIdent, Height);

		CombStash();

		Log.Info("🏠: Player data loaded from string!");
	}

	public void Save()
	{
		Game.AssertClient();
		if(SteamId == 0) return;
		Log.Info("🏠: Saving player data...");
		string steamId = Game.LocalClient.SteamId.ToString();
		if(!FileSystem.Data.DirectoryExists(steamId))
		{
			FileSystem.Data.CreateDirectory(steamId);
		}
		FileSystem.Data.WriteJson(steamId + "/player.json", this);
		Log.Info("🏠: Player data saved!");
	}

	public void SetAchievementProgress(string name, int progress)
	{
		var list = Achievements.Where(x => x.Name == name).ToList();
		AchievementProgress achievement;
		if(list.Count() == 0)
		{
			achievement = new AchievementProgress()
			{
				Name = name,
				Progress = progress,
				Unlocked = false
			};
			if(achievement.Progress >= HomeAchievement.Find(name).Goal)
			{
				AchievementUnlock(name);
			}
		}
		else
		{
			achievement = list[0];
			achievement.Progress = progress;
			if(achievement.Progress >= HomeAchievement.Find(name).Goal)
			{
				AchievementUnlock(name);
			}
		}
	}

	public void AddAchievementProgress(string name, int amount)
	{
		var list = Achievements.Where(x => x.Name == name).ToList();
		AchievementProgress achievement;
		if(list.Count() == 0)
		{
			achievement = new AchievementProgress()
			{
				Name = name,
				Progress = amount,
				Unlocked = false
			};
			if(achievement.Progress >= HomeAchievement.Find(name).Goal)
			{
				AchievementUnlock(name);
			}
		}
		else
		{
			achievement = list[0];
			achievement.Progress += amount;
			int _goal = HomeAchievement.Find(name).Goal;
			if(achievement.Progress >= _goal)
			{
				achievement.Progress = _goal;
				AchievementUnlock(name);
			}
		}
	}

	private void AchievementUnlock(string name)
	{

	}

	private void OnMoneyChanged(long oldMoney, long newMoney)
	{
		if(!Game.IsClient) return;
		if(SteamId != Game.LocalClient.SteamId) return;
		if(HomeGUI.Current == null) return;
		var change = HomeGUI.Current.MoneyChangesPanel.AddChild<MoneyChanged>();
		change.SetAmount(newMoney - oldMoney);
	}

	// Comb through the stash and remove any invalid entries
	private void CombStash()
	{
		for(int i = 0; i < Stash.Count; i++)
		{
			if(Stash[i].Amount <= 0 || Stash[i].GetPlaceable() == null)
			{
				Stash.RemoveAt(i);
				i--;
			}
			else
			{
				Stash[i].Used = 0;
			}
		}
	}

	public HomePlayer GetPlayer()
	{
		return Game.Clients.FirstOrDefault(c => c.SteamId == SteamId)?.Pawn as HomePlayer;
	}
}

public partial class HomePlayer
{
    [Net] public PlayerData Data { get; set; }
	public List<RoomLayout> RoomLayouts = new List<RoomLayout>();

	[ConVar.ClientData] public string HomeUploadData { get; set; } = "";

	[ClientRpc]
	public void SavePlayerDataClientRpc()
	{
		if(!FileSystem.Data.DirectoryExists(Client.SteamId.ToString()))
		{
			FileSystem.Data.CreateDirectory(Client.SteamId.ToString());
		}

		Data?.Save();
	}

	[ClientRpc]
	public void LoadPlayerDataClientRpc()
	{
		if(Game.LocalPawn is not HomePlayer player) return;

		// Load thumbnails for placeables
		// LoadPlaceableThumbnails();
		
		// Load thumbnails for playermodels
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
		if(string.IsNullOrEmpty(HomeUploadData))
		{
			HomeUploadData = JsonSerializer.Serialize(new PlayerData(Client.SteamId));
			Log.Info(HomeUploadData);
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

		foreach(string file in FileSystem.Data.FindFile(steamId + "/layouts/" + Game.Server.MapIdent, "*.json"))
		{
			RoomLayout layout = FileSystem.Data.ReadJson<RoomLayout>(steamId + "/layouts/" + Game.Server.MapIdent + "/" + file);
			player.RoomLayouts.Add(layout);
		}

		OnPlayerDataLoaded(Client.SteamId.ToString());
	}

	[ConCmd.Server]
	public static void OnPlayerDataLoaded(string steamIdString)
	{
		long steamId = long.Parse(steamIdString);
		IClient client = Game.Clients.FirstOrDefault(c => c.SteamId == steamId);
		if(client.Pawn is not HomePlayer player) return;
		player.Data = new PlayerData(steamId);
		player.Data.LoadFromString(client.GetClientData<string>("HomeUploadData"));
		player.LoadBadges();
	}

	public bool HasMoney(long amount)
	{
		return Data.Money >= amount;
	}

	public bool GiveMoney(long amount)
	{
		Data.Money += amount;
		Sandbox.Services.Stats.Increment(this.Client, "money_earned", amount);
		SavePlayerDataClientRpc(To.Single(this.Client));
		return true;
	}

	public bool TakeMoney(long amount)
	{
		if(!HasMoney(amount))
			return false;
		Data.Money -= amount;
		SavePlayerDataClientRpc(To.Single(this.Client));
		return true;
	}

	[ConCmd.Server]
	public static void SetHeight(int networkIdent, float height)
	{
		if(Entity.FindByIndex<HomePlayer>(networkIdent) is not HomePlayer player) return;
		player.SetAnimParameter("scale_height", height);
		if(player.Data != null)
		{
			player.Data.Height = Math.Clamp(height, 0.5f, 1.5f);
			player.SavePlayerDataClientRpc(To.Single(player));
		}
		//player.CreateHull();
	}

	public bool HasPlaceable(string id, int amount = 1)
	{
		return Data.Stash.Any(s => s.Id == id && s.Amount >= amount);
	}

	public bool CanUsePlaceable(string id, int amount = 1)
	{
		return Data.Stash.Any(s => s.Id == id && s.Amount - s.Used >= amount);
	}

	public void GivePlaceable(string id, long amount = 1)
	{
		if(Data.Stash.Any(s => s.Id == id))
		{
			Data.Stash.First(s => s.Id == id).Amount += (int)amount;
		}
		else
		{
			Data.Stash.Add(new StashEntry(this.Client.SteamId, id, (int)amount));
		}
		SavePlayerDataClientRpc(To.Single(this.Client));
	}

	public void GiveClothing(int id)
	{
		if(Data.Clothing.Contains(id)) return;
		Data.Clothing.Add(id);
		SavePlayerDataClientRpc(To.Single(this.Client));
	}

	public bool TakePlaceable(string id, int amount = 1)
	{
		if(!HasPlaceable(id, amount))
			return false;
		Data.Stash.First(s => s.Id == id).Amount -= (int)amount;
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
		if(!Data.Stash.Any(s => s.Id == id && s.Used >= amount)) return;
		var thing = Data.Stash.First(s => s.Id == id);
		thing.Used -= amount;
		if(thing.Used < 0) thing.Used = 0;
	}

	public void GiveBadge(string id)
	{
		Game.AssertServer();
		var badge = HomeBadge.FindById(id);
		if(badge == null) return;
		if(Data.Badges.Contains(badge)) return;
		Data.Badges.Add(badge);
		SavePlayerDataClientRpc(To.Single(this.Client));
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