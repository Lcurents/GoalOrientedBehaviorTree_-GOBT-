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
            
            foreach (var col in nearbyColliders)
            {
                var crop = col.GetComponent<CropBehaviour>();
                if (crop != null)
                {
                    // CRITICAL: Verify this crop is reserved by THIS agent
                    var reservedAgent = CropManager.Instance?.GetReservedAgent(crop);
                    if (reservedAgent == agent.gameObject)
                    {
                        data.Crop = crop;
                        UnityEngine.Debug.Log($"[Water] {agent.gameObject.name} verified reserved crop: {crop.name}");
                        break;
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"[Water] {agent.gameObject.name} found {crop.name} but it's reserved by {reservedAgent?.name ?? "NONE"}! Ignoring.");
                    }
                }
            }
            
            if (data.Crop == null)
            {
                UnityEngine.Debug.LogError($"[Water] {agent.gameObject.name} couldn't find their reserved crop at target position!");
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
            // Keep reservation for next goal on same crop
            if (data.Crop != null && data.Agent != null)
            {
                UnityEngine.Debug.Log($"[Water] {data.Agent.name} finished watering {data.Crop.name} (keeping reservation)");
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
        }
    }
}
