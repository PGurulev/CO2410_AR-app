using System;
using UnityEngine;
using TMPro;
using MultiSet;

/**
 * Handles the navigation UI state and input.
 */
[DefaultExecutionOrder(-150)]
public class NavigationUIController : MonoBehaviour
{
    public static NavigationUIController instance;

    /// <summary>Fired after <see cref="DestinationSelectUI"/> active state changes (argument is the new value).</summary>
    public event Action<bool> OnDestinationSelectVisibilityChanged;

    [Tooltip("Label to show remaining distance")]
    public TextMeshProUGUI remainingDistance;

    [Tooltip("Button to stop navigation")]
    public GameObject stopButton;

    [Tooltip("SelectList where POIs are shown")]
    public SelectList poiList;

    [Tooltip("Parent GameObject of POIs selection UI")]
    public GameObject DestinationSelectUI;

    [Tooltip("Parent GameObject of accessibility sheet (same pattern as Destination Select)")]
    public GameObject AccessibilitySelectUI;

    [Tooltip("Label to show name of current destination")]
    public TextMeshProUGUI destinationName;

    [Tooltip("Parent GameObject of navigation progress slider")]
    public GameObject navigationProgressSlider;

    [Tooltip("Navigation Path Material")]
    public Material material;

    void Awake()
    {
        instance = this;
        // Accessibility FAB on the root canvas before any Start (localization may add full-screen UI in Start).
        if (AccessibilitySelectUI != null)
        {
            var acc = AccessibilitySelectUI.GetComponent<AccessibilitySelectUIController>();
            acc?.EnsureOpenFab();
        }
    }

    void Start()
    {
        if (AccessibilitySelectUI != null)
        {
            var acc = AccessibilitySelectUI.GetComponent<AccessibilitySelectUIController>();
            acc?.EnsureOpenFab();
        }

        if (navigationProgressSlider != null && stopButton != null)
        {
            ShowNavigationUIElements(false);
        }

        if (DestinationSelectUI != null)
        {
            DestinationSelectUI.SetActive(false);
        }

        OnDestinationSelectVisibilityChanged?.Invoke(false);

        if (AccessibilitySelectUI != null)
        {
            AccessibilitySelectUI.SetActive(false);
        }

        if (destinationName != null)
        {
            destinationName.text = "";
        }
    }

    void Update()
    {
        HandleNavigationState();
        UpdateRemainingDistance();
    }

    // handles the 
    void HandleNavigationState()
    {
        if (destinationName == null)
        {
            return;
        }

        if (NavigationController.instance.IsCurrentlyNavigating())
        {
            destinationName.text = NavigationController.instance.currentDestination.poiName;
            return;
        }

        destinationName.text = "";
    }

    /**
     * Toggles visibility of destination select UI.
     */
    public void ToggleDestinationSelectUI()
    {
        bool show = !DestinationSelectUI.activeSelf;
        if (show && AccessibilitySelectUI != null && AccessibilitySelectUI.activeSelf)
        {
            AccessibilitySelectUI.SetActive(false);
        }

        DestinationSelectUI.SetActive(show);
        OnDestinationSelectVisibilityChanged?.Invoke(show);

        if (!show)
        {
            poiList.ResetPOISearch();
            return;
        }

        poiList.RenderPOIs();
    }

    /// <summary>Toggles the accessibility sheet; closes Destination Select if it is open.</summary>
    public void ToggleAccessibilitySelectUI()
    {
        if (AccessibilitySelectUI == null)
        {
            return;
        }

        bool show = !AccessibilitySelectUI.activeSelf;
        if (show && DestinationSelectUI != null && DestinationSelectUI.activeSelf)
        {
            DestinationSelectUI.SetActive(false);
            OnDestinationSelectVisibilityChanged?.Invoke(false);
            poiList.ResetPOISearch();
        }

        AccessibilitySelectUI.SetActive(show);
    }

    /// <summary>Closes the accessibility sheet without toggling.</summary>
    public void CloseAccessibilitySelectUI()
    {
        if (AccessibilitySelectUI != null && AccessibilitySelectUI.activeSelf)
        {
            AccessibilitySelectUI.SetActive(false);
        }
    }

    public void ResetPoiSearch()
    {
        poiList.ResetPOISearch();
    }

    public void RenderPoiCall()
    {
        poiList.RenderPOIs();
    }

    // User clicked to start navigation. Is called from ListItemUI.cs
    public void ClickedStartNavigation(POI poi)
    {
        NavigationController.instance.SetPOIForNavigation(poi);
        ToggleDestinationSelectUI();

        ShowNavigationUIElements(true);
    }

    // User clicked to stop navigation
    public void ClickedStopButton()
    {
        ShowNavigationUIElements(false);
        NavigationController.instance.StopNavigation();
    }

    // toggle visibility of navigation UI elements
    void ShowNavigationUIElements(bool isVisible)
    {
        if (navigationProgressSlider != null)
        {
            navigationProgressSlider.SetActive(isVisible);
        }

        if (stopButton != null)
        {
            stopButton.SetActive(isVisible);
        }
    }

    // Update info about remaining distance.
    void UpdateRemainingDistance()
    {
        if (remainingDistance == null)
        {
            return;
        }

        if (!NavigationController.instance.IsCurrentlyNavigating())
        {
            remainingDistance.SetText("");
            return;
        }

        int distance = PathEstimationUtils.instance.getRemainingDistanceMeters();
        string distanceText = distance + "";

        if (distance > 1)
        {
            if (material != null)
                material.SetFloat("_PathLength", distance);
        }
        if (distance <= 1)
        {
            distanceText += " m remaining";
        }
        else
        {
            distanceText += " m remaining";
        }
        remainingDistance.text = distanceText;
    }

    // Show arrival state, is called from NavigationController.cs
    public void ShowArrivedState()
    {
        ShowNavigationUIElements(false);
        ToastManager.Instance.ShowAlert("You arrived at the destination!");
    }
}
