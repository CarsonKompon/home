namespace Home;

[StyleSheet("/Entities/Arcade/Rhythm4K/Rhythm4K/UI/Game/GamePage.cs.scss")]
public class RhythmGamePage : Rhythm4K.GamePage
{
    public override string LeftInput => "RhythmLeft";
    public override string DownInput => "RhythmDown";
    public override string UpInput => "RhythmUp";
    public override string RightInput => "RhythmRight";
    public override bool ColouredArrows => false;
}
