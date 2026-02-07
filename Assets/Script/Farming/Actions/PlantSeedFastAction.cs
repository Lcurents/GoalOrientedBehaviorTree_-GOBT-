using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using FarmingGoap.Managers;
using UnityEngine;

namespace FarmingGoap.Actions
{
    /// <summary>
    /// Plant seed WITH shovel - FAST (2 seconds)
    /// Requires: HasSeed + HasShovel
    /// </summary>
    [GoapId("Farming-PlantSeedFastAction")]
    public class PlantSeedFastAction : GoapActionBase<PlantSeedFastAction.Data>
    {
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.Timer = 2f; // Fast with shovel
            data.Agent = agent.gameObject; // Store agent reference for cleanup

            // Find crop at target position
            var targetPos = data.Target.Position;
            var nearbyColliders = Physics2D.OverlapCircleAll(targetPos, 1f);
            
            CropBehaviour bestFallback = null;
            
            foreach (var col in nearbyColliders)
            {
                var crop = col.GetComponent<CropBehaviour>();
                if (crop != null)
                {
                    var reservedAgent = CropManager.Instance?.GetReservedAgent(crop);
                    if (reservedAgent == agent.gameObject)
                    {
                        data.Crop = crop;
                        UnityEngine.Debug.Log($"[PlantFast] {agent.gameObject.name} verified reserved crop: {crop.name}");
                        break;
                    }
                    
                    // Track unreserved empty crop as fallback
                    if (reservedAgent == null && crop.GrowthStage == 0)
                    {
                        bestFallback = crop;
                    }
                }
            }
            
            // Fallback: claim free empty crop if reservation was lost
            if (data.Crop == null && bestFallback != null && CropManager.Instance != null)
            {
                CropManager.Instance.SubmitBid(bestFallback, agent.gameObject, 1f, "Planting");
                if (CropManager.Instance.IsReservedBy(bestFallback, agent.gameObject))
                {
                    data.Crop = bestFallback;
                    UnityEngine.Debug.LogWarning($"[PlantFast] {agent.gameObject.name} fallback: claimed free crop {bestFallback.name}");
                }
            }
            
            if (data.Crop == null)
            {
                UnityEngine.Debug.LogWarning($"[PlantFast] {agent.gameObject.name} no plantable crop at target - action will stop");
            }
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (data.Crop == null)
                return ActionRunState.Stop;

            data.Timer -= context.DeltaTime;

            if (data.Timer <= 0f)
            {
                // Plant seed
                data.Crop.Plant();
                
                // Consume seed and shovel
                var stats = agent.GetComponent<NPCStats>();
                if (stats != null)
                {
                    stats.HasSeed--;
                    stats.HasShovel--; // Consume shovel
                    UnityEngine.Debug.Log($"[PlantSeedFast] Tanam dengan sekop (2s)! Seed={stats.HasSeed}, Shovel={stats.HasShovel}");
                }

                data.ActionCompleted = true; // Mark as successfully completed
                return ActionRunState.Completed;
            }

            return ActionRunState.Continue;
        }

        public override void End(IMonoAgent agent, Data data)
        {
            // Release crop after planting completes - crop is now available for ANY farmer
            // Next goal cycle: farmers re-evaluate and bid for available crops
            if (data.Crop != null && data.Agent != null && CropManager.Instance != null)
            {
                CropManager.Instance.ReleaseCrop(data.Crop, data.Agent);
                UnityEngine.Debug.Log($"[PlantFast] {data.Agent.name} finished planting {data.Crop.name}, RELEASED");
            }
        }

        public override void Stop(IMonoAgent agent, Data data)
        {
            // ONLY release crop if action was interrupted (not completed)
            // If ActionCompleted=true, crop should be kept for next farming stage
            if (!data.ActionCompleted && data.Crop != null && data.Agent != null && CropManager.Instance != null)
            {
                CropManager.Instance.ReleaseCrop(data.Crop, data.Agent);
                UnityEngine.Debug.Log($"[PlantFast] {data.Agent.name} INTERRUPTED, released {data.Crop.name}");
            }
        }

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            public float Timer;
            public CropBehaviour Crop;
            public GameObject Agent; // For crop release
            public bool ActionCompleted; // Track if action finished successfully
        }
    }
}
