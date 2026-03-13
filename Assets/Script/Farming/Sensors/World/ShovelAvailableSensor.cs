using CrashKonijn.Agent.Core;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Managers;

namespace FarmingGoap.Sensors.World
{
    /// <summary>
    /// Global sensor: reads shared shovel availability from storage.
    /// </summary>
    [GoapId("Farming-ShovelAvailableSensor")]
    public class ShovelAvailableSensor : LocalWorldSensorBase
    {
        public override void Created() { }
        public override void Update() { }

        public override SenseValue Sense(IActionReceiver agent, IComponentReference references)
        {
            if (ShovelStorage.Instance == null)
                return 0;

            var agentObject = agent.Transform.gameObject;
            if (ShovelStorage.Instance.IsReservedBy(agentObject))
                return 1;

            return ShovelStorage.Instance.IsAvailable() ? 1 : 0;
        }
    }
}
