using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Sandbox;

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
	[Net] public IList<int> BadgeIds {get; set;} = new List<int>();
	[Net] public int CurrentPet {get; set;} = 0; 

	public PlayerData() {}

	public PlayerData(long steamId) : this()
	{
		SteamId = steamId;
	}

	public void LoadFromString(string jsonString)
	{
		var newData = JsonSerializer.Deserialize<PlayerData>(jsonString);

		Money = newData.Money;
		Clothing = newData.Clothing;
		Height = newData.Height;
		Achievements = newData.Achievements;
		Pets = newData.Pets;
		CurrentPet = newData.CurrentPet;

		// Authorize badges
		BadgeIds = new List<int>();
		foreach(var badgeId in newData.BadgeIds)
		{
			var badge = HomeBadge.Find(badgeId);
			if(!badge.RequiresAuthority) BadgeIds.Add(badgeId);
		}

        // Filter out duplicate stash
        Stash = new List<StashEntry>();
        foreach(var entry in newData.Stash)
        {
            if(Stash.FirstOrDefault(x => x.Id == entry.Id) == null)
            {
                Stash.Add(entry);
            }
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
		foreach(var badgeId in BadgeIds)
		{
			badges.Add(HomeBadge.Find(badgeId));
		}
		return badges;
	}

	public HomePlayer GetPlayer()
	{
		return Game.Clients.FirstOrDefault(c => c.SteamId == SteamId)?.Pawn as HomePlayer;
	}
}