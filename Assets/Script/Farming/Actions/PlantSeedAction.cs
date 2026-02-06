using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using UnityEngine;

namespace FarmingGoap.Actions
{
    [GoapId("Farming-PlantSeedAction")]
    public class PlantSeedAction : GoapActionBase<PlantSeedAction.Data>
    {
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            var stats = agent.GetComponent<NPCStats>();
            
            // Cek apakah punya sekop atau tidak
            if (stats != null && stats.HasShovel > 0)
            {
                data.Timer = 2f; // Dengan sekop: cepat (2 detik)
            }
            else
            {
                data.Timer = 5f; // Tanpa sekop: lambat (5 detik)
            }

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
                // Tanam bibit
                data.Crop.Plant();
                
                // Consume bibit dan sekop
                var stats = agent.GetComponent<NPCStats>();
                if (stats != null)
                {
                    stats.HasSeed--;
                    if (stats.HasShovel > 0)
                    {
                        stats.HasShovel--; // Sekop habis dipakai
                    }
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
