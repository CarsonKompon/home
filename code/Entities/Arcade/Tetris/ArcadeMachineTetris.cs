using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox;
using Sandbox.UI;
using Editor;

namespace Home;

/// <summary>
/// This is a teleporter that allows you to teleport around the lobby
/// </summary>
[Library("home_arcade_tetros"), HammerEntity]
[Title("Tetros Arcade Cabinet"), Category("Arcade"), Icon("gamepad")]
public partial class ArcadeMachineTetris : ArcadeMachineBase
{
    public override string ControllerType => "ArcadeControllerTetris";
    public ArcadeScreenTetris Screen { get; set; }

    public enum BlockType { Empty, I, O, T, S, Z, J, L };
    const int BOARD_WIDTH = 10;
    const int QUEUE_LENGTH = 5;
    [Net, Predicted]  List<BlockType> Queue {get; set;} = new List<BlockType>();
    [Net] public BlockType HeldPiece {get; set;} = BlockType.Empty;

    [Net] private List<BlockType> GrabBag {get; set;} = new List<BlockType>();

    [Net] public List<BlockType> Board {get; set;}
    [Net] public BlockType CurrentPiece {get; set;} = new BlockType();
    [Net] public long Score {get; set;} = 0;
    [Net, Predicted] public long HighScore {get; set;} = 0;
    [Net, Predicted] public int CurrentPieceX {get; set;}
    [Net, Predicted] public int CurrentPieceY {get; set;}
    [Net, Predicted] public int CurrentPieceRotation {get; set;} = 0;
    [Net, Predicted] public bool FastDrop {get; set;} = false;
    private RealTimeSince LastUpdate = 0f;
    public float UpdateInterval = 0.2f;

    public ArcadeMachineTetris()
    {
        for(int i=0; i<200; i++)
        {
            Board.Add(BlockType.Empty);
        }
    }

    public override void ClientSpawn()
    {
        base.ClientSpawn();
        
        Screen = new ArcadeScreenTetris(this);
    }

    [GameEvent.Client.Frame]
    public void OnFrame()
    {
        if (Screen == null) return;
        var screenPos = GetAttachment("ScreenPos").Value;
        Screen.Transform = screenPos;
    }

    public override void StartGame()
    {
        base.StartGame();
        for(int i=0; i<QUEUE_LENGTH; i++)
        {
            Queue.Add(GetRandomBlock());
        }

        UpdateBoard();
        UpdateNextPieces();
    }

    public override void EndGame()
    {
        CurrentUser.GiveMoney(Score);
        SaveHighScore(To.Single(CurrentUser));

        CurrentPiece = BlockType.Empty;
        Board = new List<BlockType>();
        for(int i=0; i<200; i++)
        {
            Board.Add(BlockType.Empty);
        }
        Queue = new List<BlockType>();
        HeldPiece = BlockType.Empty;
        Score = 0;
        UpdateBoard();

        base.EndGame();
    }

