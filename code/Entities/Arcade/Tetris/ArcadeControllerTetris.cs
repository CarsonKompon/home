using Sandbox;

namespace Home;

public class ArcadeControllerTetris : ArcadeControllerBase
{
    
    public override void BuildInput()
    {
        base.BuildInput();

        if(ArcadeMachine is not ArcadeMachineTetris tetris) return;

        if(Input.Pressed("Jump"))
        {
            tetris.HardDrop();
        }
        else if(Input.Pressed("Run"))
        {
            tetris.Hold();
        }
        else
        {
            if(Input.Pressed("Left"))
            {
                Log.Info("left");
                tetris.Move(-1);
            }
            if(Input.Pressed("Right"))
            {
                Log.Info("right");
                tetris.Move(1);
            }
            if(Input.Pressed("Forward"))
            {
                Log.Info("rotate");
                tetris.Rotate();
            }
            var backward = Input.Down("Backward");
            tetris.SetFastDrop(backward);
        }
    }



}