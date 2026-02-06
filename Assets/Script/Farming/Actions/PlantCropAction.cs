using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using UnityEngine;

namespace FarmingGoap.Actions
{
    [GoapId("Farming-PlantCropAction")]
    public class PlantCropAction : GoapActionBase<PlantCropAction.Data>
    {
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.Timer = 0f;
            
            // Find crop at target position
            var targetPos = data.Target.Position;
            var nearbyColliders = UnityEngine.Physics2D.OverlapCircleAll(targetPos, 1f);
            
            foreach (var col in nearbyColliders)
            {
                var crop = col.GetComponent<CropBehaviour>();
                if (crop != null)
                {
                    // CRITICAL: Verify this crop is reserved by THIS agent
                    var reservedAgent = FarmingGoap.Managers.CropManager.Instance?.GetReservedAgent(crop);
                    if (reservedAgent == agent.gameObject)
                    {
                        data.Crop = crop;
                        UnityEngine.Debug.Log($"[PlantCrop] {agent.gameObject.name} verified reserved crop: {crop.name}");
                        break;
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning($"[PlantCrop] {agent.gameObject.name} found {crop.name} but it's reserved by {reservedAgent?.name ?? "NONE"}! Ignoring.");
                    }
                }
            }
            
            if (data.Crop == null)
            {
                UnityEngine.Debug.LogError($"[PlantCrop] {agent.gameObject.name} couldn't find their reserved crop at target position!");
            }
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            data.Timer += context.DeltaTime;

            // Simulate planting time (2 detik)
            if (data.Timer < 2f)
                return ActionRunState.Continue;

            // Set crop stage to planted (1)
            var crop = data.Crop;
            if (crop != null)
            {
                crop.SetGrowthStage(1);
                UnityEngine.Debug.Log($"[PlantCropAction] Tanaman ditanam!");
            }

            data.ActionCompleted = true; // Mark as successfully completed
            return ActionRunState.Completed;
        }

        public override void End(IMonoAgent agent, Data data)
        {
        }

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            
            public CropBehaviour Crop { get; set; } // Manual reference, not GetComponent
            
            public float Timer { get; set; }
            
            public bool ActionCompleted { get; set; } // Track if action finished successfully
        }
    }
}
