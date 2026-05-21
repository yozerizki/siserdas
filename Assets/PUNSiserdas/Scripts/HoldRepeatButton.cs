using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class HoldRepeatButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField] private float holdDelay = .6f;
    [SerializeField] private float repeatInterval = 0.6f;
    [SerializeField] private bool useDynamicHoldSteps;
    [SerializeField] private bool triggerTickOnPressInDynamicMode = true;
    [SerializeField] private float step1StartSeconds = 1f;
    [SerializeField] private float step2StartSeconds = 2f;
    [SerializeField] private float step3StartSeconds = 3f;
    [SerializeField] private float step4StartSeconds = 4f;
    [SerializeField] private int step1TicksPerFrame = 1;
    [SerializeField] private int step2TicksPerFrame = 10;
    [SerializeField] private int step3TicksPerFrame = 50;
    [SerializeField] private int step4TicksPerFrame = 100;
    [SerializeField] private int maxTicksPerFrame = 200;
    [SerializeField] private UnityEvent onPressStart;
    [SerializeField] private UnityEvent onPressEnd;
    [SerializeField] private UnityEvent onRepeatTick;

    private UnityAction runtimePressStart;
    private UnityAction runtimePressEnd;

    private bool isHolding;
    private float holdTimer;
    private float repeatTimer;

    private void Update()
    {
        if (!isHolding)
            return;

        holdTimer += Time.unscaledDeltaTime;

        if (useDynamicHoldSteps)
        {
            int ticksThisFrame = GetDynamicTicks(holdTimer);
            if (ticksThisFrame <= 0)
                return;

            ticksThisFrame = Mathf.Clamp(ticksThisFrame, 1, Mathf.Max(1, maxTicksPerFrame));
            for (int i = 0; i < ticksThisFrame; i++)
                onRepeatTick?.Invoke();

            return;
        }

        if (holdTimer < holdDelay)
            return;

        repeatTimer += Time.unscaledDeltaTime;
        if (repeatTimer >= repeatInterval)
        {
            repeatTimer = 0f;
            onRepeatTick?.Invoke();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isHolding = true;
        holdTimer = 0f;
        repeatTimer = 0f;
        onPressStart?.Invoke();
        runtimePressStart?.Invoke();

        if (useDynamicHoldSteps && triggerTickOnPressInDynamicMode)
            onRepeatTick?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetState();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetState();
    }

    private void ResetState()
    {
        if (isHolding)
        {
            onPressEnd?.Invoke();
            runtimePressEnd?.Invoke();
        }

        isHolding = false;
        holdTimer = 0f;
        repeatTimer = 0f;
    }

    public void SetRuntimePressCallbacks(UnityAction onStart, UnityAction onEnd)
    {
        runtimePressStart = onStart;
        runtimePressEnd = onEnd;
    }

    private int GetDynamicTicks(float heldSeconds)
    {
        if (heldSeconds < step1StartSeconds)
            return 0;

        if (heldSeconds >= step4StartSeconds)
            return step4TicksPerFrame;

        if (heldSeconds >= step3StartSeconds)
            return step3TicksPerFrame;

        if (heldSeconds >= step2StartSeconds)
            return step2TicksPerFrame;

        return step1TicksPerFrame;
    }
}
