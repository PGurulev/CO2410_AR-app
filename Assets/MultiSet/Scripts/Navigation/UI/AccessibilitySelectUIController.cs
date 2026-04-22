using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Builds and wires the accessibility sheet under <see cref="NavigationUI"/> (same pattern as Destination Select):
/// dim root, white Box, header + Close, Scroll View with Viewport and toggles.
/// </summary>
[DefaultExecutionOrder(-200)]
[DisallowMultipleComponent]
public sealed class AccessibilitySelectUIController : MonoBehaviour
{
    [Header("Sprites (assign in inspector; optional fallbacks are null)")]
    [SerializeField] private Sprite closeButtonSprite;
    [SerializeField] private Sprite boxPanelSprite;

    [Header("Theme")]
    [SerializeField] private Color dimColor = new Color(0.14150941f, 0.14150941f, 0.14150941f, 0.36078432f);
    [SerializeField] private Color boxColor = new Color(0.9245283f, 0.9245283f, 0.9245283f, 0.9411765f);
    [SerializeField] private Color titleColor = new Color(1f, 0.4117647f, 0f, 1f);
    [SerializeField] private Color subtleTextColor = new Color(0.29f, 0.33f, 0.39f, 1f);
    [SerializeField] private Color dividerColor = new Color(0.12f, 0.14f, 0.18f, 0.10f);

    private const string BuiltMarker = "Box";
    private const string OpenFabName = "AccessibilityOpenFab";
    private const string FabLayerName = "AccessibilityFabLayer";
    private const float FabSize = 200f;

    private static Sprite _fabCircleSprite;

