using System.Diagnostics;
using System.ComponentModel;
using System;
using Sandbox;
using Sandbox.UI;
using Sandbox.Utility;
using Sandbox.UI.Construct;
using System.Collections.Generic;

namespace Home;

public class RotatingModelScenePanel : ScenePanel
{
    public int DistanceToObject = 20;
    public int HeightOffset = 10;
    public string Model = "models/citizen/citizen.vmdl";

    private SceneModel WorldModel;

    private SceneSpotLight LightWarm;
	private SceneSpotLight LightBlue;
    private float RotationAngle = 45f;
    private bool Hovering = false;

    public RotatingModelScenePanel()
    {
        // Cleanup
        World?.Delete();

        // Create
        World = new SceneWorld();
        WorldModel = new SceneModel(World, "models/citizen/citizen.vmdl", Transform.Zero);

        LightWarm = new SceneSpotLight(World);
        LightWarm.Radius = 280;
        LightWarm.Position = Vector3.Up * 100.0f + Vector3.Forward * 100.0f + Vector3.Right * -200;
		LightWarm.LightColor = new Color( 1.0f, 0.95f, 0.8f ) * 20.0f;
		LightWarm.Rotation = Rotation.LookAt( -LightWarm.Position );
		LightWarm.ConeInner = 50;
		LightWarm.ConeOuter = 70;

        LightBlue = new SceneSpotLight(World);
		LightBlue.Radius = 250;
		LightBlue.Position = Vector3.Up * 100.0f + Vector3.Forward * 100.0f + Vector3.Right * 100;
		LightBlue.LightColor = new Color( 1f, 1f, 1f ) * 5.0f;
		LightBlue.Rotation = Rotation.LookAt( -LightBlue.Position );
		LightBlue.ConeInner = 70;
		LightBlue.ConeOuter = 70;

        new SceneCubemap(World, Texture.Load("textures/cubemaps/default.vtex" ), BBox.FromPositionAndSize( Vector3.Zero, 1000 ) );

        Angles angles = new( 5, RotationAngle, 0 );
		Vector3 pos = Vector3.Up * HeightOffset + angles.Forward * -DistanceToObject;

		Camera.Position = pos;
		Camera.Rotation = Rotation.From( angles );
		//Camera.AmbientLightColor = Color.Gray * 0.1f;
    }

    public override void Tick()
    {
        base.Tick();

        if(Hovering)
        {
            RotationAngle += 4f * Time.Delta;
        }
    }

    protected override void OnMouseOver( MousePanelEvent e )
    {
        base.OnMouseOver(e);

        Hovering = true;

    }

    protected override void OnMouseOut( MousePanelEvent e )
    {
        base.OnMouseOut(e);

        Hovering = false;
        RotationAngle = 45f;
    }
}