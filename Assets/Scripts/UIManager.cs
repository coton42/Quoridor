using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public event Action<int, int> Moved;
    private void OnMoved(int x, int y) => Moved?.Invoke(x, y);

    public event Action<int, int, bool> TriedToPut;
    private void OnTriedToPut(int s, int t) => TriedToPut?.Invoke(s, t, _isVertical);

    [SerializeField] private GameObject _UICanvas;
    [SerializeField] private GameObject[] _players;
    [SerializeField] private GameObject _cellsParent;
    [SerializeField] private GameObject _WallCellsParent;

    private GameObject _matchPnl;
    private GameObject _wallPnl;
    private GameObject _resultPnl;
    private GameObject _pausePnl;
    private GameObject _howToPlayPnl;

    private readonly int _boardSize = Board.boardSize;
    private int _playerNum;
    
    private ICellHandler[,] _cells;
    private IWallCellHandler[,] _wallCells;
    private (int, int) _selectedWallCell;
    private RectTransform _wallPnlTransform;
    private TextMeshProUGUI _PWSwitchLbl;
    private TextMeshProUGUI[] _wallNumsTxt;
    private TextMeshProUGUI _errMsg;
    private bool _isWallMode;
    private bool _isVertical;
    private int _currentPlayer = 0;
    private float _playerHeight;
    private IReadOnlyList<(int, int)> _accessibleLocs;
    private int _msgCount = 0;

    // public メソッド
    public void Put(int s, int t)
    {
        _wallCells[s, t].Put();
        _wallCells[s, t] = null;
        _selectedWallCell = (-1, -1);
    }

    public void ChangeTurn(int playerIndex, IReadOnlyList<(int, int)> locs)
    {
        if (!_isWallMode)
        {
            InactivateCells();
            SetWallCellsActivation(true);
        }

        _players[_currentPlayer].transform.Find("Point Light").gameObject.SetActive(false); // プレイヤーのライト消灯
        _currentPlayer = playerIndex;
        _accessibleLocs = locs;
        _players[playerIndex].transform.Find("Point Light").gameObject.SetActive(true); // 点灯

        if (_isWallMode)
        { 
            ChangeMode(); 
        }
        else
        {
            SetWallCellsActivation(false);
            ActivateCells();
        }
    }

    public void UpdateNumWall(int playerIndex, int num)
    {
        var colorcode = ColorUtility.ToHtmlStringRGBA(_players[playerIndex].GetComponent<MeshRenderer>().material.color); 
        _wallNumsTxt[playerIndex].text = $"<color=#{colorcode}>P{playerIndex + 1}</color>: {num}";
    }

    public async void ShowMsg(string msg)
    {
        // 要修正。非同期処理についてもっと学ぶ
        var msgObj = new string(msg.ToCharArray());
        _errMsg.text = msgObj;
        var msgID = ++_msgCount;
        await Task.Delay(1500);
        if (_msgCount == msgID) _errMsg.text = "";
    }

    public void EndGame(int playerIndex)
    {
        InactivateCells();
        foreach (var cell in _cells)
        {
            Destroy(cell);
        }
        foreach (var wallCell in _wallCells)
        {
            if (wallCell != null)
            {
                Destroy(wallCell);
            }
        }
        var colorcode = ColorUtility.ToHtmlStringRGBA(_players[playerIndex].GetComponent<MeshRenderer>().material.color);
        _resultPnl.transform.Find("Winner Name").GetComponent<TextMeshProUGUI>().text = $"<color=#{colorcode}>Player{playerIndex + 1}</color> Win!";
        _matchPnl.SetActive(false);
        _resultPnl.SetActive(true);
    }

    // Button 用メソッド
    public void Determine()
    {
        var (s, t) = _selectedWallCell;
        if (s >= 0)
        {
            OnTriedToPut(s, t);
        }
    }

    public void ChangeDir()
    {
        _isVertical ^= true;
        foreach (var wallCell in _wallCells)
        {
            wallCell?.ChangeDir();
        }
    }

    public void ChangeMode()
    {
        _isWallMode ^= true; // 反転
        if (_isWallMode)
        {
            _PWSwitchLbl.text = "コマの移動";
            _wallPnl.SetActive(true);
            _wallPnlTransform.position = new Vector2(99999, 99999);
            InactivateCells();
            SetWallCellsActivation(true);
        }
        else
        {
            var (os, ot) = _selectedWallCell;
            if (os >= 0)
            {
                _wallCells[os, ot].Deselect();
            }
            _selectedWallCell = (-1, -1);
            _PWSwitchLbl.text = "壁の配置";
            _wallPnl.SetActive(false);
            SetWallCellsActivation(false);
            ActivateCells();
        }
    }

    public void PauseGame()
    {
        _matchPnl.SetActive(false);
        _pausePnl.SetActive(true);
    }

    public void ReturnToGame()
    {
        _pausePnl.SetActive(false);
        _matchPnl.SetActive(true);
    }

    public void ResetGame()
    {
        SceneLoader.StartMatch();
    }

    public void ReturnToTitle()
    {
        SceneLoader.ReturnToTitle();
    }
    public void ShowHowTo()
    {
        _howToPlayPnl.SetActive(true);
    }

    public void HideHowTo()
    {
        _howToPlayPnl.SetActive(false);
    }

    // Unity イベント
    private void Awake()
    {
        _matchPnl = _UICanvas.transform.Find("MatchUI").gameObject;
        _wallPnl = _UICanvas.transform.Find("WallUI").gameObject;
        _resultPnl = _UICanvas.transform.Find("ResultUI").gameObject;
        _pausePnl = _UICanvas.transform.Find("PauseUI").gameObject;
        _howToPlayPnl = _pausePnl.transform.Find("How to Play").gameObject;
        _PWSwitchLbl = _matchPnl.transform.Find("Change Mode").Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
        _wallPnlTransform = _wallPnl.GetComponent<RectTransform>();

        _cells = new ICellHandler[_boardSize, _boardSize];
        _wallCells = new IWallCellHandler[_boardSize - 1, _boardSize - 1];

        _resultPnl.SetActive(false);
        _pausePnl.SetActive(false);
        _playerNum = Board.playerNum;
        
        for (int i = 0; i < _playerNum; i++)
        {
            _players[i].SetActive(true);
        }
        for (int i = _playerNum; i < _players.Length; i++)
        {
            _players[i].SetActive(false);
        }

        _wallNumsTxt = new TextMeshProUGUI[_playerNum];
        for (int i = 0; i < _playerNum; i++)
        {
            _wallNumsTxt[i] = _matchPnl.transform.Find("Wall Nums").Find($"P{i + 1} Txt").GetComponent<TextMeshProUGUI>();
        }
        if (_playerNum == 2)
        {
            _wallNumsTxt[0].gameObject.GetComponent<RectTransform>().localPosition += Vector3.down * 40;
            _wallNumsTxt[1].gameObject.GetComponent<RectTransform>().localPosition += Vector3.down * 40;
        }

        _errMsg = _matchPnl.transform.Find("Error Msg").GetComponent<TextMeshProUGUI>();

        _isWallMode = false;
        _isVertical = true;
        _playerHeight = _players[0].transform.position.y;

        _PWSwitchLbl.text = "壁の配置";
        _wallPnl.SetActive(false);

        _selectedWallCell = (4, 4);
    }

    private void Start()
    {
        var cells = _cellsParent.GetComponentsInChildren<ICellHandler>();
        foreach (var cell in cells)
        {
            cell.Clicked += Move;
            _cells[cell.X, cell.Y] = cell;
        }

        var wallCells = _WallCellsParent.GetComponentsInChildren<IWallCellHandler>();
        foreach (var wallCell in wallCells)
        {
            wallCell.Selected += UpdateSelectedWallCell;
            wallCell.SetActivation(false);
            _wallCells[wallCell.S, wallCell.T] = wallCell;
        }
    }

    // private メソッド
    private void Move(int x, int y)
    {
        _players[_currentPlayer].transform.position = _cells[x, y].transform.position + new Vector3(0, _playerHeight, 0);
        OnMoved(x, y);
    }

    private void UpdateSelectedWallCell(int s, int t)
    {
        if ((s, t) == _selectedWallCell) return;
        var (os, ot) = _selectedWallCell;
        if (os >= 0)
        {
            _wallCells[os, ot].Deselect();
        }
        _selectedWallCell = (s, t);

        var pos = RectTransformUtility.WorldToScreenPoint(Camera.main, _wallCells[s, t].transform.position);
        _wallPnl.GetComponent<RectTransform>().position = pos;
    }

    private void ActivateCells()
    {
        if (_accessibleLocs != null)
        {
            var color = _players[_currentPlayer].GetComponent<Renderer>().material.color;
            foreach (var loc in _accessibleLocs)
            {
                var (x, y) = loc;
                _cells[x, y].Activate(color);
            }
        }
    }

    private void InactivateCells()
    {
        if (_accessibleLocs != null)
        {
            foreach (var loc in _accessibleLocs)
            {
                var (x, y) = loc;
                _cells[x, y].Inactivate();
            }
        }
    }

    private void SetWallCellsActivation(bool isActivated)
    {
        foreach (var wallCell in  _wallCells)
        {
            wallCell?.SetActivation(isActivated);
        }
    }
}
