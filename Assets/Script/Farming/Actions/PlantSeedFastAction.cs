using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
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

            // Find crop
            var targetPos = data.Target.Position;
            var nearbyColliders = Physics2D.OverlapCircleAll(targetPos, 1f);
            
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

                return ActionRunState.Completed;
            }

            return ActionRunState.Continue;
        }

        public override void End(IMonoAgent agent, Data data)
        {
        }

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            public float Timer;
            public CropBehaviour Crop;
        }
    }
}
