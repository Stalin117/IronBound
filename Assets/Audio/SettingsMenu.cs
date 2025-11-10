using UnityEngine;
using UnityEngine.Audio; // ¡Necesario para el AudioMixer!
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio")]
    public AudioMixer mainMixer;
    public Slider musicSlider;
    public Slider sfxSlider;

    // Nombre de los parámetros expuestos en el AudioMixer
    private const string MUSIC_VOLUME_PARAM = "MusicVolume";
    private const string SFX_VOLUME_PARAM = "SFXVolume";

    void Start()
    {
        // Configura los sliders para que llamen a las funciones al moverse
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        // Carga el volumen guardado al iniciar
        LoadVolume();
    }

    void LoadVolume()
    {
        // Carga el valor guardado (de 0.0001 a 1). 
        // Usa 1 como default si no hay nada guardado.
        float musicVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_PARAM, 1f);
        float sfxVol = PlayerPrefs.GetFloat(SFX_VOLUME_PARAM, 1f);

        // Actualiza la posición visual de las barras
        musicSlider.value = musicVol;
        sfxSlider.value = sfxVol;

        // Actualiza el volumen real en el mixer
        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);
    }

    // Esta función es llamada por la barra de Música
    public void SetMusicVolume(float sliderValue)
    {
        // El slider va de 0 a 1. El mixer usa decibelios (logarítmico).
        // Esta fórmula convierte el valor del slider a decibelios.
        // Usamos Log10(0.0001) = -80 (silencio) y Log10(1) = 0 (volumen max)
        mainMixer.SetFloat(MUSIC_VOLUME_PARAM, Mathf.Log10(sliderValue) * 20);
        
        // Guarda la preferencia del jugador (el valor del slider, no el decibelio)
        PlayerPrefs.SetFloat(MUSIC_VOLUME_PARAM, sliderValue);
    }

    // Esta función es llamada por la barra de SFX
    public void SetSFXVolume(float sliderValue)
    {
        mainMixer.SetFloat(SFX_VOLUME_PARAM, Mathf.Log10(sliderValue) * 20);
        PlayerPrefs.SetFloat(SFX_VOLUME_PARAM, sliderValue);
    }
}