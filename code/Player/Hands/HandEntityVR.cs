using Sandbox;

namespace Home;

public partial class HandEntityVR : AnimatedEntity
{
    protected virtual string ModelName => "";

    [Net] public HandEntityVR Other { get; set; }

    public bool GripPressed => InputHand.Grip > 0.5f;
    public bool TriggerPressed => InputHand.Trigger > 0.5f;

    public virtual Input.VrHand InputHand => Input.VR.RightHand;

    public override void Spawn()
    {
        SetModel(ModelName);

        Transmit = TransmitType.Always;
    }

    public override void FrameSimulate(IClient cl)
    {
        base.FrameSimulate(cl);

        Transform = InputHand.Transform;
    }

    public override void Simulate(IClient cl)
    {
        base.Simulate(cl);

        Transform = InputHand.Transform;
        Animate();
    }

    public void Animate()
    {
        if(InputHand.ButtonB.IsPressed)
        {
            SetAnimParameter( "Index", 1.0f );
            SetAnimParameter( "Middle", 0.0f );
            SetAnimParameter( "Ring", 1.0f );
            SetAnimParameter( "Thumb", 1.0f );
        }
        else
        {
            SetAnimParameter( "Index", InputHand.GetFingerCurl( 1 ) );
            SetAnimParameter( "Middle", InputHand.GetFingerCurl( 2 ) );
            SetAnimParameter( "Ring", InputHand.GetFingerCurl( 3 ) );
            SetAnimParameter( "Thumb", InputHand.GetFingerCurl( 0 ) );
        }
    }
}