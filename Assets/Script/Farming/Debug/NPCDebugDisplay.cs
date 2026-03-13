using UnityEngine;
using FarmingGoap.Behaviours;
using CrashKonijn.Goap.Runtime;
using System.Linq;

namespace FarmingGoap.Debug
{
    /// <summary>
    /// Helper untuk debugging stats NPC di Inspector
    /// Attach ke Farmer GameObject untuk melihat real-time stats
    /// </summary>
    public class NPCDebugDisplay : MonoBehaviour
    {
        private static readonly System.Collections.Generic.List<NPCDebugDisplay> ActiveDisplays =
            new System.Collections.Generic.List<NPCDebugDisplay>();

        [Header("References")]
        [SerializeField] private NPCStats stats;
        [SerializeField] private CropBehaviour crop;
        [SerializeField] private GoapActionProvider actionProvider;

        [Header("Screen Display")]
        [SerializeField] private bool enableScreenDisplay = true;
        [SerializeField] private float marginX = 10f;
        [SerializeField] private float marginY = 10f;
        [SerializeField] private float blockWidth = 220f;
        [SerializeField] private float blockHeight = 150f;
        [SerializeField] private float blockSpacing = 10f;

        [Header("Current Stats (Read-Only)")]
        [SerializeField] private float currentHunger;
        [SerializeField] private float currentEnergy;
        [SerializeField] private int currentFood;
        [SerializeField] private int currentCropStage;

        [Header("Current Goal/Action")]
        [SerializeField] private string currentGoal;
        [SerializeField] private string currentAction;

        private void OnValidate()
        {
            // Auto-assign if missing
            if (stats == null) stats = GetComponent<NPCStats>();
            if (crop == null) crop = GetComponent<CropBehaviour>();
            if (actionProvider == null) actionProvider = GetComponent<GoapActionProvider>();
        }

        private void Update()
        {
            if (stats != null)
            {
                currentHunger = stats.Hunger;
                currentEnergy = stats.Energy;
                currentFood = stats.FoodCount;
            }

            if (crop != null)
            {
                currentCropStage = crop.GrowthStage;
            }

            if (actionProvider != null)
            {
                currentGoal = actionProvider.CurrentPlan?.Goal?.GetType().Name ?? "None";
                currentAction = actionProvider.CurrentPlan?.Action?.GetType().Name ?? "None";
            }
        }

        private void OnEnable()
        {
            if (!ActiveDisplays.Contains(this))
                ActiveDisplays.Add(this);
        }

        private void OnDisable()
        {
            ActiveDisplays.Remove(this);
        }

        private void OnGUI()
        {
            // Optional: Display on screen
            if (!enableScreenDisplay || stats == null)
                return;

            // Stable ordering by agent name
            var ordered = ActiveDisplays.OrderBy(d => d.gameObject.name).ToList();
            int index = ordered.IndexOf(this);
            if (index < 0)
                return;

            float x = marginX + index * (blockWidth + blockSpacing);
            float y = marginY;

            GUILayout.BeginArea(new Rect(x, y, blockWidth, blockHeight));
            GUILayout.Label($"<b>NPC Stats</b>");
            GUILayout.Label($"Hunger: {stats.Hunger:F1}/100");
            GUILayout.Label($"Energy: {stats.Energy:F1}/100");
            GUILayout.Label($"Food: {stats.FoodCount}");
            
            if (crop != null)
                GUILayout.Label($"Crop Stage: {crop.GrowthStage}/3");
            
            if (actionProvider != null)
            {
                GUILayout.Label($"Goal: {currentGoal}");
                GUILayout.Label($"Action: {currentAction}");
            }
            
            GUILayout.EndArea();
        }
    }
}
