namespace Home;

[Library]
public class WalkControllerVR : HomeWalkController
{
    public override void FrameSimulate()
	{
		base.FrameSimulate();

		EyeRotation = Input.VR.Head.Rotation;
	}

    public override void Simulate()
    {
        var pl = Pawn as HomePlayer;

        EyeLocalPosition = Vector3.Up * (EyeHeight * Pawn.Scale);
        UpdateBBox();

        EyeLocalPosition += TraceOffset;
        EyeRotation = Input.VR.Head.Rotation;

        RestoreGroundPos();

        if (Unstuck.TestAndFix())
            return;

        CheckLadder();
        Swimming = Pawn.GetWaterLevel() > 0.5f;

        //
        // Start Gravity
        //
        if (!Swimming && !IsTouchingLadder)
        {
            Velocity -= new Vector3(0, 0, Gravity * 0.5f) * Time.Delta;
            Velocity += new Vector3(0, 0, BaseVelocity.z) * Time.Delta;

            BaseVelocity = BaseVelocity.WithZ(0);
        }

        if(Input.VR.RightHand.JoystickPress.IsPressed)
        {
            CheckJumpButton();
        }

        // Fricion is handled before we add in any base velocity. That way, if we are on a conveyor,
        //  we don't slow when standing still, relative to the conveyor.
        bool bStartOnGround = GroundEntity != null;
        //bool bDropSound = false;
        if (bStartOnGround)
        {
            //if ( Velocity.z < FallSoundZ ) bDropSound = true;

            Velocity = Velocity.WithZ(0);
            //player->m_Local.m_flFallVelocity = 0.0f;

            if (GroundEntity != null)
            {
                ApplyFriction(GroundFriction * SurfaceFriction);
            }
        }

        //
        // Work out wish velocity.. just take input, rotate it to view, clamp to -1, 1
        //
        WishVelocity = Vector3.Zero;
        WishVelocity += Input.VR.LeftHand.Joystick.Value.y * Input.VR.Head.Rotation.Forward;
		WishVelocity += Input.VR.LeftHand.Joystick.Value.x * Input.VR.Head.Rotation.Right;

        // WishVelocity = new Vector3( pl.InputDirection.x.Clamp( -1f, 1f ), pl.InputDirection.y.Clamp( -1f, 1f ), 0);
        var inSpeed = WishVelocity.Length.Clamp(0, 1);
        // WishVelocity *= pl.ViewAngles.WithPitch(0).ToRotation();

        if (!Swimming && !IsTouchingLadder)
        {
            WishVelocity = WishVelocity.WithZ(0);
        }

        WishVelocity = WishVelocity.Normal * inSpeed;
        WishVelocity *= GetWishSpeed();

        Duck.PreTick();

        bool bStayOnGround = false;
        if (Swimming)
        {
            ApplyFriction(1);
            WaterMove();
        }
        else if (IsTouchingLadder)
        {
            SetTag("climbing");
            LadderMove();
        }
        else if (GroundEntity != null)
        {
            bStayOnGround = true;
            WalkMove();
        }
        else
        {
            AirMove();
        }

        CategorizePosition(bStayOnGround);

        // FinishGravity
        if (!Swimming && !IsTouchingLadder)
        {
            Velocity -= new Vector3(0, 0, Gravity * 0.5f) * Time.Delta;
        }


        if (GroundEntity != null)
        {
            Velocity = Velocity.WithZ(0);
        }

        SaveGroundPos();
    }


}
