using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using CarsonsWebArcade;

namespace Home;

public partial class ArcadeScreenCarsonWeb : WorldPanel
{
    
    public ArcadeMachineCarsonWeb Machine;
    public AttractMenuPage Menu;

    public ArcadeScreenCarsonWeb()
    {

        AddClass("game-carson-web");
        
        Menu = AddChild<AttractMenuPage>();

        float width = 590f;
        float height = 460f;
        PanelBounds = new Rect(-width/2, -height/2 - 5f, width, height);
        //Scale = 0.5f;
    }

    public ArcadeScreenCarsonWeb(ArcadeMachineCarsonWeb machine) : this()
    {
        Machine = machine;
        // Menu.Machine = Machine;
    }

    public void StartGame()
    {
        // Menu.Navigate("/game");
        // Menu.StartGame.Invoke();
        var menu = Game.RootPanel.AddChild<WebArcadeMenu>();
        menu.OnClose = () => {
            EndGame();
            ArcadeMachineBase.RequestRemoveUser(Machine.NetworkIdent);
        };
    }

    public void EndGame()
    {
        foreach(var child in Game.RootPanel.Children)
        {
            if(child is WebArcadeMenu)
            {
                child.Delete();
            }
        }
    }

    [GameEvent.Tick.Client]
    public void OnTick()
    {
        Style.Opacity = MathX.Clamp(1.25f - (Vector3.DistanceBetween( Camera.Position, Position ) * 0.004f), 0f, 1f);
        
        
    }

}