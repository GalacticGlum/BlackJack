using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScreenFadeController : MonoBehaviour
{
    private const byte TargetAlpha = 180;

    [SerializeField]
    private SpriteRenderer fadeScreenSpriteRenderer;
    [SerializeField]
    #pragma warning disable 649
    private CanvasGroup hudCanvasGroup;
    private Dictionary<CanvasGroup, List<LerpInformation<float>>> activeFadeCanvasGroups;

    private void Start()
    {
        activeFadeCanvasGroups = new Dictionary<CanvasGroup, List<LerpInformation<float>>>();
    }

    private void Update()
    {
        for (int i = activeFadeCanvasGroups.Count - 1; i >= 0; i--)
        {
            Fade(activeFadeCanvasGroups.Keys.ElementAt(i));
        }
    }

    private void Fade(CanvasGroup target)
    {
        if (target == null || !activeFadeCanvasGroups.ContainsKey(target)) return;
        for (int i = activeFadeCanvasGroups[target].Count - 1; i >= 0; i--)
        {
            if (activeFadeCanvasGroups[target][i].TimeLeft <= 0)
            {
                activeFadeCanvasGroups[target].Remove(activeFadeCanvasGroups[target][i]);
                continue;
            }

            float alpha = activeFadeCanvasGroups[target][i].Step(Time.deltaTime) / 255f;
            fadeScreenSpriteRenderer.color = new Color(0, 0, 0, alpha);
            target.alpha = 1 - alpha;
        }
    }

    public void FadeIn(float duration, CanvasGroup target = null, float targetAlpha = TargetAlpha, float startAlpha = 0)
    {
        if (target == null)
        {
            target = hudCanvasGroup;
        }

        if (!activeFadeCanvasGroups.ContainsKey(target))
        {
            activeFadeCanvasGroups[target] = new List<LerpInformation<float>>();
        }

        activeFadeCanvasGroups[target].Add(new LerpInformation<float>(startAlpha, targetAlpha, duration, Mathf.Lerp));
    }

    public void FadeOut(float duration, CanvasGroup target = null, float targetAlpha = 1)
    {
        if (target == null)
        {
            target = hudCanvasGroup;
        }


        if (!activeFadeCanvasGroups.ContainsKey(target))
        {
            activeFadeCanvasGroups[target] = new List<LerpInformation<float>>();
        }

        activeFadeCanvasGroups[target].Add(new LerpInformation<float>(TargetAlpha, targetAlpha, duration, Mathf.Lerp));
    }
}