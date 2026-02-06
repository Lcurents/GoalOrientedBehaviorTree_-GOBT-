using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using FarmingGoap.Goals;
using UnityEngine;

namespace FarmingGoap.Brain
{
    public class FarmerBrain : MonoBehaviour
    {
        private AgentBehaviour agent;
        private GoapActionProvider provider;
        private GoapBehaviour goap;
        private NPCStats stats;
        private CropBehaviour crop; // Reference ke crop di scene

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

            // Set AgentType via code (PENTING!)
            if (this.provider.AgentTypeBehaviour == null)
            {
                this.provider.AgentType = this.goap.GetAgentType("FarmerAgent");
            }
        }

        private void Start()
        {
            // Set initial goal
            SelectGoal();
        }

        private void Update()
        {
            // Re-evaluate goal setiap 1 detik
            if (Time.frameCount % 60 == 0)
            {
                SelectGoal();
            }
        }

        private void SelectGoal()
        {
            if (stats == null || crop == null)
                return;

            // ========== REDESIGNED BRAIN - Priority Based ==========
            
            // Priority 1: Eat (hanya jika punya food)
            if (stats.Hunger > 70f && stats.FoodCount > 0)
            {
                provider.RequestGoal<EatGoal>();
                return;
            }

            // Priority 2: Sleep (jika energy rendah)
            if (stats.Energy < 30f)
            {
                provider.RequestGoal<SleepGoal>();
                return;
            }

            // Priority 3: Harvest (jika crop matang)
            if (crop.GrowthStage >= 3)
            {
                provider.RequestGoal<HarvestingGoal>();
                return;
            }

            // Priority 4: Water (jika crop perlu air)
            if (crop.GrowthStage >= 1 && crop.GrowthStage < 3)
            {
                provider.RequestGoal<WateringGoal>();
                return;
            }

            // Priority 5: Plant (jika tanah kosong)
            if (crop.GrowthStage == 0)
            {
                provider.RequestGoal<PlantingGoal>();
                return;
            }

            // Priority 6: Idle (tidak ada yang dilakukan)
            provider.RequestGoal<IdleGoal>();
        }
    }
}
