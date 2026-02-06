# 🚀 Extending the System - Farming GOAP

Panduan untuk mengembangkan sistem GOAP farming Anda lebih lanjut.

---

## 📚 Table of Contents

1. [Menambah Goal & Action Baru](#menambah-goal--action-baru)
2. [Menambah WorldKey & Sensor](#menambah-worldkey--sensor)
3. [Multiple Crops](#multiple-crops)
4. [Multiple NPCs](#multiple-npcs)
5. [Global Sensors](#global-sensors)
6. [Upgrade ke Utility AI](#upgrade-ke-utility-ai)
7. [Visual & Audio Feedback](#visual--audio-feedback)

---

## 🎯 Menambah Goal & Action Baru

### Example: Menambah "RestGoal" (Istirahat Santai)

#### 1. Buat Goal
```csharp
// Goals/RestGoal.cs
using CrashKonijn.Goap.Core;

namespace FarmingGoap.Goals
{
    [GoapId("Farming-RestGoal")]
    public class RestGoal : GoalBase
    {
    }
}
```

#### 2. Buat Action
```csharp
// Actions/RestAction.cs
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using UnityEngine;

namespace FarmingGoap.Actions
{
    [GoapId("Farming-RestAction")]
    public class RestAction : GoapActionBase<RestAction.Data>
    {
        public override void Start(IMonoAgent agent, Data data)
        {
            data.Timer = 0f;
            Debug.Log("[RestAction] NPC istirahat santai...");
        }

        public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
        {
            data.Timer += context.DeltaTime;

            // Istirahat 5 detik
            if (data.Timer < 5f)
                return ActionRunState.Continue;

            // Restore energy sedikit
            var stats = data.Stats;
            if (stats != null)
            {
                stats.IncreaseEnergy(20f);
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
```

#### 3. Daftarkan di Capability
```csharp
// Capabilities/FarmingCapabilityFactory.cs
// Tambahkan di Create():

builder.AddGoal<RestGoal>()
    .AddCondition<EnergyLevel>(Comparison.GreaterThanOrEqual, 60) // Moderate energy
    .SetBaseCost(4);

builder.AddAction<RestAction>()
    .AddEffect<EnergyLevel>(EffectType.Increase)
    .SetTarget<BedTarget>() // Atau buat RestBenchTarget
    .SetBaseCost(2)
    .SetInRange(1f);
```

#### 4. Update Brain (Optional)
```csharp
// Brain/FarmerBrain.cs
// Tambahkan kondisi di SelectGoal():

// Priority 3.5: Rest if moderate energy
if (stats.Energy >= 40f && stats.Energy < 60f)
{
    provider.RequestGoal<RestGoal>();
    return;
}
```

---

## 🔑 Menambah WorldKey & Sensor

### Example: Menambah "Thirst" (Haus)

#### 1. Buat WorldKey
```csharp
// WorldKeys/ThirstLevel.cs
using CrashKonijn.Goap.Core;

namespace FarmingGoap.WorldKeys
{
    [GoapId("Farming-ThirstLevel")]
    public class ThirstLevel : WorldKeyBase
    {
    }
}
```

#### 2. Update NPCStats
```csharp
// Behaviours/NPCStats.cs
// Tambahkan field:

[SerializeField] private float thirst = 0f; // 0-100
public float Thirst => thirst;

// Tambahkan di Update():
thirst = Mathf.Min(100f, thirst + thirstIncreaseRate);

// Tambahkan methods:
public void DecreaseThirst(float amount)
{
    thirst = Mathf.Max(0f, thirst - amount);
}
```

#### 3. Buat Sensor
```csharp
// Sensors/World/ThirstLevelSensor.cs
using CrashKonijn.Agent.Core;
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;

namespace FarmingGoap.Sensors.World
{
    [GoapId("Farming-ThirstLevelSensor")]
    public class ThirstLevelSensor : LocalWorldSensorBase
    {
        public override SenseValue Sense(IActionReceiver agent, IComponentReference references)
        {
            var stats = references.GetCachedComponent<NPCStats>();
            return stats != null ? (int)stats.Thirst : 0;
        }
    }
}
```

#### 4. Daftarkan di Capability
```csharp
// Capabilities/FarmingCapabilityFactory.cs
builder.AddWorldSensor<ThirstLevelSensor>()
    .SetKey<ThirstLevel>();
```

#### 5. Buat Goal & Action (DrinkGoal, DrinkAction)
Similar dengan EatGoal/EatAction.

---

## 🌱 Multiple Crops

### Approach 1: Simple Array

#### Update CropBehaviour
```csharp
// Behaviours/CropBehaviour.cs
// Ganti single crop jadi array:

[SerializeField] private CropSlot[] cropSlots;

[System.Serializable]
public class CropSlot
{
    public Transform position;
    public int growthStage = 0;
    public SpriteRenderer sprite;
}

public int GetAvailableSlotIndex()
{
    for (int i = 0; i < cropSlots.Length; i++)
    {
        if (cropSlots[i].growthStage == 0)
            return i;
    }
    return -1; // All busy
}
```

#### Update Sensors
```csharp
// Sensors/Target/CropTargetSensor.cs
// Pilih crop terdekat yang kosong atau perlu attention:

public override ITarget Sense(...)
{
    var crop = references.GetCachedComponent<CropBehaviour>();
    
    // Find nearest crop that needs work
    int slotIndex = crop.GetSlotNeedingWork();
    
    if (slotIndex >= 0)
        return new TransformTarget(crop.cropSlots[slotIndex].position);
    
    return null;
}
```

### Approach 2: Separate GameObjects

#### Setup
1. Buat beberapa `Crop` GameObject di scene
2. Tag semua dengan "Crop"
3. Update sensor untuk find all

```csharp
// Sensors/Target/NearestEmptyCropSensor.cs
using UnityEngine;

public class NearestEmptyCropSensor : LocalTargetSensorBase
{
    private CropBehaviour[] allCrops;

    public override void Created()
    {
        allCrops = GameObject.FindObjectsOfType<CropBehaviour>();
    }

    public override ITarget Sense(...)
    {
        CropBehaviour nearestEmpty = null;
        float minDistance = float.MaxValue;

        foreach (var crop in allCrops)
        {
            if (crop.GrowthStage == 0) // Empty
            {
                float dist = Vector3.Distance(agent.Transform.position, crop.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    nearestEmpty = crop;
                }
            }
        }

        return nearestEmpty != null ? new TransformTarget(nearestEmpty.transform) : null;
    }
}
```

---

## 👥 Multiple NPCs

### Setup dengan Shared World State

#### 1. Buat Manager
```csharp
// Managers/FarmManager.cs
using UnityEngine;
using System.Collections.Generic;

public class FarmManager : MonoBehaviour
{
    public static FarmManager Instance;

    public List<CropBehaviour> allCrops = new List<CropBehaviour>();
    public Transform storage;
    public Transform[] beds;

    private void Awake()
    {
        Instance = this;
        
        // Find all crops in scene
        allCrops.AddRange(FindObjectsOfType<CropBehaviour>());
    }

    public CropBehaviour GetNearestEmptyCrop(Vector3 position)
    {
        CropBehaviour nearest = null;
        float minDist = float.MaxValue;

        foreach (var crop in allCrops)
        {
            if (crop.GrowthStage == 0)
            {
                float dist = Vector3.Distance(position, crop.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = crop;
                }
            }
        }

        return nearest;
    }
}
```

#### 2. Update Sensors
```csharp
// Sensors/Target/CropTargetSensor.cs
public override ITarget Sense(...)
{
    var nearestCrop = FarmManager.Instance.GetNearestEmptyCrop(agent.Transform.position);
    return nearestCrop != null ? new TransformTarget(nearestCrop.transform) : null;
}
```

#### 3. Spawn Multiple NPCs
```csharp
// Spawner/NPCSpawner.cs
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [SerializeField] private GameObject farmerPrefab;
    [SerializeField] private int npcCount = 3;
    [SerializeField] private Vector2 spawnArea = new Vector2(10, 10);

    private void Start()
    {
        for (int i = 0; i < npcCount; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-spawnArea.x, spawnArea.x),
                Random.Range(-spawnArea.y, spawnArea.y),
                0
            );

            Instantiate(farmerPrefab, randomPos, Quaternion.identity);
        }
    }
}
```

---

## 🌍 Global Sensors

### Example: IsDaytime Sensor

#### 1. Buat WorldKey
```csharp
// WorldKeys/IsDaytime.cs
using CrashKonijn.Goap.Core;

namespace FarmingGoap.WorldKeys
{
    [GoapId("Farming-IsDaytime")]
    public class IsDaytime : WorldKeyBase
    {
    }
}
```

#### 2. Buat Global Sensor
```csharp
// Sensors/World/IsDaytimeSensor.cs
using CrashKonijn.Goap.Core;
using CrashKonijn.Goap.Runtime;
using UnityEngine;

namespace FarmingGoap.Sensors.World
{
    [GoapId("Farming-IsDaytimeSensor")]
    public class IsDaytimeSensor : GlobalWorldSensorBase
    {
        public override void Created()
        {
        }

        public override void Update()
        {
        }

        public override SenseValue Sense()
        {
            // Check time of day (example: 6am-6pm = day)
            float timeOfDay = TimeManager.Instance.CurrentHour; // 0-24
            return (timeOfDay >= 6f && timeOfDay < 18f) ? 1 : 0;
        }
    }
}
```

#### 3. Gunakan di Conditions
```csharp
// Capabilities/FarmingCapabilityFactory.cs
builder.AddAction<PlantCropAction>()
    .AddCondition<IsDaytime>(Comparison.GreaterThanOrEqual, 1) // Only plant during day
    .AddEffect<CropGrowthStage>(EffectType.Increase)
    .SetTarget<CropTarget>()
    .SetBaseCost(2);
```

---

## 🧠 Upgrade ke Utility AI

Replace priority-based brain dengan utility scoring:

```csharp
// Brain/UtilityFarmerBrain.cs
using UnityEngine;
using CrashKonijn.Agent.Runtime;
using CrashKonijn.Goap.Runtime;
using FarmingGoap.Behaviours;
using FarmingGoap.Goals;
using System.Collections.Generic;

namespace FarmingGoap.Brain
{
    public class UtilityFarmerBrain : MonoBehaviour
    {
        private GoapActionProvider provider;
        private NPCStats stats;
        private CropBehaviour crop;

        private void Awake()
        {
            provider = GetComponent<GoapActionProvider>();
            stats = GetComponent<NPCStats>();
            crop = GetComponent<CropBehaviour>();
        }

        private void Update()
        {
            if (Time.frameCount % 60 == 0)
            {
                SelectGoalByUtility();
            }
        }

        private void SelectGoalByUtility()
        {
            var scores = new Dictionary<System.Type, float>();

            // Score EatGoal
            float hungerUrgency = Mathf.InverseLerp(0, 100, stats.Hunger);
            float hasFood = stats.FoodCount > 0 ? 1f : 0f;
            scores[typeof(EatGoal)] = hungerUrgency * hasFood * 10f;

            // Score SleepGoal
            float energyUrgency = 1f - Mathf.InverseLerp(0, 100, stats.Energy);
            scores[typeof(SleepGoal)] = energyUrgency * 8f;

            // Score HarvestGoal
            float cropReady = crop.GrowthStage == 3 ? 1f : 0f;
            scores[typeof(HarvestingGoal)] = cropReady * 7f;

            // Score WaterGoal
            float needsWater = (crop.GrowthStage >= 1 && crop.GrowthStage < 3) ? 1f : 0f;
            scores[typeof(WateringGoal)] = needsWater * 5f;

            // Score PlantGoal
            float isEmpty = crop.GrowthStage == 0 ? 1f : 0f;
            scores[typeof(PlantingGoal)] = isEmpty * 3f;

            // Select highest score
            System.Type bestGoal = null;
            float bestScore = 0f;

            foreach (var kvp in scores)
            {
                if (kvp.Value > bestScore)
                {
                    bestScore = kvp.Value;
                    bestGoal = kvp.Key;
                }
            }

            // Request goal
            if (bestGoal == typeof(EatGoal))
                provider.RequestGoal<EatGoal>();
            else if (bestGoal == typeof(SleepGoal))
                provider.RequestGoal<SleepGoal>();
            else if (bestGoal == typeof(HarvestingGoal))
                provider.RequestGoal<HarvestingGoal>();
            else if (bestGoal == typeof(WateringGoal))
                provider.RequestGoal<WateringGoal>();
            else
                provider.RequestGoal<PlantingGoal>();
        }
    }
}
```

---

## 🎨 Visual & Audio Feedback

### Particle Effects

```csharp
// Actions/WaterCropAction.cs - Update Perform():
public override IActionRunState Perform(IMonoAgent agent, Data data, IActionContext context)
{
    // ... existing code ...

    // Add particle effect
    if (!data.ParticleSpawned)
    {
        var particle = ParticlePool.Instance.Get("WaterSplash");
        particle.transform.position = data.Target.Position;
        particle.Play();
        data.ParticleSpawned = true;
    }

    // ... rest of code ...
}

// Update Data class:
public class Data : IActionData
{
    // ... existing fields ...
    public bool ParticleSpawned { get; set; }
}
```

### Audio Feedback

```csharp
// Add to Actions:
[SerializeField] private AudioClip actionSound;

public override void Start(IMonoAgent agent, Data data)
{
    AudioManager.Instance.PlaySFX(actionSound, agent.Transform.position);
    // ... rest of code ...
}
```

### Animation

```csharp
// Actions/PlantCropAction.cs
public override void Start(IMonoAgent agent, Data data)
{
    var animator = agent.GetComponent<Animator>();
    if (animator != null)
    {
        animator.SetTrigger("Plant");
    }
}
```

---

## 📊 Performance Tips

### 1. Object Pooling untuk Crops
```csharp
public class CropPool : MonoBehaviour
{
    private Queue<CropBehaviour> pool = new Queue<CropBehaviour>();
    
    public CropBehaviour Get()
    {
        return pool.Count > 0 ? pool.Dequeue() : CreateNew();
    }
    
    public void Return(CropBehaviour crop)
    {
        crop.gameObject.SetActive(false);
        pool.Enqueue(crop);
    }
}
```

### 2. Spatial Hashing untuk Crop Search
```csharp
public class SpatialGrid
{
    private Dictionary<Vector2Int, List<CropBehaviour>> grid;
    private float cellSize = 5f;
    
    public List<CropBehaviour> GetNearbyCrops(Vector3 position)
    {
        Vector2Int cell = WorldToCell(position);
        return grid.ContainsKey(cell) ? grid[cell] : new List<CropBehaviour>();
    }
}
```

### 3. Reduce Brain Update Frequency
```csharp
// Update setiap 2 detik instead of 1
if (Time.frameCount % 120 == 0) // 60 fps * 2 seconds
{
    SelectGoal();
}
```

---

## 🎓 Advanced Topics

### State Machines untuk Complex Behaviors
Combine GOAP dengan state machine untuk granular control.

### Blackboard System
Share data antar actions tanpa coupling.

### Dynamic Action Cost
Adjust cost based on distance, resources, etc.

### Interruption Handling
Allow high-priority goals to interrupt current action.

---

**💡 Mulai dari yang sederhana, lalu ekspansi bertahap!**
