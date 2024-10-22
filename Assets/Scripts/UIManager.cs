using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public event Action<int, int> Moved;
    private void OnMoved(int x, int y) => Moved?.Invoke(x, y);

    public event Action<int, int, bool> TriedToPut;
    private void OnTriedToPut() => TriedToPut?.Invoke(_selectedWallCell.Item1, _selectedWallCell.Item2, _isVertical);

    [SerializeField] private GameObject _UICanvas;
    [SerializeField] private GameObject[] _players;
    [SerializeField] private GameObject _cellsParent;
    [SerializeField] private GameObject _WallCellsParent;

    private GameObject _matchPnl;
    private GameObject _resultPnl;
    private GameObject _pausePnl;
    private GameObject _DetermineBtn;
    private GameObject _VHSwitchBtn;

    private readonly int _boardSize = 9;
    private ICellHandler[,] _cells;
    private IWallCellHandler[,] _wallCells;
    private (int, int) _selectedWallCell;
    private TextMeshProUGUI _PWSwitchLbl;
    private TextMeshProUGUI[] _wallNumsTxt;
    private TextMeshProUGUI _errMsg;
    private bool _isWallMode;
    private bool _isVertical;
    private int _currentPlayer = 0;
    private float _playerHeight;
    private IReadOnlyList<(int, int)> _accessibleLocs;

    // public メソッド
    public void Put(int s, int t)
    {
        _wallCells[s, t].Put();
        _wallCells[s, t] = null;
        _selectedWallCell = (-1, -1);
    }

    public void ChangeTurn(int playerIndex, IReadOnlyList<(int, int)> locs)
    {
        if (!_isWallMode) InactivateCells();
        _players[_currentPlayer].transform.GetChild(0).gameObject.SetActive(false);
        _currentPlayer = playerIndex;
        _accessibleLocs = locs;
        _players[playerIndex].transform.GetChild(0).gameObject.SetActive(true);
        if (_isWallMode)
        { 
            ChangeMode(); 
        }
        else
        {
            ActivateCells();
        }
    }

    public void UpdateNumWall(int playerIndex, int num)
    {
        _wallNumsTxt[playerIndex].text = $"P{playerIndex + 1}: {num}";
    }

    public async void ShowMsg(string msg)
    {
        _errMsg.text = msg;
        await Task.Delay(1500);
        _errMsg.text = "";
    }

    public void EndGame(int playerIndex)
    {
        foreach (var cell in _cells)
        {
            Destroy(cell);
        }
        foreach (var wallCell in _wallCells)
        {
            if (wallCell != null) Destroy(wallCell);
        }
        _resultPnl.transform.Find("Winner Name").GetComponent<TextMeshProUGUI>().text = $"Player{playerIndex + 1} Win!";
        _matchPnl.SetActive(false);
        _resultPnl.SetActive(true);
    }


    // Button 用メソッド
    public void Determine()
    {
        var (s, t) = _selectedWallCell;
        if (s >= 0) OnTriedToPut();
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
            _DetermineBtn.SetActive(true);
            _VHSwitchBtn.SetActive(true);
            InactivateCells();
        }
        else
        {
            var (os, ot) = _selectedWallCell;
            if (os >= 0) _wallCells[os, ot].SetTransparency(0f);
            _selectedWallCell = (-1, -1);
            _PWSwitchLbl.text = "壁の配置";
            _DetermineBtn.SetActive(false);
            _VHSwitchBtn.SetActive(false);
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

    // Unity イベント
    private void Awake()
    {
        _matchPnl = _UICanvas.transform.Find("MatchUI").gameObject;
        _resultPnl = _UICanvas.transform.Find("ResultUI").gameObject;
        _pausePnl = _UICanvas.transform.Find("PauseUI").gameObject;
        _DetermineBtn = _matchPnl.transform.Find("Determine").gameObject;
        _PWSwitchLbl = _matchPnl.transform.Find("Change Mode").GetChild(0).GetComponent<TextMeshProUGUI>();
        _VHSwitchBtn = _matchPnl.transform.Find("Change Dir").gameObject;

        _cells = new ICellHandler[_boardSize, _boardSize];
        _wallCells = new IWallCellHandler[_boardSize - 1, _boardSize - 1];

        _resultPnl.SetActive(false);
        _pausePnl.SetActive(false);
        _wallNumsTxt = new TextMeshProUGUI[2];
        _wallNumsTxt[0] = _matchPnl.transform.Find("Wall Nums").Find("P1 Txt").GetComponent<TextMeshProUGUI>();
        _wallNumsTxt[1] = _matchPnl.transform.Find("Wall Nums").Find("P2 Txt").GetComponent<TextMeshProUGUI>();
        _errMsg = _matchPnl.transform.Find("Error Msg").GetComponent<TextMeshProUGUI>();

        _isWallMode = false;
        _isVertical = true;
        _playerHeight = _players[0].transform.position.y;

        _PWSwitchLbl.text = "壁の配置";
        _DetermineBtn.SetActive(false);
        _VHSwitchBtn.SetActive(false);

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
        if (_isWallMode && _wallCells[s, t] != null)
        {
            var (os, ot) = _selectedWallCell;
            if (os >= 0) _wallCells[os, ot].SetTransparency(0f);
            _selectedWallCell = (s, t);
            _wallCells[s, t].SetTransparency(.7f);
        }
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

}
