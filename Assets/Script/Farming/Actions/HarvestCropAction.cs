using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using FarmingGoap.Managers;
using UnityEngine;

namespace FarmingGoap.Actions
{
    [GoapId("Farming-HarvestCropAction")]
    public class HarvestCropAction : GoapActionBase<HarvestCropAction.Data>
    {
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.Timer = 4f; // 4 detik untuk panen
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
                    // Verify this crop is reserved by THIS agent
                    var reservedAgent = CropManager.Instance?.GetReservedAgent(crop);
                    if (reservedAgent == agent.gameObject)
                    {
                        data.Crop = crop;
                        UnityEngine.Debug.Log($"[Harvest] {agent.gameObject.name} verified reserved crop: {crop.name}");
                        break;
                    }
                    
                    // Track unreserved crop at harvest stage as fallback
                    if (reservedAgent == null && crop.GrowthStage == 3)
                    {
                        bestFallback = crop;
                    }
                }
            }
            
            // Fallback: if no reserved crop found but there's a free harvestable crop nearby,
            // reserve it and proceed (handles edge case where reservation was lost between frames)
            if (data.Crop == null && bestFallback != null && CropManager.Instance != null)
            {
                CropManager.Instance.SubmitBid(bestFallback, agent.gameObject, 1f, "Harvesting");
                if (CropManager.Instance.IsReservedBy(bestFallback, agent.gameObject))
                {
                    data.Crop = bestFallback;
                    UnityEngine.Debug.LogWarning($"[Harvest] {agent.gameObject.name} fallback: claimed free crop {bestFallback.name}");
                }
            }
            
            if (data.Crop == null)
            {
                UnityEngine.Debug.LogWarning($"[Harvest] {agent.gameObject.name} no harvestable crop at target - action will stop");
            }
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            if (data.Crop == null)
                return ActionRunState.Stop;

            data.Timer -= context.DeltaTime;

            if (data.Timer <= 0f)
            {
                // Harvest the crop
                data.Crop.Harvest();
                
                // Add food
                var stats = data.Stats;
                if (stats != null)
                {
                    stats.AddFood(1);
                    UnityEngine.Debug.Log($"[HarvestCropAction] Panen berhasil! Food: {stats.FoodCount}");
                }

                data.ActionCompleted = true; // Mark as successfully completed
                return ActionRunState.Completed;
            }

            return ActionRunState.Continue;
        }

        public override void End(IMonoAgent agent, Data data)
        {
            // Release crop setelah harvest (crop reset to stage 0)
            if (data.Crop != null && data.Agent != null && CropManager.Instance != null)
            {
                CropManager.Instance.ReleaseCrop(data.Crop, data.Agent);
                UnityEngine.Debug.Log($"[Harvest] {data.Agent.name} finished harvesting {data.Crop.name}, RELEASED");
            }
        }

        public override void Stop(IMonoAgent agent, Data data)
        {
            // ONLY release crop if action was interrupted (not completed)
            // If ActionCompleted=true, End() already released the crop
            if (!data.ActionCompleted && data.Crop != null && data.Agent != null && CropManager.Instance != null)
            {
                CropManager.Instance.ReleaseCrop(data.Crop, data.Agent);
                UnityEngine.Debug.Log($"[Harvest] {data.Agent.name} INTERRUPTED, released {data.Crop.name}");
            }
        }

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            
            public CropBehaviour Crop { get; set; } // Manual reference
            
            [GetComponent]
            public NPCStats Stats { get; set; }
            
            public float Timer { get; set; }
            
            public GameObject Agent { get; set; } // For crop release
            
            public bool ActionCompleted { get; set; } // Track if action completed successfully
        }
    }
}
