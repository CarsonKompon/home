using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Editor;
using System.Linq;

namespace Home;

/// <summary>
/// A placeable TV that you can queue media on
/// </summary>
[EditorModel("models/sbox_props/office_chair/office_chair.vmdl")]
public partial class ChairBase : ModelEntity, IUse
{

    [Net] public HomePlayer CurrentUser { get; set; } = null;
    public virtual Transform SeatOffset => Transform.Zero;
    public virtual Transform ExitOffset => Transform.Zero;
    public virtual bool IsUsable( Entity user ) => CurrentUser == null;

    public override void Spawn()
    {
        base.Spawn();
        SetModel("models/sbox_props/office_chair/office_chair.vmdl");
        SetupPhysicsFromModel(PhysicsMotionType.Keyframed);
    }

    public void SetUser(HomePlayer player)
    {
        if(player.Controller is ArcadeControllerBase controller)
        {
            NotificationPanel.AddEntry(To.Single(player), "🚫 You are already using an arcade machine.", "", 3);
            return;
        }
        ChairController chairController = new ChairController();
        chairController.Chair = this;
        player.Controller = chairController;
        CurrentUser = player;
        
        var attachment = GetAttachment("Seat");
        player.SetParent(this, "seat", SeatOffset);
    }

    public void RemoveUser()
    {
        if(CurrentUser == null) return;
        
        CurrentUser.SetParent(null, null, Transform.Zero);

        if(CurrentUser.Controller is not HomeWalkController)
        {
            CurrentUser.Controller = new HomeWalkController();
        }
        CurrentUser = null;
    }

    public virtual bool OnUse(Entity user)
    {
        Game.AssertServer();
        if(user is not HomePlayer player) return false;
        if(CurrentUser == null)
        {
            SetUser(player);
        }
        return false;
    }
}