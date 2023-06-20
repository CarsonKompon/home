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

    public Panel BoardPanel {get; set;}
    public Panel[] Blocks {get; set;} = new Panel[200];
    public Panel[] CurrentBlocks {get; set;} = new Panel[4];
    public Panel[][] NextBlocks {get; set;} = new Panel[5][];
    public Panel[] HoldBlocks {get; set;} = new Panel[4];
    public Panel[] GhostBlocks {get; set;} = new Panel[4];

    // Game Variables
    public enum BlockType { Empty, I, O, T, S, Z, J, L };
    const int BOARD_WIDTH = 10;
    const int QUEUE_LENGTH = 5;
    public BlockType HeldPiece {get; set;} = BlockType.Empty;
    public int Level {get; set;} = 1;
    public int LinesNeeded {get; set;} = 10;
    private List<BlockType> GrabBag {get; set;} = new List<BlockType>();
    private List<BlockType> Queue {get; set;} = new List<BlockType>();
    public List<BlockType> Board {get; set;} = new List<BlockType>();
    public BlockType CurrentPiece {get; set;} = new BlockType();
    public long Score {get; set;} = 0;
    public long HighScore {get; set;} = 0;
    public int CurrentPieceX {get; set;}
    public int CurrentPieceY {get; set;}
    public int CurrentPieceRotation {get; set;} = 0;
    public bool FastDrop {get; set;} = false;
    public bool JustHeld {get; set;} = false;
    public int Combo {get; set;} = -1;
    public bool Playing {get; set;} = false;
    private RealTimeSince LastUpdate = 0f;
    
    public ArcadeMachineTetris Machine;

    public ArcadeScreenTetris()
    {
        Board = new List<BlockType>();
        for(int i=0; i<200; i++)
        {
            Board.Add(BlockType.Empty);
        }

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
        BoardPanel = Add.Panel("board");

        for(int i=0; i<200; i++)
        {
            Blocks[i] = BoardPanel.Add.Panel("block");
        }

        for(int i=0; i<4; i++)
        {
            CurrentBlocks[i] = BoardPanel.Add.Panel("block current");
        }

        for(int i=0; i<4; i++)
        {
            GhostBlocks[i] = BoardPanel.Add.Panel("block ghost");
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

    public void StartGame()
    {
        for(int i=0; i<QUEUE_LENGTH; i++)
        {
            Queue.Add(GetRandomBlock());
        }

        Score = 0;
        Combo = -1;
        Level = 1;
        LinesNeeded = 10;
        Playing = true;

        ShowAll();
    }

    public void EndGame(long steamId)
    {
        ArcadeMachineTetris.Payout(steamId, Score);
        SaveHighScore();

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
        Playing = false;

        RequestUpdateBoard();

        HideAll();
    }

    public void LoadHighScore()
    {
        HighScore = Cookie.Get<long>("home.arcade.tetros.hiscore", 0);
        UpdateHighScore(Score);
    }

    public void SaveHighScore()
    {
        if(Score > Cookie.Get<long>("home.arcade.tetros.hiscore", 0))
        {
            Cookie.Set("home.arcade.tetros.hiscore", Score);
        }
    }

    [GameEvent.Client.Frame]
    public void OnFrame()
    {
        Style.Opacity = MathX.Clamp(1.25f - (Vector3.DistanceBetween( Camera.Position, Position ) * 0.004f), 0f, 1f);

        var interval = GetWaitTime();
        if(FastDrop) interval = MathF.Min(0.04f, interval / 4f);
        if(Playing && LastUpdate > interval)
        {
            if(CurrentPiece == BlockType.Empty)
            {
                CurrentPiece = GetPieceFromQueue();
                CurrentPieceX = 5;
                CurrentPieceY = -2;
                CurrentPieceRotation = 0;
                RequestUpdatePlayer();
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
                    Sound.FromEntity("tetros_move", Machine).SetPitch(1.5f);
                    Score += 1;
                }
                RequestUpdatePlayer();
            }
            LastUpdate = 0f;
            if(CheckPieceCollision(CurrentPiece, CurrentPieceRotation, new Vector2(CurrentPieceX, CurrentPieceY + 1)))
            {
                LastUpdate = -GetWaitTime()/4;
            }
        }
    }

    [GameEvent.Client.BuildInput]
    public void BuildInput()
    {
        if(!Playing) return;
        if(Game.LocalClient.SteamId != Machine.CurrentUser.Client.SteamId) return;

        if(Input.Pressed("Jump"))
        {
            HardDrop();
        }
        else if(Input.Pressed("Run"))
        {
            Hold();
        }
        else
        {
            if(Input.Pressed("Left"))
            {
                Move(-1);
            }
            if(Input.Pressed("Right"))
            {
                Move(1);
            }
            if(Input.Pressed("Forward"))
            {
                Rotate();
            }
            FastDrop = Input.Down("Backward");
        }
    }

    #region UI UPDATING

        private void RequestUpdateBoard()
        {
            int[] board = new int[200];
            for(int i=0; i<200; i++)
            {
                board[i] = (int)Board[i];
            }
            ArcadeMachineTetris.RequestUpdateBoard(Machine.NetworkIdent, BoardToString(board));
            UpdateBoard(board);
        }

        public void UpdateBoard(int[] board)
        {
            for(int i=0; i<200; i++)
            {
                int val = board[i];
                Blocks[i].SetClass("t-1 t-2 t-3 t-4 t-5 t-6 t-7", false);
                Blocks[i].SetClass("active t-" + val.ToString(), val != 0);
            }
            ScoreLabel.Text = Score.ToString();
            LevelLabel.Text = Level.ToString();
        }

        private void RequestUpdateScore()
        {
            ArcadeMachineTetris.RequestScore(Machine.NetworkIdent, Score);
            UpdateScore(Score);
        }

        public void UpdateScore(long score)
        {
            ScoreLabel.Text = score.ToString();
        }

        private void RequestUpdatePlayer()
        {
            ArcadeMachineTetris.RequestUpdatePlayer(Machine.NetworkIdent, (int)CurrentPiece, CurrentPieceX, CurrentPieceY, CurrentPieceRotation);
            UpdatePlayer(CurrentPiece, new Vector2(CurrentPieceX, CurrentPieceY), CurrentPieceRotation);
        }

        public void UpdatePlayer(BlockType blockType, Vector2 pos, int rotation)
        {
            if(blockType == BlockType.Empty)
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
            while(!CheckPieceCollision(blockType, rotation, new Vector2(pos.x, ghostY)))
            {
                ghostY++;
            }
            ghostY--;
            SetPositionFromPiece(GhostBlocks, blockType, new Vector2(pos.x, ghostY), rotation);
        }

        private void RequestHeldPiece()
        {
            ArcadeMachineTetris.RequestHeldPiece(Machine.NetworkIdent, (int)HeldPiece);
            UpdateHeldPiece(HeldPiece);
        }

        public void UpdateHeldPiece(BlockType blockType)
        {
            if(blockType == BlockType.Empty)
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
                HoldBlocks[i].SetClass("current t-" + ((int)blockType).ToString(), blockType != BlockType.Empty);
            }
            SetPositionFromPiece(HoldBlocks, blockType, new Vector2(0, 0), 0);
        }

        private void RequestNextPieces()
        {
            int[] nextPieces = new int[Queue.Count()];
            for(int i=0; i<Queue.Count(); i++)
            {
                nextPieces[i] = (int)Queue[i];
            }
            ArcadeMachineTetris.RequestNextPieces(Machine.NetworkIdent, BoardToString(nextPieces));
            UpdateNextPieces(Queue.ToArray());
        }

        public void UpdateNextPieces(BlockType[] blockTypes)
        {
            for(int i=0; i<blockTypes.Count(); i++)
            {
                if(i >= NextBlocks.Count()) break;
                for(int j=0; j<NextBlocks[i].Count(); j++)
                {
                    NextBlocks[i][j].SetClass("t-1 t-2 t-3 t-4 t-5 t-6 t-7", false);
                    if(i < blockTypes.Count()) NextBlocks[i][j].SetClass("current t-" + ((int)blockTypes[i]).ToString(), blockTypes[i] != BlockType.Empty);
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

    #endregion

    #region GRAB BAG / QUEUE

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

            RequestNextPieces();
            return block;
        }

        public BlockType GetPieceFromQueue()
        {
            var block = Queue[0];
            Queue.RemoveAt(0);
            Queue.Add(GetRandomBlock());
            return block;
        }

    #endregion


    #region COLLISION CHECK

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

    #endregion

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
                    ArcadeMachineTetris.RequestEndGame(Machine.NetworkIdent);
                    Machine.RemoveUser();
                    return;
                }
                if(pos < Board.Count)
                {
                    Board[pos] = CurrentPiece;
                }
            }
        }
        JustHeld = false;
        Sound.FromEntity("tetros_place", Machine);
        CurrentPiece = BlockType.Empty;
        
        RequestUpdateBoard();
        CheckLine();
    }

    #region LINE CHECK

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
                Sound.FromEntity("tetros_line", Machine).SetPitch(1f + (MathF.Max(0, Combo) * (1.0f/12.0f)));
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
                        Sound.FromEntity("tetros_tetros", Machine);
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
                RequestUpdateBoard();
                RequestUpdateScore();
            }
            else
            {
                Combo = -1;
            }
        }

    #endregion

    #region CONTROLS

        public void Move(int dir)
        {
            CurrentPieceX += MathF.Sign(dir);
            if(CheckPieceCollision(CurrentPiece, CurrentPieceRotation, new Vector2(CurrentPieceX, CurrentPieceY)))
            {
                CurrentPieceX -= MathF.Sign(dir);
            }
            Sound.FromEntity("tetros_move", Machine);
            RequestUpdatePlayer();
        }

        public void Rotate()
        {
            CurrentPieceRotation = (CurrentPieceRotation + 1) % 4;
            if(CheckPieceCollision(CurrentPiece, CurrentPieceRotation, new Vector2(CurrentPieceX, CurrentPieceY)))
            {
                CurrentPieceRotation = (CurrentPieceRotation - 1) % 4;
            }
            LastUpdate /= 2;
            Sound.FromEntity("tetros_rotate", Machine);
            RequestUpdatePlayer();
        }

        public void HardDrop()
        {
            while(!CheckPieceCollision(CurrentPiece, CurrentPieceRotation, new Vector2(CurrentPieceX, CurrentPieceY)))
            {
                CurrentPieceY += 1;
                Score += 2;
            }
            RequestUpdateScore();
            Score -= 2;
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
            Sound.FromEntity("tetros_hold", Machine);
            RequestHeldPiece();
            RequestUpdatePlayer();
        }

    #endregion

    #region HELPER FUNCTIONS

        public void SetPositionFromPiece(Panel[] panel, BlockType blockType, Vector2 pos, int rotation)
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

        public static string BoardToString(int[] board)
        {
            string str = "";
            for(int i=0; i<board.Length; i++)
            {
                str += board[i].ToString();
            }
            return str;
        } 

        public static int[] StringToBoard(string str)
        {
            int[] board = new int[str.Length];
            for(int i=0; i<board.Length; i++)
            {
                board[i] = int.Parse(str[i].ToString());
            }
            return board;
        }

    #endregion

}