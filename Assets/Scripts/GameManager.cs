using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _UIManagerObj;

    private Board _board;
    private int _playerNum;
    private UIManager _UIManager;
    private int _currentPlayer;

    private void Awake()
    {
        _board = Board.GetBoard();
        _board.InitializeBoard();
        _playerNum = Board.playerNum;

        _UIManager = _UIManagerObj.GetComponent<UIManager>();
        _UIManager.Moved += (x, y) =>
        {
            _board.Move(_currentPlayer, x, y);
            if (_board.WinnerNum != -1)
            {
                _UIManager.EndGame(_board.WinnerNum);
            }
            else
            {
                ChangeTurn();
            }
        };
        _UIManager.TriedToPut += (s, t, isVertical) =>
        {
            if (_board.TryPutWall(_currentPlayer, s, t, isVertical))
            {
                _UIManager.UpdateNumWall(_currentPlayer, _board.NumsWall[_currentPlayer]);
                _UIManager.Put(s, t);
                ChangeTurn();
            }
            else
            {
                _UIManager.ShowMsg(_board.ErrorMsg);
            }
        };

        _currentPlayer = Random.Range(0, _playerNum);
    }

    private void Start()
    {
        for (var i = 0; i < _playerNum; i++)
        {
            _UIManager.UpdateNumWall(i, _board.NumsWall[i]);
        }
        var locs = _board.GetListOfAccessibleLocs(_currentPlayer);
        _UIManager.ChangeTurn(_currentPlayer, locs);
    }

    private void ChangeTurn()
    {
        _currentPlayer = (_currentPlayer + 1) % _playerNum;
        var locs = _board.GetListOfAccessibleLocs(_currentPlayer);
        _UIManager.ChangeTurn(_currentPlayer, locs);
    }
}

