using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public enum TransitionType
{
    Fade,
    CircleExpand,
    ImageTranslation
}

public class TransitionManager : MonoBehaviour
{
    public static TransitionManager Instance;

    [Header("Referencias")]
    public Animator fadeAnimator;
    public Animator circleAnimator;
    public Animator slideAnimator;

    [Header("Configuración")]
    public float transitionTime = 1f;

    [Header("Opcional")]
    public AudioSource transitionSound; // 🔥 NUEVO (opcional)
    public bool debugMode = false;      // 🔥 NUEVO

    private bool isTransitioning = false; // 🔥 NUEVO (evita doble transición)

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }
    }

    // -------------------------------
    // CAMBIO DE ESCENA
    // -------------------------------
    public void LoadScene(string sceneName, TransitionType transitionType)
    {
        if (!isTransitioning)
            StartCoroutine(ProcessSceneTransition(sceneName, transitionType));
    }

    // -------------------------------
    // CAMBIO LOCAL
    // -------------------------------
    public void LocalTransition(TransitionType type, Action midTransitionAction)
    {
        if (!isTransitioning)
            StartCoroutine(ProcessLocalTransition(type, midTransitionAction));
    }

    private IEnumerator ProcessSceneTransition(string sceneName, TransitionType type)
    {
        isTransitioning = true;

        Animator currentAnimator = GetAnimator(type);
        PlayTransition(currentAnimator);

        yield return new WaitForSeconds(transitionTime);

        yield return SceneManager.LoadSceneAsync(sceneName);

        EndTransition(currentAnimator);

        isTransitioning = false;
    }

    private IEnumerator ProcessLocalTransition(TransitionType type, Action action)
    {
        isTransitioning = true;

        Animator currentAnimator = GetAnimator(type);
        PlayTransition(currentAnimator);

        yield return new WaitForSeconds(transitionTime);

        if (action != null)
            action.Invoke();

        EndTransition(currentAnimator);

        isTransitioning = false;
    }

    // -------------------------------
    // MÉTODOS AUXILIARES (NUEVOS)
    // -------------------------------

    private void PlayTransition(Animator animator)
    {
        if (animator == null)
        {
            if (debugMode)
                Debug.LogWarning("No Animator asignado para esta transición.");
            return;
        }

        animator.gameObject.SetActive(true);
        animator.SetTrigger("StartTransition");

        if (transitionSound != null)
            transitionSound.Play();
    }

    private void EndTransition(Animator animator)
    {
        if (animator == null) return;

        animator.SetTrigger("EndTransition");
        StartCoroutine(DisableAfterDelay(animator));
    }

    private IEnumerator DisableAfterDelay(Animator animator)
    {
        yield return new WaitForSeconds(transitionTime);
        animator.gameObject.SetActive(false);
    }

    private Animator GetAnimator(TransitionType type)
    {
        if (fadeAnimator) fadeAnimator.gameObject.SetActive(false);
        if (circleAnimator) circleAnimator.gameObject.SetActive(false);
        if (slideAnimator) slideAnimator.gameObject.SetActive(false);

        switch (type)
        {
            case TransitionType.Fade: return fadeAnimator;
            case TransitionType.CircleExpand: return circleAnimator;
            case TransitionType.ImageTranslation: return slideAnimator;
            default: return null;
        }
    }
}