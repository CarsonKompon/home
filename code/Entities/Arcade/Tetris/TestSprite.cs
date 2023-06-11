using Sandbox;

namespace Home;

public class TestSprite : Sprite
{
    public TestSprite()
    {
        SpriteTexture  = "sprites/arcade/tetris/test.png";
        Scale = 10f;

        Filter = SpriteFilter.Pixelated;
    }

    [GameEvent.Tick.Server]
    public void Tick()
    {
        Rotation += 1f;
    }
}