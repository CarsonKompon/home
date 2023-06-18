using System;
using Sandbox;
using Sandbox.UI;

namespace Home;

public partial class HomeMainMenuBackground : Panel
{
    public int CurrentImage = 0;
    public int ImageCount = 4;

    public RealTimeSince TimeUntilNext = 5;

    public HomeMainMenuBackground()
    {
        AddClass( "background" );

        Random rand = new Random();
        CurrentImage = rand.Int(0, ImageCount);
    }

    public override void Tick()
    {
        base.Tick();

        if(TimeUntilNext >= 5f)
        {
            foreach(var child in Children)
            {
                child.Delete();
            }
            Add.Panel("background-image image-" + CurrentImage.ToString());
            CurrentImage = (CurrentImage + 1) % ImageCount;
            TimeUntilNext = 0;
        }
    }

    protected override int BuildHash()
    {
        return HashCode.Combine(CurrentImage);
    }
}