    [GameEvent.Tick.Server]
    public void Tick()
    {
        var interval = UpdateInterval;
        if(FastDrop) interval /= 4f;
        if(InUse && LastUpdate > interval)
        {
            if(CurrentPiece == BlockType.Empty)
            {
                CurrentPiece = GetRandomBlock();
                CurrentPieceX = 5;
                CurrentPieceY = -2;
                CurrentPieceRotation = 0;
                UpdatePlayer();
            }
            else
            {
                CurrentPieceY += 1;
                if(CheckPieceCollision(CurrentPiece, CurrentPieceRotation, new Vector2(CurrentPieceX, CurrentPieceY)))
                {
                    CurrentPieceY -= 1;
                    PlacePiece();
                }
                else if(FastDrop)
                {
                    Sound.FromEntity("tetros_move", this).SetPitch(1.5f);
                }
                UpdatePlayer();
            }
            LastUpdate = 0f;
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
        UpdateNextPieces();
        return block;
    }

    [ClientRpc]
    public void UpdateBoard()
    {
        if (Screen == null) return;
        int[] board = new int[Board.Count];
        for(int i=0; i<Board.Count; i++)
        {
            board[i] = (int)Board[i];
        }
        Screen.UpdateBoard(board);
    }

    [ClientRpc]
    public void UpdatePlayer()
    {
        if (Screen == null) return;
        Screen.UpdatePlayer(CurrentPiece, new Vector2(CurrentPieceX, CurrentPieceY), CurrentPieceRotation);
    }

    [ClientRpc]
    public void UpdateHeldPiece()
    {
        if (Screen == null) return;
        Screen.UpdateHeldPiece(HeldPiece);
    }

    [ClientRpc]
    public void UpdateNextPieces()
    {
        if (Screen == null) return;
        Screen.UpdateNextPieces(Queue.ToArray());
    }

    [ClientRpc]
    public void UpdateHighScore()
    {
        if (Screen == null) return;
        Screen.UpdateHighScore(HighScore);
    }

    [ClientRpc]
    public void LoadHighScore()
    {
        HighScore = Cookie.Get<long>("home.arcade.tetros.hiscore", 0);
        UpdateHighScore();
    }

    [ClientRpc]
    public void SaveHighScore()
    {
        if(Score > Cookie.Get<long>("home.arcade.tetros.hiscore", 0))
        {
            Cookie.Set("home.arcade.tetros.hiscore", Score);
        }
    }

    public bool CheckPieceCollision(BlockType block, int rot, Vector2 pos)
    {
        var piece = TetrisShapes.GetShape(block, rot);
        var grid = piece.GetGrid();
        for(int i=0; i<16; i++)
        {
            if(grid[i] == 1)
            {
                int x = (int)pos.x + (i % 4) - 1;
                int y = (int)pos.y + (i / 4) - 1;
                if(x < 0 || x >= BOARD_WIDTH || y >= 20)
                {
                    return true;
                }

                int ipos = x + (y * BOARD_WIDTH);
                if(ipos >= 0 && ipos < Board.Count && Board[ipos] != BlockType.Empty)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void PlacePiece()
    {
        var piece = TetrisShapes.GetShape(CurrentPiece, CurrentPieceRotation);
        var grid = piece.GetGrid();
        for(int i=0; i<16; i++)
        {
            if(grid[i] == 1)
            {
                var x = CurrentPieceX + (i % 4) - 1;
                var y = CurrentPieceY + (i / 4) - 1;
                int pos = x + (y * BOARD_WIDTH);
                if(pos < 0)
                {
                    EndGame();
                    RemoveUser();
                    return;
                }
                if(pos < Board.Count)
                {
                    Board[pos] = CurrentPiece;
                }
            }
        }
        Sound.FromEntity("tetros_place", this);
        CurrentPiece = BlockType.Empty;
        CheckLine();
    }

    private void CheckLine()
    {
        for(int y=0; y<20; y++)
        {
            bool line = true;
            for(int x=0; x<BOARD_WIDTH; x++)
            {
                int pos = x + (y * BOARD_WIDTH);
                if(Board[pos] == BlockType.Empty)
                {
                    line = false;
                    break;
                }
            }
            if(line)
            {
                for(int x=0; x<BOARD_WIDTH; x++)
                {
                    int pos = x + (y * BOARD_WIDTH);
                    Board[pos] = BlockType.Empty;
                }
                for(int i=y; i>0; i--)
                {
                    for(int x=0; x<BOARD_WIDTH; x++)
                    {
                        int pos = x + (i * BOARD_WIDTH);
                        int prevPos = x + ((i-1) * BOARD_WIDTH);
                        Board[pos] = Board[prevPos];
                    }
                }
                Score += 100;
            }
        }
        UpdateBoard();
    }


    public void Move(int dir)
    {
        CurrentPieceX += MathF.Sign(dir);
        if(CheckPieceCollision(CurrentPiece, CurrentPieceRotation, new Vector2(CurrentPieceX, CurrentPieceY)))
        {
            CurrentPieceX -= MathF.Sign(dir);
        }
        Sound.FromEntity("tetros_move", this);
        UpdatePlayer();
    }

    public void Rotate()
    {
        CurrentPieceRotation = (CurrentPieceRotation + 1) % 4;
        if(CheckPieceCollision(CurrentPiece, CurrentPieceRotation, new Vector2(CurrentPieceX, CurrentPieceY)))
        {
            CurrentPieceRotation = (CurrentPieceRotation - 1) % 4;
        }
        LastUpdate /= 2;
        Sound.FromEntity("tetros_rotate", this);
        UpdatePlayer();
    }

    public void SetFastDrop(bool fastDrop)
    {
        FastDrop = fastDrop;
    }

    public void HardDrop()
    {
        while(!CheckPieceCollision(CurrentPiece, CurrentPieceRotation, new Vector2(CurrentPieceX, CurrentPieceY)))
        {
            CurrentPieceY += 1;
        }
        CurrentPieceY -= 1;
        PlacePiece();
    }

    public void Hold()
    {
        if(HeldPiece == BlockType.Empty)
        {
            HeldPiece = CurrentPiece;
            CurrentPiece = BlockType.Empty;
        }
        else
        {
            var temp = HeldPiece;
            HeldPiece = CurrentPiece;
            CurrentPiece = temp;
        }
        CurrentPieceX = 5;
        CurrentPieceY = -2;
        CurrentPieceRotation = 0;
        UpdateHeldPiece();
    }

}