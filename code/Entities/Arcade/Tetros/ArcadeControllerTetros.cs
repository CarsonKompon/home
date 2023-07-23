using System;
using Sandbox;
using Tetros;

namespace Home;

public class ArcadeControllerTetros : ArcadeControllerBase
{
    public override void Simulate()
    {
        base.Simulate();

        ArcadeMachine?.SetAnimParameter("left", Input.Down("TetrosMoveLeft"));
        ArcadeMachine?.SetAnimParameter("right", Input.Down("TetrosMoveRight"));
        ArcadeMachine?.SetAnimParameter("down", Input.Down("TetrosSoftDrop"));

    }
}
