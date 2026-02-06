using UnityEngine;
using FarmingGoap.Behaviours;
using CrashKonijn.Goap.Runtime;

namespace FarmingGoap.Debug
{
    /// <summary>
    /// Helper untuk debugging stats NPC di Inspector
    /// Attach ke Farmer GameObject untuk melihat real-time stats
    /// </summary>
    public class NPCDebugDisplay : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private NPCStats stats;
        [SerializeField] private CropBehaviour crop;
        [SerializeField] private GoapActionProvider actionProvider;

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

        private void OnGUI()
        {
            // Optional: Display on screen
            if (stats == null) return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
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
