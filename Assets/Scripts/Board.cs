using System;
using System.Collections.Generic;
using System.Linq;

public class Board
{
    private enum Direction
    {
        North,
        East,
        South,
        West
    }

    private class Player
    {
        public int X { get; private set; }
        public int Y { get; private set; }
        public Direction Dir { get; }

        public Player(int x, int y, Direction dir)
        {
            X = x;
            Y = y;
            Dir = dir;
        }

        public void Move(int newX, int newY)
        {
            X = newX;
            Y = newY;
        }
    }

    public event Action<int> Won;
    private void OnWon(int playerIndex) => Won?.Invoke(playerIndex);

    public event Action<String> SelectedInaccessibleLoc;
    private void OnSelectedInaccessibleLoc(String msg) => SelectedInaccessibleLoc?.Invoke(msg);

    public int[] NumsWall { get; private set; }

    private readonly int _boardSize;
    private readonly int _boardMatSize;
    private readonly int _leftEnd, _rightEnd, _center;
    private readonly bool[,] _boardMat;
    private readonly Player[] _players;

    public Board()
    {
        _boardSize = 9;
        _leftEnd = 0;
        _rightEnd = _boardSize - 1;
        _center = (_leftEnd + _rightEnd) / 2;
        _boardMatSize = _boardSize * 2 + 1;
        _boardMat = new bool[_boardMatSize, _boardMatSize];
        InitializeBoard();
        _players = new Player[2];
        _players[0] = new Player(_rightEnd, _center, Direction.North);
        _players[1] = new Player(_leftEnd, _center, Direction.South);
        NumsWall = new int[2] { 10, 10 };
    }

    private void InitializeBoard()
    {
        for (int i = 0; i < _boardMatSize; i++)
        {
            _boardMat[i, 0] = true;
            _boardMat[i, _boardMatSize - 1] = true;
            _boardMat[0, i] = true;
            _boardMat[_boardMatSize - 1, i] = true;
        }
    }

    public void Move(int playerIndex, int newX, int newY)
    {
        var p = _players[playerIndex];
        var (i, j) = GetBoardIndexFromCellLoc(p.X, p.Y);
        _boardMat[i, j] = false;
        (i, j) = GetBoardIndexFromCellLoc(newX, newY);
        _boardMat[i, j] = true;
        p.Move(newX, newY);

        if (IsWinning(p.X, p.Y, p.Dir))
        {
            OnWon(playerIndex);
        }
    }

    private bool IsWinning(int x, int y, Direction dir) =>
        dir switch
        {
            Direction.North => x == _leftEnd,
            Direction.South => x == _rightEnd,
            Direction.West => y == _leftEnd,
            Direction.East => y == _rightEnd,
            _ => false
        };

    public IReadOnlyList<(int, int)> GetListOfAccessibleLocs(int playerIndex)
    {
        var p = _players[playerIndex];
        var (i, j) = GetBoardIndexFromCellLoc(p.X, p.Y);
        var locs = new List<(int, int)>();

        foreach (Direction dir in Enum.GetValues(typeof(Direction)))
        {
            locs.AddRange(GetAccessibleLocs(i, j, dir));
        }
        return locs.Select(t => ((t.Item1 - 1) / 2, (t.Item2 - 1) / 2)).ToList().AsReadOnly();
    }

    private IEnumerable<(int, int)> GetAccessibleLocs(int i, int j, Direction dir)
    {
        var (wallI, wallJ) = GetBoardIndexFromDist(i, j, 1, dir);
        if (!_boardMat[wallI, wallJ]) // 壁がなければ
        {
            var (newI, newJ) = GetBoardIndexFromDist(i, j, 2, dir);
            if (_boardMat[newI, newJ]) // そこにプレイヤーが居るならば
            {
                (wallI, wallJ) = GetBoardIndexFromDist(i, j, 3, dir);
                if (_boardMat[wallI, wallJ]) // さらにその先に壁があれば
                {
                    // 左方向のチェック
                    var left = (Direction)(((int)dir + 3) % 4);
                    var (leftWallI, leftWallJ) = GetBoardIndexFromDist(newI, newJ, 1, left);
                    var (leftNewI, leftNewJ) = GetBoardIndexFromDist(newI, newJ, 2, left);
                    if (!_boardMat[leftWallI, leftWallJ] && !_boardMat[leftNewI, leftNewJ])
                    {
                        yield return (leftNewI, leftNewJ);
                    }

                    // 右方向のチェック
                    var right = (Direction)(((int)dir + 1) % 4);
                    var (rightWallI, rightWallJ) = GetBoardIndexFromDist(newI, newJ, 1, right);
                    var (rightNewI, rightNewJ) = GetBoardIndexFromDist(newI, newJ, 2, right);
                    if (!_boardMat[rightWallI, rightWallJ] && !_boardMat[rightNewI, rightNewJ])
                    {
                        yield return (rightNewI, rightNewJ);
                    }
                }
                else // さらにその先に壁がなければ
                {
                    var (furtherI, furtherJ) = GetBoardIndexFromDist(i, j, 4, dir);
                    if (!_boardMat[furtherI, furtherJ]) // さらにその先にプレイヤーがいなければ
                    {
                        yield return (furtherI, furtherJ);
                    }
                }
            }
            else // そこにプレイヤーがいなければ
            {
                yield return (newI, newJ);
            }
        }
    }

