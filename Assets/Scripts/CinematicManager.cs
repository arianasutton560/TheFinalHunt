
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class CinematicManager : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public string gameSceneName = "SampleScene";

    private bool isLoading = false;

    void Start()
    {
        videoPlayer.isLooping = false;
        videoPlayer.loopPointReached += OnVideoFinished;
    }

    void OnVideoFinished(VideoPlayer player)
    {
        StartGame();
    }

    public void SkipCinematic()
    {
        StartGame();
    }

    void StartGame()
    {
        if (isLoading)
            return;

        isLoading = true;
        videoPlayer.Stop();

        SceneManager.LoadScene(gameSceneName);
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }
}
