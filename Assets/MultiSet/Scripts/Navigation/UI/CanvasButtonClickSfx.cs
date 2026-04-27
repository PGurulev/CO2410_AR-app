using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
///Playing a click sound effect on Button/Toggles/Dropdowns without adding AudioSource and event listeners to each of them manually.
/// </summary>
[DisallowMultipleComponent]
public class CanvasButtonClickSfx : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [FormerlySerializedAs("changeClip")]
    [SerializeField] private AudioClip clickClip;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    [Header("What to subscribe to")]
    [Tooltip("Subscribe to Button.onClick for all buttons under the search root.")]
    [SerializeField] private bool includeButtons = true;

    [Tooltip("Subscribe to Toggle.onValueChanged for all toggles under the search root.")]
    [SerializeField] private bool includeToggles = true;

    [Tooltip("Subscribe to Dropdown / TMP_Dropdown.onValueChanged for all dropdowns under the search root.")]
    [SerializeField] private bool includeDropdowns = true;

    [Header("Search")]
    [Tooltip("The root transform to start searching for UI elements. If null, the nearest Canvas in the parent hierarchy or the GameObject itself will be used.")]
    [SerializeField] private Transform searchRoot;

    [Tooltip("Periodically re-scan the hierarchy for dynamically added UI elements.")]
    [SerializeField] private bool autoRescan = true;

    [Min(0.05f)]
    [SerializeField] private float rescanInterval = 1f;

    [Header("Debug")]
    [SerializeField] private bool logWarnings = true;

    private readonly HashSet<Button> trackedButtons = new HashSet<Button>();
    private readonly HashSet<Toggle> trackedToggles = new HashSet<Toggle>();
    private readonly HashSet<Dropdown> trackedDropdowns = new HashSet<Dropdown>();
    private readonly HashSet<TMP_Dropdown> trackedTmpDropdowns = new HashSet<TMP_Dropdown>();

    private float nextRescanTime;

    private void Reset()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        ResolveDependencies();
        Refresh();
        nextRescanTime = Time.unscaledTime + rescanInterval;
    }

    private void OnDisable()
    {
        UnsubscribeAll();
    }

    private void Update()
    {
        if (!autoRescan) return;
        if (Time.unscaledTime < nextRescanTime) return;
        nextRescanTime = Time.unscaledTime + rescanInterval;
        Refresh();
    }

    /// <summary>
    /// Refreshes the UI elements and subscribes to all relevant events.
    /// Already subscribed elements are skipped.
    /// </summary>
    public void Refresh()
    {
        var root = ResolveSearchRoot();
        if (root == null)
        {
            if (logWarnings)
            {
                Debug.LogWarning($"{nameof(CanvasButtonClickSfx)}: search root is null.", this);
            }
            return;
        }

        if (includeButtons)
        {
            var buttons = root.GetComponentsInChildren<Button>(true);
            for (int i = 0; i < buttons.Length; i++)
            {
                var b = buttons[i];
                if (b == null) continue;
                if (trackedButtons.Add(b))
                {
                    b.onClick.AddListener(PlayClick);
                }
            }
        }

        if (includeToggles)
        {
            var toggles = root.GetComponentsInChildren<Toggle>(true);
            for (int i = 0; i < toggles.Length; i++)
            {
                var t = toggles[i];
                if (t == null) continue;
                if (trackedToggles.Add(t))
                {
                    t.onValueChanged.AddListener(OnToggleValueChanged);
                }
            }
        }

        if (includeDropdowns)
        {
            var ddl = root.GetComponentsInChildren<Dropdown>(true);
            for (int i = 0; i < ddl.Length; i++)
            {
                var d = ddl[i];
                if (d == null) continue;
                if (trackedDropdowns.Add(d))
                {
                    d.onValueChanged.AddListener(OnIntValueChanged);
                }
            }

            var tmpDdl = root.GetComponentsInChildren<TMP_Dropdown>(true);
            for (int i = 0; i < tmpDdl.Length; i++)
            {
                var d = tmpDdl[i];
                if (d == null) continue;
                if (trackedTmpDropdowns.Add(d))
                {
                    d.onValueChanged.AddListener(OnIntValueChanged);
                }
            }
        }
    } 
    public void PlayClick()
    {
        if (clickClip == null)
        {
            if (logWarnings)
            {
                Debug.LogWarning($"{nameof(CanvasButtonClickSfx)}: clickClip is not assigned.", this);
            }
            return;
        }

        if (audioSource != null)
        {
            audioSource.PlayOneShot(clickClip, volume);
        }
        else
        {
            AudioSource.PlayClipAtPoint(clickClip, transform.position, volume);
        }
    }

    private void OnToggleValueChanged(bool _)
    {
        PlayClick();
    }

    private void OnIntValueChanged(int _)
    {
        PlayClick();
    }

    private void UnsubscribeAll()
    {
        foreach (var b in trackedButtons)
        {
            if (b != null) b.onClick.RemoveListener(PlayClick);
        }
        trackedButtons.Clear();

        foreach (var t in trackedToggles)
        {
            if (t != null) t.onValueChanged.RemoveListener(OnToggleValueChanged);
        }
        trackedToggles.Clear();

        foreach (var d in trackedDropdowns)
        {
            if (d != null) d.onValueChanged.RemoveListener(OnIntValueChanged);
        }
        trackedDropdowns.Clear();

        foreach (var d in trackedTmpDropdowns)
        {
            if (d != null) d.onValueChanged.RemoveListener(OnIntValueChanged);
        }
        trackedTmpDropdowns.Clear();
    }

    private void ResolveDependencies()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    private Transform ResolveSearchRoot()
    {
        if (searchRoot != null) return searchRoot;

        var canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            // taken root canvas to support world space canvases with nested structure
            //for screen space canvases this will just return the canvas transform as before
            return canvas.rootCanvas != null ? canvas.rootCanvas.transform : canvas.transform;
        }

        return transform;
    }
}
