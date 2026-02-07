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
                        FarmLog.Action(agent.gameObject.name, $"WaterCrop START | Target={crop.name} (reserved, verified)");
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
                    FarmLog.ActionWarn(agent.gameObject.name, $"WaterCrop FALLBACK | Claimed free crop {bestFallback.name}");
                }
            }
            
            if (data.Crop == null)
            {
                FarmLog.ActionWarn(agent.gameObject.name, "WaterCrop ABORTED | No waterable crop at target");
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

                data.ActionCompleted = true;
                FarmLog.Action(agent.gameObject.name, $"WaterCrop PERFORM | {data.Crop.name} watered -> Stage={data.Crop.GrowthStage}");
                return ActionRunState.Completed;
            }

            return ActionRunState.Continue;
        }

        public override void End(IMonoAgent agent, Data data)
        {
            // Only release on successful completion (ActionCompleted=true)
            // Interrupted case is handled by Stop()
            if (data.ActionCompleted && data.Crop != null && data.Agent != null && CropManager.Instance != null)
            {
                CropManager.Instance.ReleaseCrop(data.Crop, data.Agent);
                FarmLog.Action(data.Agent.name, $"WaterCrop COMPLETE | {data.Crop.name} watered and released");
            }
        }

        public override void Stop(IMonoAgent agent, Data data)
        {
            // ONLY release crop if action was interrupted (not completed)
            // If ActionCompleted=true, crop should be kept for next farming stage
            if (!data.ActionCompleted && data.Crop != null && data.Agent != null && CropManager.Instance != null)
            {
                CropManager.Instance.ReleaseCrop(data.Crop, data.Agent);
                FarmLog.Action(data.Agent.name, $"WaterCrop INTERRUPTED | {data.Crop.name} released");
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
