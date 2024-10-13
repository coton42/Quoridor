using UnityEngine.SceneManagement;

public static class SceneLoader
{
    public static void StartMatch()
    {
        SceneManager.LoadScene("Match");
    }

    public static void ReturnToTitle()
    {
        SceneManager.LoadScene("Title");
    }
}
