using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class enemigo_sound : MonoBehaviour
{
    [Header("Sonidos")]
    [SerializeField] private AudioClip movimiento;
    [SerializeField] private AudioClip ataque;
    [SerializeField] private AudioClip daño;
    [SerializeField] private AudioClip muerte;
    [SerializeField] private AudioClip fatality;

    private AudioSource movimientoSource;
    private AudioSource efectosSource;

    private void Awake()
    {
        // Obtener todos los audios del objeto
        AudioSource[] sources = GetComponents<AudioSource>();

        if (sources.Length == 1)
        {
            movimientoSource = sources[0];

            // Crear segundo AudioSource para efectos
            efectosSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            movimientoSource = sources[0];
            efectosSource = sources[1];
        }

        ConfigurarAudioSource(movimientoSource);
        ConfigurarAudioSource(efectosSource);
    }

    void ConfigurarAudioSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f; // 2D SIEMPRE
        source.volume = 1f;
        source.outputAudioMixerGroup = null; // Sin mixer para evitar problemas
    }

    // ================= MOVIMIENTO =================

    public void StartMovimiento()
    {
        if (movimiento == null) return;

        if (movimientoSource.isPlaying) return;

        movimientoSource.clip = movimiento;
        movimientoSource.loop = true;
        movimientoSource.Play();
    }

    public void StopMovimiento()
    {
        if (movimientoSource.isPlaying)
            movimientoSource.Stop();
    }

    // ================= EFECTOS =================

    public void PlayAtaque()
    {
        if (ataque != null)
            efectosSource.PlayOneShot(ataque);
    }

    public void PlayDaño()
    {
        if (daño != null)
            efectosSource.PlayOneShot(daño);
    }

    public void PlayMuerte()
    {
        if (muerte != null)
            efectosSource.PlayOneShot(muerte);
    }

    public void PlayFatality()
    {
        if (fatality != null)
            efectosSource.PlayOneShot(fatality);
    }
}