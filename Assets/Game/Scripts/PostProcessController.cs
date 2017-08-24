using UnityEngine;
using UnityEngine.PostProcessing;

[RequireComponent(typeof(PostProcessingBehaviour))]
public class PostProcessController : MonoBehaviour
{
    public static PostProcessController Instance { get; private set; }
    public float DefaultVignetteIntensity { get; private set; }

    private PostProcessingProfile profile;
    private LerpInformation<float> vignetteFadeLerpInformation;

    private void OnEnable()
    {
        Instance = this;

        profile = GetComponent<PostProcessingBehaviour>().profile;
        DefaultVignetteIntensity = profile.vignette.settings.intensity;

        SetVignetteIntensity(0);
    }

    private void Update()
    {
        HandleVignetteFade();
    }

    private void HandleVignetteFade()
    {
        if (vignetteFadeLerpInformation == null || vignetteFadeLerpInformation.TimeLeft <= 0)
        {
            vignetteFadeLerpInformation = null;
            return;
        }

        SetVignetteIntensity(vignetteFadeLerpInformation.Step(Time.deltaTime));
    }
    
    public void FadeVignette(float target, float duration)
    {
        vignetteFadeLerpInformation = new LerpInformation<float>(profile.vignette.settings.intensity, target, duration, Mathf.Lerp);
    }

    public void SetVignetteIntensity(float intensity)
    {
        VignetteModel.Settings vignetteSettings = profile.vignette.settings;
        vignetteSettings.intensity = intensity;
        profile.vignette.settings = vignetteSettings;
    }
}