    private bool ExistPath()
    {
        bool[,] isVisited;
        foreach (var p in _players)
        {
            isVisited = new bool[_boardSize, _boardSize];
            if (!_FindPath(p.X, p.Y, p.Dir, isVisited)) return false;
        }
        return true;

        bool _FindPath(int x, int y, Direction playerDir, bool[,] isVisited)
        {
            if (isVisited[x, y]) return false;
            isVisited[x, y] = true;

            if (IsWinning(x, y, playerDir)) return true;

            var (i, j) = GetBoardIndexFromCellLoc(x, y);
            var seq = new int[] { 0, 1, 3, 2 };
            foreach (var c in seq)
            {
                var nextDir = (Direction)(((int)playerDir + c) % 4);
                var (wallI, wallJ) = GetBoardIndexFromDist(i, j, 1, nextDir);
                if (_boardMat[wallI, wallJ]) continue;
                var (nextI, nextJ) = GetBoardIndexFromDist(i, j, 2, nextDir);
                var (nextX, nextY) = GetCellLocFromBoardIndex(nextI, nextJ);
                if (_FindPath(nextX, nextY, playerDir, isVisited)) return true;
            }

            return false;
        }
    }

    public bool TryPutWall(int playerIndex, int s, int t, bool isVertical)
    {
        if (NumsWall[playerIndex] <= 0)
        {
            OnSelectedInaccessibleLoc("手持ちの壁がありません！");
            return false;
        }
        var (i, j) = GetBoardIndexFromWallLoc(s, t);
        if (_boardMat[i, j])
        {
            return false;
        }

        var io1 = isVertical ? -1 : 0;
        var io2 = isVertical ? 1 : 0;
        var jo1 = isVertical ? 0 : -1;
        var jo2 = isVertical ? 0 : 1;

        if (_boardMat[i + io1, j + jo1] || _boardMat[i + io2, j + jo2])
        {
            OnSelectedInaccessibleLoc("壁が重なっています！");
            return false;
        }

        _boardMat[i, j] = true;
        _boardMat[i + io1, j + jo1] = true;
        _boardMat[i + io2, j + jo2] = true;

        if (!ExistPath())
        {
            _boardMat[i, j] = false;
            _boardMat[i + io1, j + jo1] = false;
            _boardMat[i + io2, j + jo2] = false;
            OnSelectedInaccessibleLoc("そこには壁をおけません！");
            return false;
        }

        NumsWall[playerIndex]--;
        return true;
    }

    /*
     * マスの座標 (x, y) について 0 <= x, y <= 8 
     * boardMat のインデックス (i, j) に対して
     * (i, j) = 2 * (x, y) + (1, 1)
    */
    private (int, int) GetBoardIndexFromCellLoc(int x, int y) => (2 * x + 1, 2 * y + 1);
    private (int, int) GetCellLocFromBoardIndex(int i, int j) => ((i - 1) / 2, (j - 1) / 2);


    /*
     * 壁の座標（格子点）(s, t) について 0 <= s, t <= 7 
     * boardMat のインデックス (i, j) に対して
     * (i, j) = 2 * (s, t) + (2, 2)
    */
    private (int, int) GetBoardIndexFromWallLoc(int s, int t) => (2 * s + 2, 2 * t + 2);

    private (int, int) GetBoardIndexFromDist(int i, int j, int dist, Direction dir) =>
        dir switch
        {
            Direction.North => (i - dist, j),
            Direction.South => (i + dist, j),
            Direction.West => (i, j - dist),
            Direction.East => (i, j + dist),
            _ => (-1, -1)
        };
}
