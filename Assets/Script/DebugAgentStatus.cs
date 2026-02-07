using UnityEngine;
using CrashKonijn.Goap.Runtime;
using CrashKonijn.Agent.Runtime;
using FarmingGoap.Behaviours;
using BehaviorDesigner.Runtime;

/// <summary>
/// Shows a floating debug label above agent's head:
/// Agent Name / Current Goal / Current Action
/// Attach to each agent GameObject
/// </summary>
public class DebugAgentStatus : MonoBehaviour
{
    [Header("Display Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, 0f);
    [SerializeField] private Color bgColor = new Color(0f, 0f, 0f, 0.7f);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private int fontSize = 12;
    
    private GoapActionProvider goapProvider;
    private string currentGoal = "None";
    private string currentAction = "None";
    
    // Cached styles
    private GUIStyle labelStyle;
    private GUIStyle bgStyle;
    private Texture2D bgTexture;

    private void Awake()
    {
        goapProvider = GetComponent<GoapActionProvider>();
    }

    private void Update()
    {
        if (goapProvider != null && goapProvider.CurrentPlan != null)
        {
            currentGoal = goapProvider.CurrentPlan.Goal?.GetType().Name ?? "None";
            currentAction = goapProvider.CurrentPlan.Action?.GetType().Name ?? "None";
        }
        else
        {
            currentGoal = "No Goal";
            currentAction = "Idle";
        }
    }

    private void OnGUI()
    {
        Camera cam = Camera.main;
        if (cam == null) return;

        Vector3 worldPos = transform.position + offset;
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);

        // Behind camera check
        if (screenPos.z < 0) return;

        // Unity GUI uses top-left origin, Camera uses bottom-left
        float guiY = Screen.height - screenPos.y;

        // Initialize styles once
        if (labelStyle == null)
        {
            bgTexture = new Texture2D(1, 1);
            bgTexture.SetPixel(0, 0, bgColor);
            bgTexture.Apply();

            labelStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = fontSize,
                wordWrap = false,
                normal = { textColor = textColor }
            };

            bgStyle = new GUIStyle()
            {
                normal = { background = bgTexture }
            };
        }

        string text = $"{gameObject.name}\nGoal: {currentGoal}\nAction: {currentAction}";
        
        GUIContent content = new GUIContent(text);
        Vector2 size = labelStyle.CalcSize(content);
        // Add padding
        size.x += 12f;
        size.y += 6f;

        Rect rect = new Rect(screenPos.x - size.x * 0.5f, guiY - size.y, size.x, size.y);

        GUI.Box(rect, GUIContent.none, bgStyle);
        GUI.Label(rect, text, labelStyle);
    }

    private void OnDestroy()
    {
        if (bgTexture != null)
            Destroy(bgTexture);
    }
}
