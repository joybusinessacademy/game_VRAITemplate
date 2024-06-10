using UnityEngine;

public class WindowsEXEController : MonoBehaviour
{
#if !UNITY_ANDROID
    private GameObject pauseMenuCanvas;

    private bool isPaused = false;

	private void Awake()
	{
        GameObject canvasPrefab = Resources.Load<GameObject>("EXECanvas");
        if (canvasPrefab != null)
            pauseMenuCanvas = Instantiate(canvasPrefab, Vector3.zero, Quaternion.identity);

        if(pauseMenuCanvas != null)
		{
            WindowsEXECanvas exeCanvas = pauseMenuCanvas.GetComponent<WindowsEXECanvas>();
            exeCanvas.continueButton.onClick.AddListener(ResumeGame);
            exeCanvas.quitButton.onClick.AddListener(QuitGame);

            ResumeGame();
        }
    }

	private void OnDestroy()
	{
        if (pauseMenuCanvas != null)
        {
            WindowsEXECanvas exeCanvas = pauseMenuCanvas.GetComponent<WindowsEXECanvas>();
            exeCanvas.continueButton.onClick.RemoveListener(ResumeGame);
            exeCanvas.quitButton.onClick.RemoveListener(QuitGame);
        }
    }

	void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        pauseMenuCanvas.SetActive(true);
    }

    void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; 
        pauseMenuCanvas.SetActive(false);
    }

    void QuitGame()
    {
        Application.Quit();
    }
#endif
}