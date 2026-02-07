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
                UnityEngine.Debug.Log($"[FarmerBrain] {gameObject.name}: BehaviorTree detected → forced to BT mode (useCodeBasedSelection=false)");
            }

            // Assign AgentType in Awake - GoapBehaviour runs at -100 so it's already initialized
            // GoapSetupHelper runs at -101 ensuring factory is registered before GoapBehaviour
            if (this.provider.AgentTypeBehaviour == null && this.goap != null)
            {
                try
                {
                    this.provider.AgentType = this.goap.GetAgentType("FarmerAgent");
                    agentTypeAssigned = true;
                    UnityEngine.Debug.Log($"[FarmerBrain] ✓ AgentType assigned to {gameObject.name}");
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError($"[FarmerBrain] ✗ Failed to assign AgentType to {gameObject.name}: {e.Message}");
                    UnityEngine.Debug.LogError($"[FarmerBrain] SOLUTION: Add 'GoapSetupHelper' + 'FarmerAgentTypeFactory' to '{goap.name}' GameObject!");
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
            
            // Priority 1: Eat (hanya jika punya food)
            if (stats.Hunger > 70f && stats.FoodCount > 0)
            {
                provider.RequestGoal<EatGoal>();
                UnityEngine.Debug.Log("[Brain] Priority: EatGoal");
                return;
            }

            // Priority 2: Sleep (jika energy rendah)
            if (stats.Energy < 30f)
            {
                provider.RequestGoal<SleepGoal>();
                UnityEngine.Debug.Log("[Brain] Priority: SleepGoal");
                return;
            }

            // ===== UTILITY-BASED (Farming Goals) =====
            
            // Hitung utility untuk 3 farming goals
            float plantingUtility = UtilityCalculator.CalculatePlantingUtility(
                stats.Energy, 
                stats.Hunger, 
                crop.GrowthStage
            );
            
            float wateringUtility = UtilityCalculator.CalculateWateringUtility(
                stats.Energy, 
                stats.Hunger, 
                crop.GrowthStage
            );
            
            float harvestingUtility = UtilityCalculator.CalculateHarvestingUtility(
                stats.Energy, 
                stats.Hunger, 
                crop.GrowthStage
            );
            
            // Debug utility values
            UnityEngine.Debug.Log(UtilityCalculator.GetUtilityDebugString(
                plantingUtility, 
                wateringUtility, 
                harvestingUtility
            ));
            
            // Pilih goal dengan utility tertinggi
            float maxUtility = Mathf.Max(plantingUtility, wateringUtility, harvestingUtility);
            
            // Jika ada goal yang valid (utility > -999)
            if (maxUtility > -999f)
            {
                if (maxUtility == harvestingUtility)
                {
                    provider.RequestGoal<HarvestingGoal>();
                    UnityEngine.Debug.Log($"[Brain] Utility: HarvestingGoal (U={maxUtility:F3})");
                }
                else if (maxUtility == plantingUtility)
                {
                    provider.RequestGoal<PlantingGoal>();
                    UnityEngine.Debug.Log($"[Brain] Utility: PlantingGoal (U={maxUtility:F3})");
                }
                else if (maxUtility == wateringUtility)
                {
                    provider.RequestGoal<WateringGoal>();
                    UnityEngine.Debug.Log($"[Brain] Utility: WateringGoal (U={maxUtility:F3})");
                }
                return;
            }

            // ===== FALLBACK =====
            
            // Tidak ada goal yang bisa dilakukan → Idle
            provider.RequestGoal<IdleGoal>();
            UnityEngine.Debug.Log("[Brain] Fallback: IdleGoal");
        }
    }
}
