using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioMixer masterMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private AudioSource sfxPreviewSource;
    [SerializeField] private AudioClip sfxPreviewClip;
    [SerializeField] private float previewCooldown = 0.12f;

    private const string MUSIC_PARAM = "Music";
    private const string SFX_PARAM = "SoundEffects";
    private const string MUSIC_PREF = "MusicVolDb";
    private const string SFX_PREF = "SFXVolDb";
    private float nextPreviewTime;

    void Start()
    {
        Load(musicSlider, MUSIC_PARAM, MUSIC_PREF);
        Load(sfxSlider, SFX_PARAM, SFX_PREF);
    }

    public void OnMusicChanged(float value)
    {
        SetVolume(value, MUSIC_PARAM, MUSIC_PREF);
    }

    public void OnSfxChanged(float value)
    {
        SetVolume(value, SFX_PARAM, SFX_PREF);
        PlaySfxPreview();
    }

    private void Load(Slider slider, string param, string pref)
    {
        float db = PlayerPrefs.GetFloat(pref, 0f);
        slider.value = DbToLinear(db);
        masterMixer.SetFloat(param, db);
    }

    private void SetVolume(float linear, string param, string pref)
    {
        float db = LinearToDb(linear);
        masterMixer.SetFloat(param, db);
        PlayerPrefs.SetFloat(pref, db);
    }

    private void PlaySfxPreview()
    {
        if (!sfxPreviewSource || !sfxPreviewClip) return;
        if (Time.unscaledTime < nextPreviewTime) return;

        nextPreviewTime = Time.unscaledTime + previewCooldown;
        sfxPreviewSource.PlayOneShot(sfxPreviewClip);
    }

    private float LinearToDb(float linear)
    {
        linear = Mathf.Clamp(linear, 0.0001f, 1f);
        return Mathf.Log10(linear) * 20f;
    }

    private float DbToLinear(float db)
    {
        return Mathf.Pow(10f, db / 20f);
    }
}
