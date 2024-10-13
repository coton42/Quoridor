using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _UIManagerObj;

    private Board _board;
    private UIManager _UIManager;
    private readonly int _playerNum = 2;
    private int _currentPlayer;

    private void Awake()
    {
        _board = new Board();
        _board.Won += EndGame;
        _board.SelectedInaccessibleLoc += ShowMsg;

        _UIManager = _UIManagerObj.GetComponent<UIManager>();
        _UIManager.Moved += (x, y) =>
        {
            _board.Move(_currentPlayer, x, y);
            ChangeTurn();
        };
        _UIManager.TriedToPut += (s, t, isVertical) =>
        {
            if (_board.TryPutWall(_currentPlayer, s, t, isVertical))
            {
                _UIManager.UpdateNumWall(_currentPlayer, _board.NumsWall[_currentPlayer]);
                _UIManager.Put(s, t);
                ChangeTurn();
            }
        };

        _currentPlayer = Random.Range(0, 2);
    }

    private void Start()
    {
        for (var i = 0; i < _playerNum; i++)
        {
            _UIManager.UpdateNumWall(i, _board.NumsWall[i]);
        }
        ChangeTurn();
    }

    private void ChangeTurn()
    {
        _currentPlayer = (_currentPlayer + 1) % _playerNum;
        var locs = _board.GetListOfAccessibleLocs(_currentPlayer);
        _UIManager.ChangeTurn(_currentPlayer, locs);
    }

    private void ShowMsg(string msg)
    {
        _UIManager.ShowMsg(msg);
    }

    private void EndGame(int winnerIndex)
    {
        _UIManager.EndGame(winnerIndex);
    }
}

