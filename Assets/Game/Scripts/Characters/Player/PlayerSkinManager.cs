using UnityEngine;

public class PlayerSkinManager : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private RuntimeAnimatorController[] skins;

    [Header("Configuración")]
    [SerializeField] private int defaultSkinIndex = 0;

    private const string SKIN_KEY = "SkinSeleccionada";

    private void Start()
    {
        if (skins == null || skins.Length == 0)
        {
            Debug.LogWarning("No hay skins asignadas.");
            return;
        }

        int skinGuardada;

        // Si no existe clave guardada, usar la default del inspector
        if (!PlayerPrefs.HasKey(SKIN_KEY))
        {
            skinGuardada = Mathf.Clamp(defaultSkinIndex, 1, skins.Length - 1);
            PlayerPrefs.SetInt(SKIN_KEY, skinGuardada);
            PlayerPrefs.Save();
        }
        else
        {
            skinGuardada = PlayerPrefs.GetInt(SKIN_KEY);

            // Seguridad por si el índice guardado es inválido
            if (skinGuardada < 0 || skinGuardada >= skins.Length)
            {
                skinGuardada = Mathf.Clamp(defaultSkinIndex, 0, skins.Length - 1);
            }
        }

        CambiarSkin(skinGuardada);
    }

    public void CambiarSkin(int index)
    {
        if (index < 0 || index >= skins.Length) return;
        if (animator == null) return;

        animator.runtimeAnimatorController = skins[index];

        PlayerPrefs.SetInt(SKIN_KEY, index);
        PlayerPrefs.Save();
    }
}