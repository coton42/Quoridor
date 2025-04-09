using UnityEngine;

public class MatchMain : MonoBehaviour
{
    [SerializeField] private GameObject _UIManagerObj;

    private MatchBoardService _matchBoardService;
    private int _currentPlayer;
    private int _playerNum;
    private MatchUiView _matchUiView;

    private void Awake()
    {
        _matchBoardService = MatchBoardService.GetBoard();
        _matchBoardService.InitializeBoard();
        _playerNum = MatchBoardService.playerNum;

        _matchUiView = _UIManagerObj.GetComponent<MatchUiView>();
        _matchUiView.Moved += (x, y) =>
        {
            _matchBoardService.Move(_currentPlayer, x, y);
            if (_matchBoardService.WinnerNum != -1)
                _matchUiView.EndGame(_matchBoardService.WinnerNum);
            else
                ChangeTurn();
        };
        _matchUiView.TriedToPut += (s, t, isVertical) =>
        {
            if (_matchBoardService.TryPutWall(_currentPlayer, s, t, isVertical))
            {
                _matchUiView.UpdateNumWall(_currentPlayer, _matchBoardService.NumsWall[_currentPlayer]);
                _matchUiView.Put(s, t);
                ChangeTurn();
            }
            else
            {
                _matchUiView.ShowMsg(_matchBoardService.ErrorMsg);
            }
        };

        _currentPlayer = Random.Range(0, _playerNum);
    }

    private void Start()
    {
        for (var i = 0; i < _playerNum; i++) _matchUiView.UpdateNumWall(i, _matchBoardService.NumsWall[i]);
        var locs = _matchBoardService.GetListOfAccessibleLocs(_currentPlayer);
        _matchUiView.ChangeTurn(_currentPlayer, locs);
    }

    private void ChangeTurn()
    {
        _currentPlayer = (_currentPlayer + 1) % _playerNum;
        var locs = _matchBoardService.GetListOfAccessibleLocs(_currentPlayer);
        _matchUiView.ChangeTurn(_currentPlayer, locs);
    }
}