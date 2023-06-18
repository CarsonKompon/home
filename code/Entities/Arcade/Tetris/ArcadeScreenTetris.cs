using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Home;

public partial class ArcadeScreenTetris : WorldPanel
{
    public Label ScoreLabel;
    public Label HighScoreLabel;
    public Label LevelLabel;

    public Panel Board {get; set;}
    public Panel[] Blocks {get; set;} = new Panel[200];
    public Panel[] CurrentBlocks {get; set;} = new Panel[4];
    public Panel[][] NextBlocks {get; set;} = new Panel[5][];
    public Panel[] HoldBlocks {get; set;} = new Panel[4];
    public Panel[] GhostBlocks {get; set;} = new Panel[4];
    RealTimeSince TimeSinceLastUpdate = 0f;
    
    public ArcadeMachineTetris Machine;

    public ArcadeScreenTetris()
    {
        StyleSheet.Load("/Entities/Arcade/Tetris/ArcadeScreenTetris.scss");

        AddClass("game-tetris");
        
        var scorePanel = Add.Panel("score-panel");
        scorePanel.Add.Label("Score:", "header");
        ScoreLabel = scorePanel.Add.Label("0", "score");

        var levelPanel = Add.Panel("level-panel");
        levelPanel.Add.Label("Level:", "header");
        LevelLabel = levelPanel.Add.Label("0", "level");

        var highScorePanel = Add.Panel("high-score-panel");
        highScorePanel.Add.Label("High Score:", "header");
        HighScoreLabel = highScorePanel.Add.Label("0", "score");

        // LEFT PANEL
        var leftPanel = Add.Panel("left-panel");
        leftPanel.Add.Label("Holding:", "header");
        var holding = leftPanel.Add.Panel("holding");
        var holdingBlockPanel = holding.Add.Panel("block-panel");
        for(int i=0; i<4; i++)
        {
            HoldBlocks[i] = holdingBlockPanel.Add.Panel("block held current");
        }

        var controlsPanel = leftPanel.AddChild<ArcadeScreenTetrisControls>();

        // GAME BOARD
        Board = Add.Panel("board");

        for(int i=0; i<200; i++)
        {
            Blocks[i] = Board.Add.Panel("block");
        }

        for(int i=0; i<4; i++)
        {
            CurrentBlocks[i] = Board.Add.Panel("block current");
        }

        for(int i=0; i<4; i++)
        {
            GhostBlocks[i] = Board.Add.Panel("block ghost");
        }

        for(int i=0; i<5; i++)
        {
            NextBlocks[i] = new Panel[4];
        }

        // RIGHT PANEL
        var rightPanel = Add.Panel("right-panel");
        rightPanel.Add.Label("Next:", "header");
        var next = rightPanel.Add.Panel("next");
        var nextBlockPanel = next.Add.Panel("block-panel");
        for(int i=0; i<4; i++)
        {
            NextBlocks[0][i] = nextBlockPanel.Add.Panel("block");
        }

        for(int i=1; i<5; i++)
        {
            var nextRow = rightPanel.Add.Panel("next small");
            var nextSmallBlockPanel = nextRow.Add.Panel("block-panel small");
            for(int j=0; j<4; j++)
            {
                NextBlocks[i][j] = nextSmallBlockPanel.Add.Panel("block current");
            }
        }

        HideAll();

        float width = 600f;
        float height = 500f;
        PanelBounds = new Rect(-width/2, -height/2, width, height);
    }

    public ArcadeScreenTetris(ArcadeMachineTetris machine) : this()
    {
        Machine = machine;
    }

    public void UpdateBoard(int[] board)
    {
        for(int i=0; i<200; i++)
        {
            int val = board[i];
            Blocks[i].SetClass("t-1 t-2 t-3 t-4 t-5 t-6 t-7", false);
            Blocks[i].SetClass("active t-" + val.ToString(), val != 0);
        }
        ScoreLabel.Text = Machine.Score.ToString();
        LevelLabel.Text = Machine.Level.ToString();
    }

