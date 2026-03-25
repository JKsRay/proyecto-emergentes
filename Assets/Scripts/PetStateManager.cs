using System;
using UnityEngine;

/// <summary>
/// Manages the virtual pet's internal needs: Hunger, Hygiene, and Happiness.
/// All values are clamped between 0 and 100.
/// </summary>
public class PetStateManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Constants
    // -------------------------------------------------------------------------

    private const float MinValue = 0f;
    private const float MaxValue = 100f;

    // -------------------------------------------------------------------------
    // Serialized fields (visible in the Unity Inspector)
    // -------------------------------------------------------------------------

    [Header("Initial Pet State")]
    [Range(0f, 100f)]
    [SerializeField] private float initialHunger = 50f;

    [Range(0f, 100f)]
    [SerializeField] private float initialHygiene = 80f;

    [Range(0f, 100f)]
    [SerializeField] private float initialHappiness = 70f;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private float _hunger;
    private float _hygiene;
    private float _happiness;

    // -------------------------------------------------------------------------
    // Public properties (read-only access)
    // -------------------------------------------------------------------------

    /// <summary>How hungry the pet is (0 = full, 100 = starving).</summary>
    public float Hunger => _hunger;

    /// <summary>How clean the pet is (0 = dirty, 100 = spotless).</summary>
    public float Hygiene => _hygiene;

    /// <summary>How happy the pet is (0 = miserable, 100 = ecstatic).</summary>
    public float Happiness => _happiness;

    // -------------------------------------------------------------------------
    // Events
    // -------------------------------------------------------------------------

    /// <summary>Fired whenever any stat value changes. Passes the new state.</summary>
    public event Action<PetState> OnStateChanged;

    // -------------------------------------------------------------------------
    // Unity lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        _hunger = Mathf.Clamp(initialHunger, MinValue, MaxValue);
        _hygiene = Mathf.Clamp(initialHygiene, MinValue, MaxValue);
        _happiness = Mathf.Clamp(initialHappiness, MinValue, MaxValue);
    }

    // -------------------------------------------------------------------------
    // Hunger methods
    // -------------------------------------------------------------------------

    /// <summary>Increases the pet's hunger level by <paramref name="amount"/>.</summary>
    public void IncreaseHunger(float amount)
    {
        SetHunger(_hunger + amount);
    }

    /// <summary>Decreases the pet's hunger level by <paramref name="amount"/>.</summary>
    public void DecreaseHunger(float amount)
    {
        SetHunger(_hunger - amount);
    }

    /// <summary>Sets hunger to an explicit value, clamped between 0 and 100.</summary>
    public void SetHunger(float value)
    {
        _hunger = Mathf.Clamp(value, MinValue, MaxValue);
        NotifyStateChanged();
    }

    // -------------------------------------------------------------------------
    // Hygiene methods
    // -------------------------------------------------------------------------

    /// <summary>Increases the pet's hygiene level by <paramref name="amount"/>.</summary>
    public void IncreaseHygiene(float amount)
    {
        SetHygiene(_hygiene + amount);
    }

    /// <summary>Decreases the pet's hygiene level by <paramref name="amount"/>.</summary>
    public void DecreaseHygiene(float amount)
    {
        SetHygiene(_hygiene - amount);
    }

    /// <summary>Sets hygiene to an explicit value, clamped between 0 and 100.</summary>
    public void SetHygiene(float value)
    {
        _hygiene = Mathf.Clamp(value, MinValue, MaxValue);
        NotifyStateChanged();
    }

    // -------------------------------------------------------------------------
    // Happiness methods
    // -------------------------------------------------------------------------

    /// <summary>Increases the pet's happiness level by <paramref name="amount"/>.</summary>
    public void IncreaseHappiness(float amount)
    {
        SetHappiness(_happiness + amount);
    }

    /// <summary>Decreases the pet's happiness level by <paramref name="amount"/>.</summary>
    public void DecreaseHappiness(float amount)
    {
        SetHappiness(_happiness - amount);
    }

    /// <summary>Sets happiness to an explicit value, clamped between 0 and 100.</summary>
    public void SetHappiness(float value)
    {
        _happiness = Mathf.Clamp(value, MinValue, MaxValue);
        NotifyStateChanged();
    }

    // -------------------------------------------------------------------------
    // Utility
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns a snapshot of the pet's current state.
    /// </summary>
    public PetState GetCurrentState()
    {
        return new PetState(_hunger, _hygiene, _happiness);
    }

    /// <summary>
    /// Resets all stats to their initial Inspector-configured values.
    /// </summary>
    public void ResetToDefaults()
    {
        _hunger = Mathf.Clamp(initialHunger, MinValue, MaxValue);
        _hygiene = Mathf.Clamp(initialHygiene, MinValue, MaxValue);
        _happiness = Mathf.Clamp(initialHappiness, MinValue, MaxValue);
        NotifyStateChanged();
    }

    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke(GetCurrentState());
    }
}

// -------------------------------------------------------------------------
// Value object representing a snapshot of the pet's state
// -------------------------------------------------------------------------

/// <summary>
/// Immutable snapshot of the pet's current stat values.
/// </summary>
[Serializable]
public readonly struct PetState
{
    public readonly float Hunger;
    public readonly float Hygiene;
    public readonly float Happiness;

    public PetState(float hunger, float hygiene, float happiness)
    {
        Hunger = hunger;
        Hygiene = hygiene;
        Happiness = happiness;
    }

    public override string ToString() =>
        $"Hunger={Hunger:F1}, Hygiene={Hygiene:F1}, Happiness={Happiness:F1}";
}
