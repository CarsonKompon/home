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
	[Net] public IList<StashEntry> Stash { get; set; } = new List<StashEntry>();
	[Net] public IList<int> Clothing { get; set; } = new List<int>();
	[Net] public IList<AchievementProgress> Achievements {get; set;} = new List<AchievementProgress>();
	[Net] public IList<int> Pets {get; set;} = new List<int>();
	[Net] public IList<int> Badges {get; set;} = new List<int>();
	[Net] public int CurrentPet {get; set;} = 0; 

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
		Pets = newData.Pets;
		CurrentPet = newData.CurrentPet;

		// Authorize badges
		Badges = new List<int>();
		foreach(var badgeId in newData.Badges)
		{
			var badge = HomeBadge.Find(badgeId);
			if(!badge.RequiresAuthority) Badges.Add(badgeId);
		} 

		GetPlayer()?.SetHeight(Height);

		CombStash();
	}

	public void Save()
	{
		Game.AssertClient();
		if(SteamId == 0) return;
		Log.Info("Saving player data...");
		string steamId = Game.LocalClient.SteamId.ToString();
		if(!FileSystem.Data.DirectoryExists(steamId))
		{
			FileSystem.Data.CreateDirectory(steamId);
		}
		FileSystem.Data.WriteJson(steamId + "/player.json", this);
		Log.Info("Player data saved!");
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

	public List<HomeBadge> GetBadges()
	{
		var badges = new List<HomeBadge>();
		foreach(var badge in Badges)
		{
			badges.Add(HomeBadge.Find(badge));
		}
		return badges;
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
	[Net] public Pet PetEntity { get; set; }

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

		// Load player data from client data
		HomeUploadData = FileSystem.Data.ReadAllText(Client.SteamId.ToString() + "/player.json");
		if(string.IsNullOrEmpty(HomeUploadData))
		{
			HomeUploadData = JsonSerializer.Serialize(new PlayerData(Client.SteamId));
		}

		// Load Clothing
		string clothing = Cookie.GetString("home.outfit", "");
		if(clothing != "")
		{
			ClothingString = clothing;
			HomeGUI.UpdateAvatar(clothing);
		}
		else
		{
			clothing = "default";
		}

		// Load Admin Settings
		player.LoadAdmin();

		// Create layout folder structure
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

		// Load local layouts
		foreach(string file in FileSystem.Data.FindFile(steamId + "/layouts/" + Game.Server.MapIdent, "*.json"))
		{
			RoomLayout layout = FileSystem.Data.ReadJson<RoomLayout>(steamId + "/layouts/" + Game.Server.MapIdent + "/" + file);
			player.RoomLayouts.Add(layout);
		}

		// Tell the server we loaded the player's data
		OnPlayerDataLoaded(Client.SteamId, clothing);
	}

	[ConCmd.Server]
	public static void OnPlayerDataLoaded(long steamId, string clothing = "")
	{
		IClient client = Game.Clients.FirstOrDefault(c => c.SteamId == steamId);
		if(client.Pawn is not HomePlayer player) return;
		Log.Info("Loading player data for " + client.Name);
		player.Data = new PlayerData(steamId);
		player.Data.LoadFromString(client.GetClientData<string>("HomeUploadData"));
		player.LoadBadges();

		// Set Clothing
		if(clothing == "default")
		{
			LoadDefaultOutfit(steamId);
		}
		else if(clothing != "")
		{
			player.ChangeOutfit(clothing);
		}

		// Set Pet
		HomePlayer.SetPet(player.NetworkIdent, player.Data.CurrentPet);
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
		Sandbox.Services.Stats.Increment(this.Client, "money_spent", amount);
		SavePlayerDataClientRpc(To.Single(this.Client));
		return true;
	}

	[ConCmd.Server]
	public static void SetHeight(int networkIdent, float height)
	{
		if(Entity.FindByIndex<HomePlayer>(networkIdent) is not HomePlayer player) return;
		player.SetHeight(height);
		if(player.Data != null) player.SavePlayerDataClientRpc(To.Single(player));
	}

	public void SetHeight(float height)
	{
		SetAnimParameter("scale_height", height);
		if(Data != null)
		{
			Data.Height = Math.Clamp(height, 0.5f, 1.5f);
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

	public void GivePet(int id)
	{
		if(Data.Pets.Contains(id)) return;
		Data.Pets.Add(id);
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
		if(Data.Badges.Contains(badge.ResourceId)) return;
		Data.Badges.Add(badge.ResourceId);
		Log.Info("Gave badge " + id + " to " + Client.Name);
	}

	[ConCmd.Server]
	public static void SetPet(int networkIdent, int petId)
	{
		if(Entity.FindByIndex<HomePlayer>(networkIdent) is not HomePlayer player) return;
		if(petId != 0 && !player.Data.Pets.Contains(petId)) return;
		if(player.PetEntity.IsValid()) player.PetEntity.Delete();
		if(petId != 0)
		{
			var pet = HomePet.Find(petId);

			// Getting a type that matches the name
			var entityType = TypeLibrary.GetType<Pet>( pet.ClassName )?.TargetType;
			if ( entityType == null ) return;

			// Creating an instance of that type
			Pet entity = TypeLibrary.Create<Pet>( entityType );
			if(entity == null) return;

			// Setting the entity's position
			entity.Position = player.Position;
			entity.Rotation = player.Rotation;
			entity.Transmit = TransmitType.Always;
			entity.Player = player;

			player.PetEntity = entity;
		}
		player.Data.CurrentPet = petId;
		player.SavePlayerDataClientRpc(To.Single(player));
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