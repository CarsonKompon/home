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
                tetris.Move(-1);
            }
            if(Input.Pressed("Right"))
            {
                tetris.Move(1);
            }
            if(Input.Pressed("Forward"))
            {
                tetris.Rotate();
            }
            var backward = Input.Down("Backward");
            tetris.SetFastDrop(backward);
        }
    }



}