using Sandbox;

namespace Home;

public class LeftHandVR : HandEntityVR
{
    
    protected override string ModelName => "models/hands_discgolf/hand_toon_left.vmdl";
    public override Input.VrHand InputHand => Input.VR.LeftHand;

    public override void Spawn()
    {
        base.Spawn();
    }

}