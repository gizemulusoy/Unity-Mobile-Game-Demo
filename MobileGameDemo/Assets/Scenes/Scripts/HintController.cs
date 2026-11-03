using UnityEngine;

public class HintController : MonoBehaviour
{
    public GridManager gridManager;
    public GameObject[] blockingPanels;
    public float idleDelay = 5f;
    public float blinkInterval = 0.5f;

    public float nudgeAmount = 0.12f;
    public float nudgeDuration = 1.2f;

    private Vector3 firstStartPosition;
    private Vector3 secondStartPosition;
    private float nudgeTimer;
    private int nudgeCount;

    private float idleTimer;
    private float blinkTimer;
    private Tile first;
    private Tile second;
    private bool showingHint;
    private bool lightVisible;

    private void Update()
    {
        if (gridManager == null)
            return;

        if (gridManager.IsBusy || IsPanelOpen())
        {
            ResetHint();
            return;
        }

        if (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            ResetHint();
            return;
        }

        if (showingHint)
        {
            AnimateHint();

            blinkTimer += Time.deltaTime;

            if (blinkTimer >= blinkInterval)
            {
                blinkTimer = 0f;
                lightVisible = !lightVisible;
                SetLights(lightVisible);
            }

            return;
        }

        idleTimer += Time.deltaTime;

        if (idleTimer >= idleDelay)
        {
            idleTimer = 0f;

            if (gridManager.TryFindHintSwap(out first, out second))
            {
                firstStartPosition = first.transform.position;
                secondStartPosition = second.transform.position;
                nudgeTimer = 0f;

                showingHint = true;
                lightVisible = true;
                blinkTimer = 0f;
                SetLights(true);
            }
        }
    }

    private bool IsPanelOpen()
    {
        if (blockingPanels == null)
            return false;

        foreach (GameObject panel in blockingPanels)
        {
            if (panel != null && panel.activeInHierarchy)
                return true;
        }

        return false;
    }

    private void AnimateHint()
    {
        if (first == null || second == null)
        {
            ResetHint();
            return;
        }

        nudgeTimer += Time.deltaTime;

        float duration = Mathf.Max(0.1f, nudgeDuration);
        float phase = Mathf.Clamp01(nudgeTimer / duration);
        float movement = Mathf.Sin(phase * Mathf.PI);
        float amount = Mathf.Clamp(nudgeAmount, 0f, 0.25f);

        Vector3 offset =
            (secondStartPosition - firstStartPosition) * amount * movement;

        first.transform.position = firstStartPosition + offset;
        second.transform.position = secondStartPosition - offset;

        if (nudgeTimer >= duration)
        {
            nudgeTimer = 0f;
            nudgeCount++;

            if (nudgeCount >= 3)
                ResetHint();
        }
    }

    private void SetLights(bool visible)
    {
        if (first != null)
            first.SetHintVisible(visible);

        if (second != null)
            second.SetHintVisible(visible);
    }

    private void ResetHint()
    {
        SetLights(false);

        if (showingHint && gridManager != null && !gridManager.IsBusy)
        {
            if (first != null)
                first.transform.position = firstStartPosition;

            if (second != null)
                second.transform.position = secondStartPosition;
        }

        first = null;
        second = null;
        showingHint = false;
        lightVisible = false;
        idleTimer = 0f;
        blinkTimer = 0f;
        nudgeTimer = 0f;
        nudgeCount = 0;
    }

    private void OnDisable()
    {
        ResetHint();
    }
}