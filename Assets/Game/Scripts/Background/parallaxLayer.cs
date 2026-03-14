using UnityEngine;

public class parallaxLayer : MonoBehaviour
{
    public Transform cam;
    public float parallaxEffect = 0.2f;

    private Vector3 lastCamPos;

    void Start()
    {
        lastCamPos = cam.position;
    }

    void LateUpdate()
    {
        Vector3 delta = cam.position - lastCamPos;

        transform.position += new Vector3(
            delta.x * parallaxEffect,
            delta.y * parallaxEffect,
            0);

        lastCamPos = cam.position;
    }
}