    private static Sprite GetFabCircleSprite()
    {
        if (_fabCircleSprite != null)
        {
            return _fabCircleSprite;
        }

        const int res = 128;
        var tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        var c = new Vector2(res * 0.5f, res * 0.5f);
        var r = res * 0.5f - 1f;
        for (var y = 0; y < res; y++)
        {
            for (var x = 0; x < res; x++)
            {
                var d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), c);
                tex.SetPixel(x, y, d <= r ? Color.white : Color.clear);
            }
        }

        tex.Apply();
        _fabCircleSprite = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f), 100f);
        return _fabCircleSprite;
    }

    private Toggle _bigTextToggle;
    private Toggle _contrastToggle;
    private AccessibilityManager _manager;
    private bool _wiredManager;

    private void Awake()
    {
        StretchToParentNavigationUi();
        BuildIfNeeded();
        // FAB must exist before other components' Start (e.g. MultiSet localization overlays the canvas).
        EnsureOpenFabImpl();
    }

    private void Start()
    {
        WireManager();
        EnsureOpenFabImpl();
    }

    private void OnEnable()
    {
        BuildIfNeeded();
        WireManager();
        EnsureOpenFabImpl();
    }

    private void OnDisable()
    {
        _wiredManager = false;
    }

    /// <summary>
    /// Creates the &quot;Aa&quot; FAB on the root canvas. Call this from an active object (e.g. <see cref="NavigationUIController"/> in <c>Start</c>),
    /// because Unity does not run <c>Start</c>/<c>OnEnable</c> on inactive GameObjects.
    /// </summary>
    public void EnsureOpenFab()
    {
        EnsureOpenFabImpl();
    }

    private void StretchToParentNavigationUi()
    {
        var rt = transform as RectTransform;
        if (rt == null)
        {
            return;
        }

        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.sizeDelta = new Vector2(8f, 8f);
        rt.anchoredPosition = Vector2.zero;
    }

    private void BuildIfNeeded()
    {
        if (transform.Find(BuiltMarker) != null)
        {
            RefreshToggleReferencesFromBuiltBox();
            return;
        }

        var rootRt = transform as RectTransform;
        if (rootRt == null)
        {
            return;
        }

        var dim = GetComponent<Image>();
        if (dim == null)
        {
            dim = gameObject.AddComponent<Image>();
        }

        dim.color = dimColor;
        dim.raycastTarget = true;
        dim.sprite = null;
        dim.type = Image.Type.Simple;

        var box = new GameObject(BuiltMarker, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var boxRt = box.GetComponent<RectTransform>();
        boxRt.SetParent(rootRt, false);
        boxRt.anchorMin = new Vector2(0f, 0f);
        boxRt.anchorMax = new Vector2(1f, 0f);
        boxRt.pivot = new Vector2(0.5f, 0.5f);
        boxRt.anchoredPosition = new Vector2(0f, 560f);
        boxRt.sizeDelta = new Vector2(-56f, 1024f);
        var boxImg = box.GetComponent<Image>();
        boxImg.color = boxColor;
        boxImg.sprite = boxPanelSprite;
        boxImg.type = boxPanelSprite != null ? Image.Type.Sliced : Image.Type.Simple;
        boxImg.pixelsPerUnitMultiplier = 3f;
        boxImg.raycastTarget = true;

        BuildHeader(boxRt);
        BuildScrollWithToggles(boxRt);
        // Toggles are assigned inside BuildScrollWithToggles (under Content/ToggleRow/). Do not overwrite with a wrong path.
    }

    private void RefreshToggleReferencesFromBuiltBox()
    {
        var box = transform.Find(BuiltMarker);
        if (box == null)
        {
            return;
        }

        _bigTextToggle = box.Find("Scroll View/Viewport/Content/ToggleRow/BigTextT")?.GetComponent<Toggle>();
        _contrastToggle = box.Find("Scroll View/Viewport/Content/ToggleRow/ContrastT")?.GetComponent<Toggle>();
    }

    private void BuildHeader(RectTransform boxRt)
    {
        var header = new GameObject("Header", typeof(RectTransform));
        var headerRt = header.GetComponent<RectTransform>();
        headerRt.SetParent(boxRt, false);
        headerRt.anchorMin = new Vector2(0f, 1f);
        headerRt.anchorMax = new Vector2(1f, 1f);
        headerRt.pivot = new Vector2(0.5f, 0.5f);
        headerRt.anchoredPosition = new Vector2(0f, -56f);
        headerRt.sizeDelta = new Vector2(0f, 112f);

        CreateTmpText(headerRt, "Title", "Accessibility", 52f, TextAlignmentOptions.Left, new Vector2(16f, -12f), new Vector2(-32f, 28f), titleColor, true);

        var closeGo = new GameObject("CloseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        var closeRt = closeGo.GetComponent<RectTransform>();
        closeRt.SetParent(headerRt, false);
        closeRt.anchorMin = new Vector2(1f, 0.5f);
        closeRt.anchorMax = new Vector2(1f, 0.5f);
        closeRt.pivot = new Vector2(0.5f, 0.5f);
        closeRt.anchoredPosition = new Vector2(-76f, 0f);
        closeRt.sizeDelta = new Vector2(66f, 66f);
        var closeImg = closeGo.GetComponent<Image>();
        closeImg.sprite = closeButtonSprite;
        closeImg.color = new Color(0.1509434f, 0.1509434f, 0.1509434f, 1f);
        closeImg.raycastTarget = true;
        var closeBtn = closeGo.GetComponent<Button>();
        closeBtn.targetGraphic = closeImg;
        closeBtn.transition = Selectable.Transition.ColorTint;
        closeBtn.onClick.AddListener(ClosePanel);
    }

    private void BuildScrollWithToggles(RectTransform boxRt)
    {
        var scrollGo = new GameObject("Scroll View", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(ScrollRect));
        var scrollRt = scrollGo.GetComponent<RectTransform>();
        scrollRt.SetParent(boxRt, false);
        scrollRt.anchorMin = Vector2.zero;
        scrollRt.anchorMax = Vector2.one;
        scrollRt.offsetMin = new Vector2(16f, 24f);
        scrollRt.offsetMax = new Vector2(-16f, -128f);
        scrollGo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.02f);
        scrollGo.GetComponent<Image>().raycastTarget = true;

        var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(RectMask2D));
        var viewportRt = viewportGo.GetComponent<RectTransform>();
        viewportRt.SetParent(scrollRt, false);
        StretchFull(viewportRt);
        var vpImg = viewportGo.GetComponent<Image>();
        vpImg.color = new Color(1f, 1f, 1f, 0.02f);
        vpImg.raycastTarget = true;

        var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        var contentRt = contentGo.GetComponent<RectTransform>();
        contentRt.SetParent(viewportRt, false);
        contentRt.anchorMin = new Vector2(0f, 1f);
        contentRt.anchorMax = new Vector2(1f, 1f);
        contentRt.pivot = new Vector2(0.5f, 1f);
        contentRt.anchoredPosition = Vector2.zero;
        contentRt.sizeDelta = new Vector2(0f, 200f);
        var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.spacing = 12f;
        vlg.padding = new RectOffset(8, 8, 8, 8);
        var fitter = contentGo.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        var div = new GameObject("Divider", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var divRt = div.GetComponent<RectTransform>();
        divRt.SetParent(contentRt, false);
        divRt.sizeDelta = new Vector2(0f, 1f);
        var leDiv = div.AddComponent<LayoutElement>();
        leDiv.minHeight = 1f;
        leDiv.preferredHeight = 1f;
        div.GetComponent<Image>().color = dividerColor;

        CreateTmpText(contentRt, "Subtitle", "Display", 15f, TextAlignmentOptions.Left, Vector2.zero, new Vector2(0f, 22f), subtleTextColor, false);

        var row = new GameObject("ToggleRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        var rowRt = row.GetComponent<RectTransform>();
        rowRt.SetParent(contentRt, false);
        var rowLe = row.AddComponent<LayoutElement>();
        rowLe.minHeight = 96f;
        rowLe.preferredHeight = 96f;
        var h = row.GetComponent<HorizontalLayoutGroup>();
        h.childAlignment = TextAnchor.MiddleLeft;
        h.childControlHeight = true;
        h.childControlWidth = true;
        h.childForceExpandWidth = true;
        h.childForceExpandHeight = true;
        h.spacing = 8f;
        h.padding = new RectOffset(4, 4, 4, 4);

        _bigTextToggle = CreateToggle(rowRt, "BigTextT", "Big text", isLeftColumn: true);
        _contrastToggle = CreateToggle(rowRt, "ContrastT", "High contrast", isLeftColumn: false);

        var scroll = scrollGo.GetComponent<ScrollRect>();
        scroll.viewport = viewportRt;
        scroll.content = contentRt;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 35f;
    }

    private void WireManager()
    {
        if (_wiredManager || _bigTextToggle == null || _contrastToggle == null)
        {
            return;
        }

        _manager = GetComponent<AccessibilityManager>();
        if (_manager == null)
        {
            _manager = gameObject.AddComponent<AccessibilityManager>();
        }

        var canvas = ResolveTargetCanvas();
        if (!_manager.Bind(_bigTextToggle, _contrastToggle, canvas))
        {
            return;
        }

        _wiredManager = true;
    }

    private Canvas ResolveTargetCanvas()
    {
        var c = GetComponentInParent<Canvas>(true);
        if (c != null)
        {
            return c.rootCanvas != null ? c.rootCanvas : c;
        }

        var nav = GameObject.Find("NavigationUI");
        if (nav != null)
        {
            c = nav.GetComponentInParent<Canvas>(true);
            if (c != null)
            {
                return c.rootCanvas != null ? c.rootCanvas : c;
            }
        }

        var named = GameObject.Find("Canvas");
        if (named != null && named.TryGetComponent<Canvas>(out var rootNamed))
        {
            return rootNamed.rootCanvas != null ? rootNamed.rootCanvas : rootNamed;
        }

        return FindFallbackScreenCanvas();
    }

    private static Canvas FindFallbackScreenCanvas()
    {
        foreach (var c in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (c == null)
            {
                continue;
            }

            var root = c.rootCanvas != null ? c.rootCanvas : c;
            if (!root.gameObject.activeInHierarchy)
            {
                continue;
            }

            if (root.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                return root;
            }
        }

        return UnityEngine.Object.FindFirstObjectByType<Canvas>();
    }

    private void EnsureOpenFabImpl()
    {
        var canvas = ResolveTargetCanvas();
        if (canvas == null)
        {
            return;
        }

        var canvasRt = canvas.transform as RectTransform;
        if (canvasRt == null)
        {
            return;
        }

        if (FindDeepRect(canvasRt, OpenFabName) != null)
        {
            return;
        }

        var searchButton = FindDeepRect(canvasRt, "ShowDestinationsButton");
        var captureButton = FindDeepRect(canvasRt, "CaptureButton");
        if (searchButton == null || captureButton == null)
        {
            return;
        }

        var layerRt = GetOrCreateFabLayer(canvasRt);

        var fabGo = new GameObject(OpenFabName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        var fabRt = fabGo.GetComponent<RectTransform>();
        fabRt.SetParent(layerRt, false);

        fabRt.anchorMin = fabRt.anchorMax = new Vector2(0.5f, 0.5f);
        fabRt.pivot = new Vector2(0.5f, 0.5f);
        fabRt.sizeDelta = new Vector2(FabSize, FabSize);

        var follow = fabGo.AddComponent<AccessibilityFabFollowSearch>();
        follow.search = searchButton;
        follow.capture = captureButton;
        follow.ApplyNow();

        var img = fabGo.GetComponent<Image>();
        img.sprite = GetFabCircleSprite();
        img.type = Image.Type.Simple;
        img.color = Color.white;
        img.raycastTarget = true;

        var outline = fabGo.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.12f);
        outline.effectDistance = new Vector2(2f, -2f);
        outline.useGraphicAlpha = true;

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        var labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.SetParent(fabRt, false);
        StretchFull(labelRt);
        var tmp = labelGo.GetComponent<TextMeshProUGUI>();
        tmp.text = "Aa";
        tmp.fontSize = 68f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.12f, 0.14f, 0.18f, 1f);
        tmp.raycastTarget = false;

        var btn = fabGo.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.transition = Selectable.Transition.ColorTint;
        btn.onClick.AddListener(() =>
        {
            if (NavigationUIController.instance != null)
            {
                NavigationUIController.instance.ToggleAccessibilitySelectUI();
            }
        });

        layerRt.SetAsLastSibling();
    }

    private static RectTransform GetOrCreateFabLayer(RectTransform canvasRt)
    {
        var existing = canvasRt.Find(FabLayerName) as RectTransform;
        if (existing != null)
        {
            EnsureFabLayerComponents(existing.gameObject);
            StretchFull(existing);
            return existing;
        }

        var go = new GameObject(FabLayerName, typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(canvasRt, false);
        StretchFull(rt);
        EnsureFabLayerComponents(go);
        return rt;
    }

    private static void EnsureFabLayerComponents(GameObject go)
    {
        if (go.GetComponent<Canvas>() == null)
        {
            var c = go.AddComponent<Canvas>();
            c.overrideSorting = true;
            c.sortingOrder = 4000;
        }
        else
        {
            var c = go.GetComponent<Canvas>();
            c.overrideSorting = true;
            c.sortingOrder = 4000;
        }

        if (go.GetComponent<GraphicRaycaster>() == null)
        {
            go.AddComponent<GraphicRaycaster>();
        }

        if (go.GetComponent<AccessibilityFabZOrder>() == null)
        {
            go.AddComponent<AccessibilityFabZOrder>();
        }

        if (go.GetComponent<AccessibilityFabVisibilitySync>() == null)
        {
            go.AddComponent<AccessibilityFabVisibilitySync>();
        }
    }

    private void ClosePanel()
    {
        if (NavigationUIController.instance != null)
        {
            NavigationUIController.instance.CloseAccessibilitySelectUI();
        }
    }

    private static RectTransform FindDeepRect(Transform root, string objectName)
    {
        if (root.name == objectName && root is RectTransform rr)
        {
            return rr;
        }

        for (var i = 0; i < root.childCount; i++)
        {
            var f = FindDeepRect(root.GetChild(i), objectName);
            if (f != null)
            {
                return f;
            }
        }

        return null;
    }

    private static void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    private static TextMeshProUGUI CreateTmpText(
        RectTransform parent,
        string objectName,
        string text,
        float fontSize,
        TextAlignmentOptions align,
        Vector2 anchoredPos,
        Vector2 sizeDelta,
        Color color,
        bool bold)
    {
        var go = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = sizeDelta;
        if (parent.GetComponent<VerticalLayoutGroup>() != null)
        {
            var le = go.AddComponent<LayoutElement>();
            le.minHeight = Mathf.Max(24f, sizeDelta.y);
            le.preferredHeight = le.minHeight;
        }

        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = color;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.raycastTarget = false;
        return tmp;
    }

    private static Toggle CreateToggle(RectTransform parent, string objectName, string label, bool isLeftColumn)
    {
        var root = new GameObject(objectName, typeof(RectTransform), typeof(Toggle), typeof(LayoutElement));
        var rt = root.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = Vector2.zero;
        rt.anchoredPosition = Vector2.zero;
        var leRoot = root.GetComponent<LayoutElement>();
        leRoot.flexibleWidth = 1f;
        leRoot.minWidth = 160f;
        leRoot.preferredHeight = 88f;
        leRoot.minHeight = 88f;

        var col = new GameObject("Column", typeof(RectTransform));
        var colRt = col.GetComponent<RectTransform>();
        colRt.SetParent(rt, false);
        colRt.localScale = Vector3.one;
        StretchFull(colRt);

        if (isLeftColumn)
        {
            colRt.anchorMin = new Vector2(0f, 0f);
            colRt.anchorMax = new Vector2(0.5f, 1f);
            colRt.pivot = new Vector2(0.5f, 0.5f);
            colRt.offsetMin = Vector2.zero;
            colRt.offsetMax = new Vector2(-6f, 0f);
        }
        else
        {
            colRt.anchorMin = new Vector2(0.5f, 0f);
            colRt.anchorMax = new Vector2(1f, 1f);
            colRt.pivot = new Vector2(0.5f, 0.5f);
            colRt.offsetMin = new Vector2(6f, 0f);
            colRt.offsetMax = Vector2.zero;
        }

        var background = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var bgRt = background.GetComponent<RectTransform>();
        bgRt.SetParent(colRt, false);
        StretchFull(bgRt);
        var bgImage = background.GetComponent<Image>();
        bgImage.color = new Color(0.93f, 0.95f, 0.98f, 1f);

        var outlineBg = background.AddComponent<Outline>();
        outlineBg.effectColor = new Color(0f, 0f, 0f, 0.08f);
        outlineBg.effectDistance = new Vector2(1f, -1f);
        outlineBg.useGraphicAlpha = true;

        var check = new GameObject("Checkmark", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        var checkRt = check.GetComponent<RectTransform>();
        checkRt.SetParent(bgRt, false);
        checkRt.anchorMin = new Vector2(0f, 0.5f);
        checkRt.anchorMax = new Vector2(0f, 0.5f);
        checkRt.pivot = new Vector2(0.5f, 0.5f);
        checkRt.anchoredPosition = new Vector2(16f, 0f);
        checkRt.sizeDelta = new Vector2(18f, 18f);
        var checkImage = check.GetComponent<Image>();
        checkImage.color = new Color(0.10f, 0.48f, 0.95f, 1f);

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        var labelRt = labelGo.GetComponent<RectTransform>();
        labelRt.SetParent(colRt, false);
        labelRt.anchorMin = new Vector2(0f, 0f);
        labelRt.anchorMax = new Vector2(1f, 1f);
        labelRt.pivot = new Vector2(0f, 0.5f);
        labelRt.offsetMin = new Vector2(40f, 0f);
        labelRt.offsetMax = new Vector2(-10f, 0f);
        var labelTmp = labelGo.GetComponent<TextMeshProUGUI>();
        labelTmp.text = label;
        labelTmp.fontSize = 17f;
        labelTmp.color = new Color(0.12f, 0.14f, 0.18f, 1f);
        labelTmp.alignment = TextAlignmentOptions.Left;
        labelTmp.raycastTarget = false;

        var toggle = root.GetComponent<Toggle>();
        toggle.targetGraphic = bgImage;
        toggle.graphic = checkImage;
        toggle.isOn = false;
        toggle.transition = Selectable.Transition.ColorTint;

        return toggle;
    }

}

/// <summary>
/// Runs after most UI so the FAB layer stays the last sibling under the root canvas (localization adds full-screen UI later).
/// </summary>
[DefaultExecutionOrder(32000)]
public sealed class AccessibilityFabZOrder : MonoBehaviour
{
    private void LateUpdate()
    {
        var p = transform.parent;
        if (p != null && transform.GetSiblingIndex() != p.childCount - 1)
        {
            transform.SetAsLastSibling();
        }
    }
}

/// <summary>
/// Keeps the Aa FAB centered between Search and Capture (world midpoint), every frame for layout/safe-area.
/// </summary>
[DefaultExecutionOrder(31900)]//31900 is the default execution order for the AccessibilityFabFollowSearch class, the order its executed in the scene.
public sealed class AccessibilityFabFollowSearch : MonoBehaviour
{
    [HideInInspector] public RectTransform search;
    [HideInInspector] public RectTransform capture;

    public void ApplyNow()
    {
        if (search == null || capture == null)
        {
            return;
        }

        var a = search.TransformPoint(new Vector3(search.rect.center.x, search.rect.center.y, 0f));
        var b = capture.TransformPoint(new Vector3(capture.rect.center.x, capture.rect.center.y, 0f));
        transform.position = (a + b) * 0.5f;
    }

    private void LateUpdate()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        ApplyNow();
    }
}

/// <summary>
/// Hides the Aa button while the accessibility sheet is open. Runs on the always-active FAB layer so visibility
/// can be restored when the sheet closes (scripts on the FAB itself do not run while inactive).
/// </summary>
[DefaultExecutionOrder(31800)]
public sealed class AccessibilityFabVisibilitySync : MonoBehaviour
{
    private const string FabObjectName = "AccessibilityOpenFab";

    private void LateUpdate()
    {
        var fab = transform.Find(FabObjectName);
        if (fab == null)
        {
            return;
        }

        var nav = NavigationUIController.instance;
        var show = nav == null || nav.AccessibilitySelectUI == null || !nav.AccessibilitySelectUI.activeSelf;
        if (fab.gameObject.activeSelf != show)
        {
            fab.gameObject.SetActive(show);
        }
    }
}
