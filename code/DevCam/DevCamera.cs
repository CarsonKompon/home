﻿using Sandbox;
using Sandbox.UI;
using Home.DevCam;

namespace Home;

public class DevCamera : Sandbox.EntityComponent
{
	Angles LookAngles;
	Vector3 MoveInput;

	Vector3 Position;
	Rotation Rotation;

	Vector3 TargetPos;
	Rotation TargetRot;

	bool PivotEnabled;
	Vector3 PivotPos;
	float PivotDist;

	float MoveSpeed;
	float BaseMoveSpeed = 300.0f;
	float FovOverride = 0;

	float LerpMode = 0;

	private static RootPanel devRoot = null;
	private static DevCamOverlay overlay = null;

	/// <summary>
	/// On the camera becoming activated, snap to the current view position
	/// </summary>
	protected override void OnActivate()
	{
		if ( !Game.IsClient )
			return;

		if ( Entity != Game.LocalClient )
			return;

		TargetPos = Camera.Position;
		TargetRot = Camera.Rotation;

		Position = TargetPos;
		Rotation = TargetRot;
		LookAngles = Rotation.Angles();
		FovOverride = 80;

		//
		// Set the devcamera class on the HUD. It's up to the HUD what it does with it.
		//
		Game.RootPanel?.SetClass( "devcamera", true );

		// Only create if we're using the dev cam
		if ( devRoot == null )
		{
			devRoot = new();
			overlay = devRoot.AddChild<DevCamOverlay>();
		}

		overlay?.Activated();
	}

	protected override void OnDeactivate()
	{
		Game.RootPanel?.SetClass( "devcamera", false );

		overlay?.Deactivated();
	}

	[GameEvent.Client.PostCamera]
	internal void Update()
	{
		if ( Entity != Game.LocalClient )
			return;

		if ( PivotEnabled )
		{
			PivotMove();
		}
		else
		{
			FreeMove();
		}

		Camera.Position = Position;
		Camera.Rotation = Rotation;
		Camera.FieldOfView = Screen.CreateVerticalFieldOfView( FovOverride );
		Camera.FirstPersonViewer = null;
	}

	[GameEvent.Client.BuildInput]
	internal void BuildInput()
	{
		if ( Entity != Game.LocalClient )
			return;

		MoveInput = Input.AnalogMove;
		MoveSpeed = 1;

		if ( Input.Down( "run" ) ) MoveSpeed = 5;
		if ( Input.Down( "crouch" ) ) MoveSpeed = 0.2f;

		if ( Input.Down( "DevCamSpeedUp" ) ) LerpMode -= 0.001f;
		if ( Input.Down( "DevCamSpeedDown" ) ) LerpMode += 0.001f;

		LerpMode = LerpMode.Clamp( 0.0f, 1.0f );

		if ( Input.Pressed( "walk" ) )
		{
			var tr = Trace.Ray( Position, Position + Rotation.Forward * 4096 ).Run();

			if ( tr.Hit )
			{
				PivotPos = tr.EndPosition;
				PivotDist = Vector3.DistanceBetween( tr.EndPosition, Position );
				PivotEnabled = true;
			}
		}

		if ( Input.Down( "rightclick" ) )
		{
			FovOverride += Input.AnalogLook.pitch * (FovOverride / 30.0f);
			FovOverride = FovOverride.Clamp( 5, 150 );
			Input.AnalogLook = default;
		}

		if ( Input.Pressed( "score" ) )
		{
			overlay?.Show();
		}

		if ( Input.Released( "score" ) )
		{
			overlay?.Hide();
		}

		LookAngles += Input.AnalogLook * (FovOverride / 80.0f);
		LookAngles.roll = 0;

		PivotEnabled = PivotEnabled && Input.Down( "walk" );

		if ( PivotEnabled )
		{
			MoveInput.x += Input.MouseWheel * 10.0f;
		}
		else
		{
			BaseMoveSpeed += Input.MouseWheel * 10.0f;
			BaseMoveSpeed = BaseMoveSpeed.Clamp( 10, 1000 );
		}

		Input.ClearActions();
		Input.AnalogMove = default;
		Input.StopProcessing = true;
	}

	void FreeMove()
	{
		var mv = MoveInput.Normal * BaseMoveSpeed * RealTime.Delta * Rotation * MoveSpeed;

		TargetRot = Rotation.From( LookAngles );
		TargetPos += mv;

		Position = Vector3.Lerp( Position, TargetPos, 10 * RealTime.Delta * (1 - LerpMode) );
		Rotation = Rotation.Slerp( Rotation, TargetRot, 10 * RealTime.Delta * (1 - LerpMode) );
	}

	void PivotMove()
	{
		PivotDist -= MoveInput.x * RealTime.Delta * 100 * (PivotDist / 50);
		PivotDist = PivotDist.Clamp( 1, 1000 );

		TargetRot = Rotation.From( LookAngles );
		Rotation = Rotation.Slerp( Rotation, TargetRot, 10 * RealTime.Delta * (1 - LerpMode) );

		TargetPos = PivotPos + Rotation.Forward * -PivotDist;
		Position = TargetPos;
	}

	[Event( "devcam.reset" )]
	internal void ResetToDefaults()
	{
		FovOverride = 80;
	}

	[Event( "devcam.setlerp")]
	internal void SetLerpMode( float lerp )
	{
		LerpMode = lerp;
	}
}
