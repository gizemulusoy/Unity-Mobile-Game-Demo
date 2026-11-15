using UnityEngine;

public class WinBackdropSync : MonoBehaviour
{
    [SerializeField] private GameObject backdrop;

    private void OnEnable()
    {
        if (backdrop != null)
            backdrop.SetActive(true);
    }

    private void OnDisable()
    {
        if (backdrop != null)
            backdrop.SetActive(false);
    }
}