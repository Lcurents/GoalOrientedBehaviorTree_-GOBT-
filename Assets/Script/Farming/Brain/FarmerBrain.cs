using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using FarmingGoap.Goals;
using FarmingGoap.AgentTypes;
using UnityEngine;

namespace FarmingGoap.Brain
{
    public class FarmerBrain : MonoBehaviour
    {
        [Header("Goal Selection Mode")]
        [Tooltip("TRUE = Use FarmerBrain code-based selection | FALSE = Use Behavior Tree")]
        [SerializeField] private bool useCodeBasedSelection = true;
        
        private AgentBehaviour agent;
        private GoapActionProvider provider;
        private GoapBehaviour goap;
        private NPCStats stats;
        private CropBehaviour crop; // Reference ke crop di scene
        private bool agentTypeAssigned = false;

        private void Awake()
        {
            this.goap = FindFirstObjectByType<GoapBehaviour>();
            this.agent = GetComponent<AgentBehaviour>();
            this.provider = GetComponent<GoapActionProvider>();
            this.stats = GetComponent<NPCStats>();
            
            // Find crop in scene (BUKAN dari component sendiri!)
            this.crop = FindFirstObjectByType<CropBehaviour>();

            // Link AgentBehaviour dengan GoapActionProvider (WAJIB!)
            this.agent.ActionProvider = this.provider;

            // AUTO-DETECT: If BehaviorTree exists, force BT mode
            // Code-based mode doesn't use CropManager/auction → breaks multi-agent
            var bt = GetComponent<BehaviorDesigner.Runtime.BehaviorTree>();
            if (bt != null && useCodeBasedSelection)
            {
                useCodeBasedSelection = false;
                FarmLog.System($"{gameObject.name}: BehaviorTree detected, switched to BT mode");
            }

            // Assign AgentType in Awake - GoapBehaviour runs at -100 so it's already initialized
            // GoapSetupHelper runs at -101 ensuring factory is registered before GoapBehaviour
            if (this.provider.AgentTypeBehaviour == null && this.goap != null)
            {
                try
                {
                    this.provider.AgentType = this.goap.GetAgentType("FarmerAgent");
                    agentTypeAssigned = true;
                    FarmLog.System($"{gameObject.name}: AgentType 'FarmerAgent' assigned successfully");
                }
                catch (System.Exception e)
                {
                    FarmLog.SystemError($"{gameObject.name}: Failed to assign AgentType - {e.Message}");
                }
            }
            else if (this.provider.AgentTypeBehaviour != null)
            {
                agentTypeAssigned = true;
            }
        }

        private void Start()
        {
            // Set initial goal (hanya jika code-based enabled)
            if (useCodeBasedSelection && agentTypeAssigned)
            {
                SelectGoal();
            }
        }

        private void Update()
        {
            // Hanya jalankan SelectGoal jika tidak pakai Behavior Tree
            if (!useCodeBasedSelection || !agentTypeAssigned)
                return;
            
            // Re-evaluate goal setiap 1 detik
            if (Time.frameCount % 60 == 0)
            {
                SelectGoal();
            }
        }

        private void SelectGoal()
        {
            if (stats == null || crop == null || !agentTypeAssigned)
                return;

            // ========== HYBRID SYSTEM: Priority + Utility ==========
            
            // ===== PRIORITY-BASED (Survival Needs) =====
            
            // Priority 1: Eat (only if has food)
            if (stats.Hunger > 70f && stats.FoodCount > 0)
            {
                provider.RequestGoal<EatGoal>();
                FarmLog.Goal(gameObject.name, $"SELECT EatGoal (Priority) | Hunger={stats.Hunger:F0}, Food={stats.FoodCount}");
                return;
            }

            // Priority 2: Sleep (low energy)
            if (stats.Energy < 30f)
            {
                provider.RequestGoal<SleepGoal>();
                FarmLog.Goal(gameObject.name, $"SELECT SleepGoal (Priority) | Energy={stats.Energy:F0}");
                return;
            }

            // ===== UTILITY-BASED (Farming Goals) =====
            
            float plantingUtility = UtilityCalculator.CalculatePlantingUtility(
                stats.Energy, stats.Hunger, crop.GrowthStage);
            
            float wateringUtility = UtilityCalculator.CalculateWateringUtility(
                stats.Energy, stats.Hunger, crop.GrowthStage);
            
            float harvestingUtility = UtilityCalculator.CalculateHarvestingUtility(
                stats.Energy, stats.Hunger, crop.GrowthStage);
            
            float maxUtility = Mathf.Max(plantingUtility, wateringUtility, harvestingUtility);
            
            if (maxUtility > -999f)
            {
                if (maxUtility == harvestingUtility)
                {
                    provider.RequestGoal<HarvestingGoal>();
                    FarmLog.Goal(gameObject.name, $"SELECT HarvestingGoal (Utility) | U={maxUtility:F3}");
                }
                else if (maxUtility == plantingUtility)
                {
                    provider.RequestGoal<PlantingGoal>();
                    FarmLog.Goal(gameObject.name, $"SELECT PlantingGoal (Utility) | U={maxUtility:F3}");
                }
                else if (maxUtility == wateringUtility)
                {
                    provider.RequestGoal<WateringGoal>();
                    FarmLog.Goal(gameObject.name, $"SELECT WateringGoal (Utility) | U={maxUtility:F3}");
                }
                return;
            }

            // ===== FALLBACK =====
            provider.RequestGoal<IdleGoal>();
            FarmLog.Goal(gameObject.name, "SELECT IdleGoal (Fallback) | No valid farming goal");
        }
    }
}
