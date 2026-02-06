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
                    data.Crop = crop;
                    break;
                }
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
        }
    }
}
