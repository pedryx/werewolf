﻿using UnityEngine;
using UnityEngine.Events;

public class GameManager : Singleton<GameManager>
{
    [Header("UI")]
    [SerializeField]
    private TextPromptUI contextPromptUI;
    /// <summary>
    /// Bounds of the path finding grid.
    /// </summary>
    [Header("PathFinder")]
    [SerializeField]
    [Tooltip("Bounds of the path finding grid.")]
    private Rect pathFinderBounds;
    /// <summary>
    /// Size of cells in the path finding grid.
    /// </summary>
    [SerializeField]
    [Tooltip("Size of cells in path finding grid.")]
    private float pathFinderCellSize = 0.2f;
    /// <summary>
    /// Radius used for physics overlap checks during creation of the path finding grid.
    /// </summary>
    [SerializeField]
    [Tooltip("Radius used for physics overlap checks during creation of the path finding grid.")]
    private float pathFinderRadius = 0.1f;

    /// <summary>
    /// Determine if day is active.
    /// </summary>
    private bool isDay = true;
    /// <summary>
    /// Time elapsed since start of the night.
    /// </summary>
    private float nightTimeElapsed = 0.0f;

    /// <summary>
    /// Determine if the path finding grid should be visualized.
    /// </summary>
    [field: SerializeField]
    [Tooltip("Determine if path finding grid should be visualized.")]
    public bool ShowPathFindingGrid { get; set; } = false;
    /// <summary>
    /// Determine if current patches should be visualized.
    /// </summary>
    [field: SerializeField]
    [Tooltip("Determine if current patches should be visualized.")]
    public bool ShowPathFindingPatches { get; set; } = false;

    /// <summary>
    /// Night duration in seconds.
    /// </summary>
    [Header("Day/Night cycle")]
    [field: SerializeField]
    [Tooltip("Night duration in seconds.")]
    public float NightDuration { get; private set; } = 5.0f * 60.0f;

    /// <summary>
    /// Determine if day is active.
    /// </summary>
    public bool IsDay => isDay;

    /// <summary>
    /// Determine if night is active.
    /// </summary>
    public bool IsNight => !isDay;

    public int DayNumber { get; private set; } = 1;

    public PathFinder PathFinder { get; private set; }

    public UnityEvent OnDayBegin = new();
    public UnityEvent OnNightBegin = new();
    public UnityEvent OnDayNightSwitch = new();

    protected override void Awake()
    {
        base.Awake();

        OnDayBegin.AddListener(GameManager_OnDayBegin);
        OnNightBegin.AddListener(GameManager_OnNightBegin);
        QuestManager.Instance.Current.TransitionQuest.OnDone.AddListener(DayEndQuest_OnDone);
        WerewolfController.Instance.OnPlayerCaught.AddListener(Werewolf_OnPlayerCaught);

        PathFinder = new PathFinder(pathFinderBounds, pathFinderCellSize, pathFinderRadius);
    }

    private void Update()
    {
        if (isDay)
            return;

        nightTimeElapsed += Time.deltaTime;

        if (nightTimeElapsed >= NightDuration)
        {
            isDay = true;
            OnDayBegin?.Invoke();
        }
    }

    private void OnDrawGizmos()
    {
        if (PathFinder == null || !ShowPathFindingGrid)
            return;

        PathFinder.DrawGizmos();
    }

    public void ShowContextPrompt(string promptText) => contextPromptUI.Show(promptText);

    public void HideContextPrompt() => contextPromptUI.Hide();

    /// <summary>
    /// Toggle between day and night.
    /// </summary>
    /// <returns>True if switched to day otherwise false.</returns>
    public bool SwitchDayNight()
    {
        isDay = !isDay;

        if (isDay)
            OnDayBegin.Invoke();
        else
            OnNightBegin.Invoke();

        return isDay;
    }

    private void GameManager_OnDayBegin()
    {
        Debug.Log("Day started.");
        DayNumber++;

        OnDayNightSwitch.Invoke();
    }

    private void GameManager_OnNightBegin()
    {
        Debug.Log("Night started.");
        nightTimeElapsed = 0.0f;

        OnDayNightSwitch.Invoke();
    }

    private void Werewolf_OnPlayerCaught()
    {
        isDay = true;
        OnDayBegin.Invoke();
    }

    private void DayEndQuest_OnDone(QuestEventArgs e)
    {
        isDay = false;
        OnNightBegin.Invoke();
    }
}