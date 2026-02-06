using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Actions;
using FarmingGoap.Goals;
using FarmingGoap.Sensors.Target;
using FarmingGoap.Sensors.World;
using FarmingGoap.TargetKeys;
using FarmingGoap.WorldKeys;

namespace FarmingGoap.Capabilities
{
    public class FarmingCapabilityFactory : CapabilityFactoryBase
    {
        public override ICapabilityConfig Create()
        {
            var builder = new CapabilityBuilder("FarmingCapability");

            // ========== GOALS - REDESIGNED ==========
            
            // GOAL: EatGoal - Priority 1 (Highest)
            builder.AddGoal<EatGoal>()
                .AddCondition<HungerLevel>(Comparison.SmallerThanOrEqual, 30)
                .AddCondition<HasFood>(Comparison.GreaterThanOrEqual, 1) // PENTING: Harus ada food
                .SetBaseCost(1);

            // GOAL: SleepGoal - Priority 2
            builder.AddGoal<SleepGoal>()
                .AddCondition<EnergyLevel>(Comparison.GreaterThanOrEqual, 80)
                .SetBaseCost(1);

            // GOAL: HarvestingGoal - Priority 3
            builder.AddGoal<HarvestingGoal>()
                .AddCondition<CropGrowthStage>(Comparison.SmallerThanOrEqual, 0)
                .AddCondition<HasFood>(Comparison.GreaterThanOrEqual, 1)
                .SetBaseCost(2);

            // GOAL: WateringGoal - Priority 4
            builder.AddGoal<WateringGoal>()
                .AddCondition<CropNeedsWater>(Comparison.SmallerThanOrEqual, 0)
                .SetBaseCost(3);

            // GOAL: PlantingGoal - Priority 5
            builder.AddGoal<PlantingGoal>()
                .AddCondition<CropGrowthStage>(Comparison.GreaterThanOrEqual, 1)
                .SetBaseCost(3);

            // GOAL: IdleGoal - Priority 6 (Lowest) - Fallback
            builder.AddGoal<IdleGoal>()
                .SetBaseCost(10); // Cost tinggi, hanya dipilih kalau tidak ada goal lain

            // ========== ACTIONS - REDESIGNED ==========

            // ACTION: GetSeedAction
            builder.AddAction<GetSeedAction>()
                .AddEffect<HasSeedKey>(EffectType.Increase)
                .SetTarget<SeedStorageTargetKey>()
                .SetBaseCost(1)
                .SetInRange(1f);

            // ACTION: GetShovelAction (Optional)
            builder.AddAction<GetShovelAction>()
                .AddEffect<HasShovelKey>(EffectType.Increase)
                .SetTarget<ShovelStorageTargetKey>()
                .SetBaseCost(2)
                .SetInRange(1f);

            // ACTION: PlantSeedAction
            builder.AddAction<PlantSeedAction>()
                .AddCondition<CropGrowthStage>(Comparison.SmallerThanOrEqual, 0) // Tanah kosong
                .AddCondition<HasSeedKey>(Comparison.GreaterThanOrEqual, 1) // Harus punya bibit
                .AddEffect<CropGrowthStage>(EffectType.Increase)
                .SetTarget<CropTarget>()
                .SetBaseCost(2) // Base cost, akan lebih mahal kalau tanpa sekop
                .SetInRange(1f);

            // ACTION: GetWateringCanAction
            builder.AddAction<GetWateringCanAction>()
                .AddEffect<HasWateringCanKey>(EffectType.Increase)
                .SetTarget<WaterSourceTargetKey>()
                .SetBaseCost(2)
                .SetInRange(1f);

            // ACTION: WaterCropAction
            builder.AddAction<WaterCropAction>()
                .AddCondition<CropGrowthStage>(Comparison.GreaterThanOrEqual, 1)
                .AddCondition<CropNeedsWater>(Comparison.GreaterThanOrEqual, 1)
                .AddCondition<HasWateringCanKey>(Comparison.GreaterThanOrEqual, 1) // Harus punya ember
                .AddEffect<CropGrowthStage>(EffectType.Increase)
                .AddEffect<CropNeedsWater>(EffectType.Decrease)
                .SetTarget<CropTarget>()
                .SetBaseCost(2)
                .SetInRange(1f);

            // ACTION: HarvestCropAction
            builder.AddAction<HarvestCropAction>()
                .AddCondition<CropGrowthStage>(Comparison.GreaterThanOrEqual, 3)
                .AddEffect<CropGrowthStage>(EffectType.Decrease)
                .AddEffect<HasFood>(EffectType.Increase)
                .SetTarget<CropTarget>()
                .SetBaseCost(3)
                .SetInRange(1f);

            // ACTION: EatAction
            builder.AddAction<EatAction>()
                .AddCondition<HasFood>(Comparison.GreaterThanOrEqual, 1)
                .AddEffect<HungerLevel>(EffectType.Decrease)
                .SetTarget<StorageTarget>()
                .SetBaseCost(1)
                .SetInRange(1f);

            // ACTION: SleepAction
            builder.AddAction<SleepAction>()
                .AddEffect<EnergyLevel>(EffectType.Increase)
                .SetTarget<BedTarget>()
                .SetBaseCost(1)
                .SetInRange(1f);

            // ACTION: WanderAction
            builder.AddAction<WanderAction>()
                .SetTarget<RandomWanderTargetKey>()
                .SetBaseCost(1)
                .SetInRange(0.5f); // Jarak dekat untuk wander

            // ========== SENSORS - World ==========
            builder.AddWorldSensor<HungerLevelSensor>()
                .SetKey<HungerLevel>();

            builder.AddWorldSensor<EnergyLevelSensor>()
                .SetKey<EnergyLevel>();

            builder.AddWorldSensor<CropGrowthStageSensor>()
                .SetKey<CropGrowthStage>();

            builder.AddWorldSensor<HasFoodSensor>()
                .SetKey<HasFood>();

            builder.AddWorldSensor<CropNeedsWaterSensor>()
                .SetKey<CropNeedsWater>();

            builder.AddWorldSensor<HasSeedSensor>()
                .SetKey<HasSeedKey>();

            builder.AddWorldSensor<HasWateringCanSensor>()
                .SetKey<HasWateringCanKey>();

            builder.AddWorldSensor<HasShovelSensor>()
                .SetKey<HasShovelKey>();

            // ========== SENSORS - Target ==========
            builder.AddTargetSensor<CropTargetSensor>()
                .SetTarget<CropTarget>();

            builder.AddTargetSensor<StorageTargetSensor>()
                .SetTarget<StorageTarget>();

            builder.AddTargetSensor<BedTargetSensor>()
                .SetTarget<BedTarget>();

            builder.AddTargetSensor<SeedStorageTargetSensor>()
                .SetTarget<SeedStorageTargetKey>();

            builder.AddTargetSensor<WaterSourceTargetSensor>()
                .SetTarget<WaterSourceTargetKey>();

            builder.AddTargetSensor<ShovelStorageTargetSensor>()
                .SetTarget<ShovelStorageTargetKey>();

            builder.AddTargetSensor<RandomWanderTargetSensor>()
                .SetTarget<RandomWanderTargetKey>();

            return builder.Build();
        }
    }
}
