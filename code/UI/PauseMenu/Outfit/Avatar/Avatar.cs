﻿
using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.Razor;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Sandbox.Utility;

namespace Home;

public partial class Avatar : Panel
{
	enum DisplayType
	{
		Large,
		Small
	}

	record struct ItemGroup( string subcategory, List<Clothing> clothing );
	record struct ClothingCategory( Clothing.ClothingCategory category, string title, string icon, Vector3? position = default );

	public AvatarHud AvatarHud { get; set; }

	List<ClothingCategory> Categories = new( new[]
	{
		new ClothingCategory( Clothing.ClothingCategory.Skin, "Skin", "emoji_people" ),
		new ClothingCategory( Clothing.ClothingCategory.Facial, "Facial", "sentiment_very_satisfied", new( 0, 0, 8 ) ),
		new ClothingCategory( Clothing.ClothingCategory.Hair, "Hair", "face", new( 0, 0, 8 ) ),
		new ClothingCategory( Clothing.ClothingCategory.Hat, "Hat", "add_reaction", new( 0, 0, 8 ) ),
		new ClothingCategory( Clothing.ClothingCategory.Tops, "Tops", "personal_injury" ),
		new ClothingCategory( Clothing.ClothingCategory.Gloves, "Gloves", "front_hand" ),
		new ClothingCategory( Clothing.ClothingCategory.Bottoms, "Bottoms", "airline_seat_legroom_reduced" ),
		new ClothingCategory( Clothing.ClothingCategory.Footwear, "Footwear", "do_not_step" ),
	} );

	public Panel ClothingCanvas { get; set; }

	public ClothingContainer Container = new();
	public string PreviewClothing = "[]";
	public List<SceneModel> ClothingModels = new();
	DisplayType CurrentDisplayType = DisplayType.Large;

	public Clothing.ClothingCategory CurrentCategory { get; set; }

	VSlider HeightSlider { get; set; }

	string originalValue;
	RealTimeSince timeSinceSave = 0;
	float fieldOfView = 50.0f;
	float targetDistance = 25;
	bool displayingVariations = false;
	List<ItemGroup> ItemGroups = new();

	protected override void OnAfterTreeRender( bool firstTime )
	{
		base.OnAfterTreeRender( firstTime );

		if ( firstTime )
		{
			

			HeightSlider.OnValueChanged += ( value ) =>
			{
				AvatarHud.SetHeight( (1.0f - value) + 0.5f );
			};


			ChangeCategory( Clothing.ClothingCategory.Skin );

			Load();
		}

		AvatarHud.SetHeight( (1.0f - HeightSlider.Value) + 0.5f );
	}

	public void ChangeCategory( Clothing.ClothingCategory category )
	{
		if(Game.LocalPawn is not HomePlayer) return;
		
		CurrentCategory = category;

		ItemGroups.Clear();

		var subcategoryGroups = HomeClothing.GetAll(Game.LocalPawn as HomePlayer)
			.Where( x => x.Category == category && (x.Parent == null || CurrentDisplayType == DisplayType.Small) )
			.OrderBy( x => x.SubCategory )
			.GroupBy( x => x.SubCategory?.Trim() ?? string.Empty );

		foreach( var group in subcategoryGroups )
		{
			var key = string.IsNullOrEmpty( group.Key ) ? category.ToString() : group.Key;
			ItemGroups.Add( new ItemGroup( key, group.ToList() ) );
		}

		var categoryInfo = Categories.FirstOrDefault( x => x.category == category );
		var position = categoryInfo.position ?? new Vector3( 0, 0, 30 );

		targetDistance = position.z;

		ClothingCanvas.ScrollOffset = 0;

		selectedClothing = null;
		displayingVariations = false;
		StateHasChanged();
	}

	private Clothing selectedClothing;

	public void Select( Clothing clothing )
	{
		selectedClothing = clothing;

		var hasVariations = HasVariations( clothing );
		var isVariation = IsVariation( clothing );

		if( !hasVariations || displayingVariations )
		{
			AddClothing( clothing );
		}

		if ( isVariation || !hasVariations ) return;

		var variations = HomeClothing.All
			.Where( x => x == clothing || x.Parent == clothing )
			.ToList();

		displayingVariations = true;

		ItemGroups.Clear();
		ItemGroups.Add( new()
		{
			clothing = new( variations ),
			subcategory = clothing.Title
		} );
		ClothingCanvas.ScrollOffset = 0;
		StateHasChanged();
	}

	bool IsVariation( Clothing clothing )
	{
		return clothing.Parent != null;
	}

	bool HasVariations( Clothing clothing )
	{
		if(CurrentDisplayType == DisplayType.Small) return false;
		return HomeClothing.All.Any( x => x.Parent == clothing );
	}

	async void AddClothing( Clothing clothing )
	{
		if(clothing is HomeClothing hcloth && !string.IsNullOrEmpty(hcloth.CloudModel))
		{
			await hcloth.MountPackage();
		}

		Container.Toggle( clothing );
		
		SetClass( "is-dirty", true );

		DressModel();
	}

	void DressModel()
	{
		PreviewClothing = Container.Serialize();
		AvatarHud.Update(PreviewClothing);

		timeSinceSave = 0;
	}

	public override void OnMouseWheel( float value )
	{
		base.OnMouseWheel( value );

		targetDistance += value * 2.0f;

		targetDistance = targetDistance.Clamp( 4, 30 );
	}

	void Load()
	{
		if(Game.LocalPawn is not HomePlayer player) return;
		originalValue = player.ClothingString;
		PreviewClothing = originalValue;
		HeightSlider.Value = 1.0f - (player.Data.Height - 0.5f);

		Container.Deserialize( originalValue );
		DressModel();
	}

	public void CancelChanges()
	{
		SetClass( "is-dirty", false );

		Load();
	}

	public void SaveChanges()
	{
		if(Game.LocalPawn is not HomePlayer player) return;
		var str = Container.Serialize();
		Cookie.SetString("home.outfit", str);

		HomePlayer.ChangeOutfit(player.NetworkIdent, str, (1.0f - HeightSlider.Value) + 0.5f);
		HomeGUI.UpdateAvatar(str);

		SetClass( "is-dirty", false );
		timeSinceSave = 0;

		originalValue = player.ClothingString;
	}

	void ResetHeight()
	{
		HeightSlider.Value = 0.5f;
	}

	void DisplayTypeChanged()
	{
		ChangeCategory( CurrentCategory );
	}

	void SetLargeDisplay( )
	{
		CurrentDisplayType = DisplayType.Large;
		DisplayTypeChanged();
	}

	void SetSmallDisplay( )
	{
		CurrentDisplayType = DisplayType.Small;
		DisplayTypeChanged();
	}

	string CategoryClass()
	{
		if(CurrentDisplayType == DisplayType.Small) return "small";
		return "";
	}

	// protected override int BuildHash()
	// {
	// 	return HashCode.Combine(PreviewClothing);
	// }


}

