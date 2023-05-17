using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Home;

public class ArcadeCabinetTetrisPanel : WorldPanel
{
    public ArcadeCabinetTetrisPanel()
    {
        StyleSheet.Load("/Entities/Arcade Cabinets/Tetris/ArcadeCabinetTetrisPanel.scss");
        Add.Label("TETRIS", "title");
    }
}