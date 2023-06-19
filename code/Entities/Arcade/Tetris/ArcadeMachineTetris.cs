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
    [Net] public BlockType HeldPiece {get; set;} = BlockType.Empty;
    [Net] public int Level {get; set;} = 1;
    [Net] public int LinesNeeded {get; set;} = 10;
    [Net] private List<BlockType> GrabBag {get; set;} = new List<BlockType>();
    [Net] private List<BlockType> Queue {get; set;}
    
    [Net] public List<BlockType> Board {get; set;}
    [Net] public BlockType CurrentPiece {get; set;} = new BlockType();
    [Net] public long Score {get; set;} = 0;
    [Net, Predicted] public long HighScore {get; set;} = 0;
    [Net, Predicted] public int CurrentPieceX {get; set;}
    [Net, Predicted] public int CurrentPieceY {get; set;}
    [Net, Predicted] public int CurrentPieceRotation {get; set;} = 0;
    [Net, Predicted] public bool FastDrop {get; set;} = false;
    [Net, Predicted] public bool JustHeld {get; set;} = false;
    [Net, Predicted] public int Combo {get; set;} = -1;
    private RealTimeSince LastUpdate = 0f;

    public ArcadeMachineTetris()
    {
        for(int i=0; i<200; i++)
        {
            Board.Add(BlockType.Empty);
        }
    }

    public override void Spawn()
    {
        // TODO: Remove this hack once map uploading is fixed
        if(IsFromMap)
        {
            Rotation = Rotation.FromYaw(180);
        }

        base.Spawn();
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

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if(Game.IsClient) Screen?.Delete();
    }

    public override void StartGame()
    {
        base.StartGame();
        for(int i=0; i<QUEUE_LENGTH; i++)
        {
            Queue.Add(GetRandomBlock());
        }

        Score = 0;
        Combo = -1;
        Level = 1;
        LinesNeeded = 10;

        ShowAll();

        UpdateBoard();
        UpdateNextPieces();
        UpdateHeldPiece();
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
        GrabBag = new List<BlockType>();
        Queue = new List<BlockType>();
        HeldPiece = BlockType.Empty;
        Score = 0;
        UpdateBoard();

        HideAll();

        base.EndGame();
    }

    [GameEvent.Tick.Server]
    public void Tick()
    {
        var interval = GetWaitTime();
        if(FastDrop) interval = MathF.Min(0.04f, interval / 4f);
        if(InUse && LastUpdate > interval)
        {
            if(CurrentPiece == BlockType.Empty)
            {
                CurrentPiece = GetPieceFromQueue();
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
                    Score += 1;
                }
                UpdatePlayer();
            }
            LastUpdate = 0f;
            if(CheckPieceCollision(CurrentPiece, CurrentPieceRotation, new Vector2(CurrentPieceX, CurrentPieceY + 1)))
            {
                LastUpdate = -GetWaitTime()/4;
            }
        }
    }

    public BlockType GetRandomBlock()
    {
        if(GrabBag.Count < QUEUE_LENGTH)
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

    public BlockType GetPieceFromQueue()
    {
        var block = Queue[0];
        Queue.RemoveAt(0);
        Queue.Add(GetRandomBlock());
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

    [ClientRpc]
    public void HideAll()
    {
        Screen?.HideAll();
    }

    [ClientRpc]
    public void ShowAll()
    {
        Screen?.ShowAll();
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
        JustHeld = false;
        Sound.FromEntity("tetros_place", this);
        CurrentPiece = BlockType.Empty;
        CheckLine();
    }

    private void CheckLine()
    {
        int lines = 0;
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
                lines++;
            }
        }
        if(lines > 0)
        {
            Sound.FromEntity("tetros_line", this).SetPitch(1f + (MathF.Max(0, Combo) * (1.0f/12.0f)));
            Combo += 1;
            switch(lines)
            {
                case 1:
                    Score += 100 * Level;
                    break;
                case 2:
                    Score += 300 * Level;
                    break;
                case 3:
                    Score += 500 * Level;
                    break;
                case 4:
                    Sound.FromEntity("tetros_tetros", this);
                    Score += 800 * Level;
                    break;
            }
            if(Combo > 0)
            {
                Score += 50 * (Combo * Level);
            }
            LinesNeeded -= lines;
            if(LinesNeeded <= 0 && Level < 20)
            {
                Level += 1;
                if(Level >= 10 && Level <= 15) LinesNeeded += 100;
                else if(Level > 15) LinesNeeded += 100 + ((Level - 15) * 10);
                else LinesNeeded += Level * 10;
            }
        }
        else
        {
            Combo = -1;
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
            Score += 2;
        }
        CurrentPieceY -= 1;
        PlacePiece();
        LastUpdate = GetWaitTime()/4f * 3f;
    }

    public void Hold()
    {
        if(JustHeld) return;

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
        JustHeld = true;
        Sound.FromEntity("tetros_hold", this);
        UpdateHeldPiece();
        UpdatePlayer();
    }

    public float GetWaitTime()
    {
        switch(Level)
        {
            case 0: return 0.85f;
            case 1: return 0.8f;
            case 2: return 0.72f;
            case 3: return 0.63f;
            case 4: return 0.55f;
            case 5: return 0.47f;
            case 6: return 0.38f;
            case 7: return 0.3f;
            case 8: return 0.22f;
            case 9: return 0.13f;
            case 10:
            case 11:
            case 12:
                return 0.1f;
            case 13:
            case 14:
            case 15:
                return 0.08f;
            case 16:
            case 17:
            case 18:
                return 0.07f;
            case 19: return 0.06f;
            case 20: return 0.05f;
            default: return 0.01f;
        }
    }

}