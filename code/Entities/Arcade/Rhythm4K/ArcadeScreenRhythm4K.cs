using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Rhythm4K;

namespace Home;

public partial class ArcadeScreenRhythm4K : WorldPanel
{
    
    public ArcadeMachineRhythm4K Machine;
    public Rhythm4KSingleMenu Menu;
    public SongSelectPage GamePage;

    public ArcadeScreenRhythm4K()
    {
        StyleSheet.Load("/Entities/Arcade/Rhythm4K/ArcadeScreenRhythm4K.scss");

        AddClass("game-rhythm4k");
        
        Menu = AddChild<Rhythm4KSingleMenu>();

        float width = 765f;
        float height = 425f;
        PanelBounds = new Rect(-width/2, -height/2, width, height);
        Scale = 0.5f;
    }

    public ArcadeScreenRhythm4K(ArcadeMachineRhythm4K machine) : this()
    {
        Machine = machine;
        Menu.Machine = Machine;
    }

    public void StartGame()
    {
        Menu.Navigate("/songs");
    }

    [GameEvent.Tick.Client]
    public void OnTick()
    {
        Style.Opacity = MathX.Clamp(1.25f - (Vector3.DistanceBetween( Camera.Position, Position ) * 0.004f), 0f, 1f);
        
        
    }

}