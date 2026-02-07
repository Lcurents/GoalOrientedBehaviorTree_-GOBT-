using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using FarmingGoap.Managers;
using UnityEngine;

namespace FarmingGoap.Actions
{
    [GoapId("Farming-WaterCropAction")]
    public class WaterCropAction : GoapActionBase<WaterCropAction.Data>
    {
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.Timer = 3f; // 3 detik untuk siram
            data.Agent = agent.gameObject; // Store agent reference
            
            // Find crop at target position
            var targetPos = data.Target.Position;
            var nearbyColliders = UnityEngine.Physics2D.OverlapCircleAll(targetPos, 1f);
            
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
                        UnityEngine.Debug.Log($"[Water] {agent.gameObject.name} verified reserved crop: {crop.name}");
                        break;
                    }
                    
                    // Track unreserved crop that needs water as fallback
                    if (reservedAgent == null && (crop.GrowthStage == 1 || crop.GrowthStage == 2))
                    {
                        bestFallback = crop;
                    }
                }
            }
            
            // Fallback: claim free waterable crop if reservation was lost
            if (data.Crop == null && bestFallback != null && CropManager.Instance != null)
            {
                CropManager.Instance.SubmitBid(bestFallback, agent.gameObject, 1f, "Watering");
                if (CropManager.Instance.IsReservedBy(bestFallback, agent.gameObject))
                {
                    data.Crop = bestFallback;
                    UnityEngine.Debug.LogWarning($"[Water] {agent.gameObject.name} fallback: claimed free crop {bestFallback.name}");
                }
            }
            
            if (data.Crop == null)
            {
                UnityEngine.Debug.LogWarning($"[Water] {agent.gameObject.name} no waterable crop at target - action will stop");
            }
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (data.Crop == null)
                return ActionRunState.Stop;

            data.Timer -= context.DeltaTime;

            if (data.Timer <= 0f)
            {
                data.Crop.WaterCrop();
                
                // Consume watering can
                var stats = agent.GetComponent<NPCStats>();
                if (stats != null)
                {
                    stats.HasWateringCan--;
                }

                data.ActionCompleted = true; // Mark as successfully completed
                UnityEngine.Debug.Log($"[WaterCropAction] Tanaman disiram! Stage: {data.Crop.GrowthStage}");
                return ActionRunState.Completed;
            }

            return ActionRunState.Continue;
        }

        public override void End(IMonoAgent agent, Data data)
        {
            // Release crop after watering completes - crop is now available for ANY farmer
            if (data.Crop != null && data.Agent != null && CropManager.Instance != null)
            {
                CropManager.Instance.ReleaseCrop(data.Crop, data.Agent);
                UnityEngine.Debug.Log($"[Water] {data.Agent.name} finished watering {data.Crop.name}, RELEASED");
            }
        }

        public override void Stop(IMonoAgent agent, Data data)
        {
            // ONLY release crop if action was interrupted (not completed)
            // If ActionCompleted=true, crop should be kept for next farming stage
            if (!data.ActionCompleted && data.Crop != null && data.Agent != null && CropManager.Instance != null)
            {
                CropManager.Instance.ReleaseCrop(data.Crop, data.Agent);
                UnityEngine.Debug.Log($"[Water] {data.Agent.name} INTERRUPTED, released {data.Crop.name}");
            }
        }

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            
            public CropBehaviour Crop { get; set; } // Manual reference
            
            public float Timer { get; set; }
            
            public GameObject Agent { get; set; } // For crop release
            
            public bool ActionCompleted { get; set; } // Track if action completed successfully
        }
    }
}
