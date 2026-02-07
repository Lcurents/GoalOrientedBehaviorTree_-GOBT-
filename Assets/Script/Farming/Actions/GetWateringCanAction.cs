using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using UnityEngine;

namespace FarmingGoap.Actions
{
    [GoapId("Farming-GetWateringCanAction")]
    public class GetWateringCanAction : GoapActionBase<GetWateringCanAction.Data>
    {
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.Timer = 1f; // 1 detik untuk ambil ember
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            data.Timer -= context.DeltaTime;

            if (data.Timer <= 0f)
            {
                // SUCCESS: Tambah watering can ke inventory
                var stats = agent.GetComponent<NPCStats>();
                if (stats != null)
                {
                    if (stats.HasWateringCan < 1)
                    {
                        stats.HasWateringCan++;
                        FarmLog.Resource(agent.gameObject.name, $"GetWateringCan | WateringCan={stats.HasWateringCan}");
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
        }
    }
}
