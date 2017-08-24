using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverScreenInput : MonoBehaviour
{
    private bool canProceed;

    private void Start()
    {
        StartCoroutine(WaitBeforeCanProceed());
    }

    private IEnumerator WaitBeforeCanProceed()
    {
        yield return new WaitForSeconds(0.5f);
        canProceed = true;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartGame();
        }
    }

    public void StartGame()
    {
        if (!canProceed) return;

        if (GameController.Instance.Money > 0)
        {
            UIController.Instance.ScreenFadeController.FadeOut(0.5f, UIController.Instance.GameOverCanvasGroup);
            GameController.Instance.StartGame();

            canProceed = false;

            Destroy(this);
        }
        else
        {
            UIController.Instance.ScreenFadeController.FadeIn(0.5f, UIController.Instance.GameOverCanvasGroup, 255, 180);
            canProceed = false;
            StartCoroutine(Reload());
        }
    }

    private static IEnumerator Reload()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }
}