    public void UpdatePlayer(ArcadeMachineTetris.BlockType blockType, Vector2 pos, int rotation)
    {
        if(blockType == ArcadeMachineTetris.BlockType.Empty)
        {
            for(int i=0; i<4; i++)
            {
                CurrentBlocks[i].Style.Left = -200;
                CurrentBlocks[i].Style.Top = -200;
            }
            return;
        }
        SetPositionFromPiece(CurrentBlocks, blockType, pos, rotation);

        // Calculate ghost position
        int ghostY = (int)pos.y;
        while(!Machine.CheckPieceCollision(blockType, rotation, new Vector2(pos.x, ghostY)))
        {
            ghostY++;
        }
        ghostY--;
        SetPositionFromPiece(GhostBlocks, blockType, new Vector2(pos.x, ghostY), rotation);

        TimeSinceLastUpdate = 0f;
    }

    public void UpdateHeldPiece(ArcadeMachineTetris.BlockType blockType)
    {
        if(blockType == ArcadeMachineTetris.BlockType.Empty)
        {
            for(int i=0; i<4; i++)
            {
                HoldBlocks[i].AddClass("hide");
            }
            return;
        }

        for(int i=0; i<4; i++)
        {
            HoldBlocks[i].RemoveClass("hide");
            HoldBlocks[i].SetClass("t-1 t-2 t-3 t-4 t-5 t-6 t-7", false);
            HoldBlocks[i].SetClass("current t-" + ((int)blockType).ToString(), blockType != ArcadeMachineTetris.BlockType.Empty);
        }
        SetPositionFromPiece(HoldBlocks, blockType, new Vector2(0, 0), 0);
    }

    public void UpdateNextPieces(ArcadeMachineTetris.BlockType[] blockTypes)
    {
        for(int i=0; i<blockTypes.Count(); i++)
        {
            if(i >= NextBlocks.Count()) break;
            for(int j=0; j<NextBlocks[i].Count(); j++)
            {
                NextBlocks[i][j].SetClass("t-1 t-2 t-3 t-4 t-5 t-6 t-7", false);
                if(i < blockTypes.Count()) NextBlocks[i][j].SetClass("current t-" + ((int)blockTypes[i]).ToString(), blockTypes[i] != ArcadeMachineTetris.BlockType.Empty);
            }
            SetPositionFromPiece(NextBlocks[i], blockTypes[i], new Vector2(0, 0), 0);
        }
    }

    public void UpdateHighScore(long score)
    {
        HighScoreLabel.Text = score.ToString();
    }

    public void ShowAll()
    {
        var panelList = new List<Panel>();
        panelList.AddRange(CurrentBlocks);
        panelList.AddRange(GhostBlocks);
        for(int i=0; i<5; i++)
        {
            panelList.AddRange(NextBlocks[i]);
        }
        panelList.AddRange(HoldBlocks);
        foreach(var block in panelList)
        {
            block.RemoveClass("hide");
        }
    }

    public void HideAll()
    {
        var panelList = new List<Panel>();
        panelList.AddRange(CurrentBlocks);
        panelList.AddRange(GhostBlocks);
        for(int i=0; i<5; i++)
        {
            panelList.AddRange(NextBlocks[i]);
        }
        panelList.AddRange(HoldBlocks);
        foreach(var block in panelList)
        {
            block.AddClass("hide");
        }
    }

    public void SetPositionFromPiece(Panel[] panel, ArcadeMachineTetris.BlockType blockType, Vector2 pos, int rotation)
    {
        TetrisShape shape = TetrisShapes.GetShape(blockType, rotation);
        int index = 0;
        for(int i=0; i<16; i++)
        {
            int x = i % 4;
            int y = i / 4;
            int x2 = x - 1;
            int y2 = y - 1;
            int x3 = (int)pos.x + x2;
            int y3 = (int)pos.y + y2;
            if(shape.Blocks.Contains(i))
            {
                panel[index].Style.Left = x3 * 10f;
                panel[index].Style.Top = y3 * 10f;
                index++;
            }
        }
    }

	protected override int BuildHash()
	{
        return HashCode.Combine(base.BuildHash(), TimeSinceLastUpdate < 0.25f);
	}

}