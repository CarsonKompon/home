using Sandbox;

namespace Home;

public class RightHandVR : HandEntityVR
{
    
    protected override string ModelName => "models/hands_discgolf/hand_toon_right.vmdl";
    public override Input.VrHand InputHand => Input.VR.RightHand;

    public override void Spawn()
    {
        base.Spawn();
    }

}