
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public GameObject infoPanel;

    public void PlayGame()
    {
        SceneManager.LoadScene("IntroCinematic");
    }

    public void ShowInfo()
    {
        infoPanel.SetActive(true);
    }

    public void CloseInfo()
    {
        infoPanel.SetActive(false);
    }
}

