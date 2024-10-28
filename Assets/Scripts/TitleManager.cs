using UnityEngine;

public class TitleManager : MonoBehaviour
{
    [SerializeField] private GameObject _howToPlayPnl;

    private void Awake()
    {
        _howToPlayPnl.SetActive(false);
    }

    public void StartMatch(int playerNum)
    {
        Board.playerNum = playerNum;
        SceneLoader.StartMatch();
    }

    public void ShowHowTo()
    {
        _howToPlayPnl.SetActive(true);
    }

    public void HideHowTo()
    {
        _howToPlayPnl.SetActive(false);
    }
}
