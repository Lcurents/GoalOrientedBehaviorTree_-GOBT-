using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using UnityEngine;

namespace FarmingGoap.Actions
{
    [GoapId("Farming-EatAction")]
    public class EatAction : GoapActionBase<EatAction.Data>
    {
        public override void Created()
        {
        }

        public override void Start(IMonoAgent agent, Data data)
        {
            data.Timer = 0f;
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            data.Timer += context.DeltaTime;

            // Simulate eating time (1 detik)
            if (data.Timer < 1f)
                return ActionRunState.Continue;

            // Eat food
            var stats = data.Stats;
            if (stats != null && stats.FoodCount > 0)
            {
                stats.RemoveFood(1);
                stats.DecreaseHunger(50);
                FarmLog.Action(agent.gameObject.name, $"Eat COMPLETE | Hunger={stats.Hunger:F0}, SharedFood={stats.FoodCount}");
            }

            return ActionRunState.Completed;
        }

        public override void End(IMonoAgent agent, Data data)
        {
        }

        public class Data : IActionData
        {
            public ITarget Target { get; set; }
            
            [GetComponent]
            public NPCStats Stats { get; set; }
            
            public float Timer { get; set; }
        }
    }
}
