using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using Tetros;

namespace Home;

public partial class ArcadeScreenSnek : WorldPanel
{
    
    public ArcadeMachineSnek Machine;
    // public TetrosSingleMenu Menu;

    public ArcadeScreenSnek()
    {

        AddClass("game-snek");
        
        // Menu = AddChild<TetrosSingleMenu>();

        float width = 590f;
        float height = 460f;
        PanelBounds = new Rect(-width/2, -height/2 - 5f, width, height);
        //Scale = 0.5f;
    }

    public ArcadeScreenSnek(ArcadeMachineSnek machine) : this()
    {
        Machine = machine;
        // Menu.Machine = Machine;
    }

    public void StartGame()
    {
        // Menu.Navigate("/game");
        // Menu.StartGame.Invoke();
    }

    [GameEvent.Tick.Client]
    public void OnTick()
    {
        Style.Opacity = MathX.Clamp(1.25f - (Vector3.DistanceBetween( Camera.Position, Position ) * 0.004f), 0f, 1f);
        
        
    }

}