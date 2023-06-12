using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Home;

public class ArcadeScreenTetris : WorldPanel
{
    public enum BlockType { Empty, I, O, T, S, Z, J, L };
    const int BOARD_WIDTH = 10;
    const int QUEUE_LENGTH = 5;

    public Label ScoreLabel;

    public Panel Board {get; set;}
    public Panel[] Blocks {get; set;} = new Panel[200];
    public List<BlockType> Queue {get; set;} = new List<BlockType>();
    public BlockType Holding {get; set;} = BlockType.Empty;

    private List<BlockType> GrabBag {get; set;} = new List<BlockType>();
    

    public ArcadeScreenTetris()
    {
        StyleSheet.Load("/Entities/Arcade/Tetris/ArcadeScreenTetris.scss");

        AddClass("game-tetris");
        
        var scorePanel = Add.Panel("score-panel");
        scorePanel.Add.Label("Score:", "header");
        ScoreLabel = scorePanel.Add.Label("0", "score");

        Board = Add.Panel("board");

        for(int i=0; i<200; i++)
        {
            Blocks[i] = Board.Add.Panel("block");
        }

        float width = 600f;
        float height = 500f;
        PanelBounds = new Rect(-width/2, -height/2, width, height);


    }

    public void StartGame()
    {
        for(int i=0; i<QUEUE_LENGTH; i++)
        {
            Queue.Add(GetRandomBlock());
        }
    }

    public BlockType GetRandomBlock()
    {
        if(GrabBag.Count == 0)
        {
            GrabBag = new List<BlockType> { BlockType.I, BlockType.O, BlockType.T, BlockType.S, BlockType.Z, BlockType.J, BlockType.L };
            // Shuffle the grab bag
            GrabBag = GrabBag.OrderBy(x => Guid.NewGuid()).ToList();
        }

        var block = GrabBag[0];
        GrabBag.RemoveAt(0);
        return block;
    }

}