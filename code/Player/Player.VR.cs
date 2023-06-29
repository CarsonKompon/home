using System;
using Sandbox;

namespace Home;

public partial class HomePlayer
{

    [Net, Local] public LeftHandVR LeftHand {get; set;}
    [Net, Local] public RightHandVR RightHand {get; set;}

    private void CreateHands()
    {
        DeleteHands();

        LeftHand = new LeftHandVR() { Owner = this };
        RightHand = new RightHandVR() { Owner = this };

        LeftHand.Other = RightHand;
        RightHand.Other = LeftHand;
    }

    private void DeleteHands()
    {
        LeftHand?.Delete();
        RightHand?.Delete();
    }

    public void SetVrAnimProperties()
    {
        if(LifeState != LifeState.Alive) return;
        if(!Input.VR.IsActive) return;

        SetAnimParameter("b_vr", true);
        var leftHandLocal = Transform.ToLocal( LeftHand.GetBoneTransform( 0 ) );
		var rightHandLocal = Transform.ToLocal( RightHand.GetBoneTransform( 0 ) );

		var handOffset = Vector3.Zero;
		SetAnimParameter( "left_hand_ik.position", leftHandLocal.Position + (handOffset * leftHandLocal.Rotation) );
		SetAnimParameter( "right_hand_ik.position", rightHandLocal.Position + (handOffset * rightHandLocal.Rotation) );

		SetAnimParameter( "left_hand_ik.rotation", leftHandLocal.Rotation * Rotation.From( 0, 0, 180 ) );
		SetAnimParameter( "right_hand_ik.rotation", rightHandLocal.Rotation );

		float height = Input.VR.Head.Position.z - Position.z;
		SetAnimParameter( "duck", 1.0f - ((height - 32f) / 32f) ); // This will probably need tweaking depending on height
    }

    private TimeSince timeSinceLastRotation;
    private void CheckRotate()
    {
        if(!Game.IsServer) return;

        const float deadzone = 0.2f;
		const float angle = 45f;
		const float delay = 0.25f;

		float rotate = Input.VR.RightHand.Joystick.Value.x;

		if ( timeSinceLastRotation > delay )
		{
			if ( rotate > deadzone )
			{
				Transform = RotateAround(
					Transform,
					Input.VR.Head.Position.WithZ( Position.z ),
					Rotation.FromAxis( Vector3.Up, -angle )
				);

				timeSinceLastRotation = 0;
			}
			else if ( rotate < -deadzone )
			{
				Transform = RotateAround(
					Transform,
					Input.VR.Head.Position.WithZ( Position.z ),
					Rotation.FromAxis( Vector3.Up, angle )
				);

				timeSinceLastRotation = 0;
			}
		}

		if ( rotate > -deadzone && rotate < deadzone )
		{
			timeSinceLastRotation = 10;
		}
    }

	private Transform RotateAround( Transform transform, Vector3 pivotPoint, Rotation rotation )
	{
		var resultTransform = transform;

		resultTransform.Position = rotation * (transform.Position - pivotPoint) + pivotPoint;
		resultTransform.Rotation = rotation * transform.Rotation;

		return resultTransform;
	}

}