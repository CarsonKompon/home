using Sandbox;
using Sandbox.UI;

namespace ArcadeZone;

public partial class SliderWithLabel : Panel
{
    public Slider Slider;
    public Label Label;

    public SliderWithLabel()
    {
        Slider = AddChild<Slider>();
        Label = AddChild<Label>();
    }
}