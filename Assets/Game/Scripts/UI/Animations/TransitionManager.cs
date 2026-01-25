using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System; // <--- IMPORTANTE: Necesario para usar Action

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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else { Destroy(gameObject); }
    }

    // --- MÉTODO ORIGINAL (Para cambiar de ESCENA) ---
    public void LoadScene(string sceneName, TransitionType transitionType)
    {
        StartCoroutine(ProcessSceneTransition(sceneName, transitionType));
    }

    // --- NUEVO MÉTODO (Para cambios DENTRO del menú) ---
    // En lugar de una escena, recibe una "Action" (lo que quieres que pase en medio)
    public void LocalTransition(TransitionType type, Action midTransitionAction)
    {
        StartCoroutine(ProcessLocalTransition(type, midTransitionAction));
    }

    // Corrutina para cambio de ESCENA
    private IEnumerator ProcessSceneTransition(string sceneName, TransitionType type)
    {
        Animator currentAnimator = GetAnimator(type);
        if (currentAnimator != null)
        {
            currentAnimator.gameObject.SetActive(true);
            currentAnimator.SetTrigger("StartTransition");
        }

        yield return new WaitForSeconds(transitionTime);

        yield return SceneManager.LoadSceneAsync(sceneName); // Carga escena

        if (currentAnimator != null)
        {
            currentAnimator.SetTrigger("EndTransition");
            yield return new WaitForSeconds(transitionTime);
            currentAnimator.gameObject.SetActive(false);
        }
    }

    // Corrutina para cambio LOCAL (NUEVA)
    private IEnumerator ProcessLocalTransition(TransitionType type, Action action)
    {
        // 1. Pantalla a Negro
        Animator currentAnimator = GetAnimator(type);
        if (currentAnimator != null)
        {
            currentAnimator.gameObject.SetActive(true);
            currentAnimator.SetTrigger("StartTransition");
        }

        // 2. Esperar
        yield return new WaitForSeconds(transitionTime);

        // 3. EJECUTAR TU LÓGICA DE MENÚS (Aquí ocurre la magia)
        // Esto ejecutará OpenStoryMenu() mientras la pantalla está negra
        if (action != null) action.Invoke();

        // 4. Pantalla a Transparente
        if (currentAnimator != null)
        {
            currentAnimator.SetTrigger("EndTransition");
            yield return new WaitForSeconds(transitionTime);
            currentAnimator.gameObject.SetActive(false);
        }
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