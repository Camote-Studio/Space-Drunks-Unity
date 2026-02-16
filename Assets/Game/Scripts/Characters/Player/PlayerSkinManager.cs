using UnityEngine;

public class PlayerSkinManager : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private RuntimeAnimatorController[] skins;

    private void Start()
    {
        int skinGuardada = PlayerPrefs.GetInt("SkinSeleccionada", 0);
        ActivarSkin(skinGuardada);
    }

    public void ActivarSkin(int index)
    {
        if (index < 0 || index >= skins.Length) return;

        animator.runtimeAnimatorController = skins[index];

        PlayerPrefs.SetInt("SkinSeleccionada", index);
        PlayerPrefs.Save();
    }
}
