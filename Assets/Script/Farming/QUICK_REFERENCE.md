# ⚡ Quick Reference - Farming GOAP

## 📦 File Structure
```
Assets/Script/Farming/
├── WorldKeys/              # 5 keys - Status dunia
├── TargetKeys/             # 4 keys - Lokasi target
├── Goals/                  # 5 goals - Tujuan NPC
├── Actions/                # 5 actions - Aksi yang bisa dilakukan
├── Sensors/
│   ├── World/             # 5 sensors - Baca world state
│   └── Target/            # 3 sensors - Temukan target
├── Capabilities/           # Factory untuk config GOAP
├── AgentTypes/             # Factory untuk agent type
├── Brain/                  # AI decision maker
├── Behaviours/             # Helper components (Stats, Crop)
├── Debug/                  # Debug helper
├── SETUP_GUIDE.md         # 👈 Panduan setup lengkap
└── DESIGN_DOCUMENT.md     # 👈 Desain sistem detail
```

---

## 🎯 Core Concepts

### Goals (Apa yang ingin dicapai)
- `PlantingGoal` - Tanam tanaman
- `WateringGoal` - Siram tanaman
- `HarvestingGoal` - Panen tanaman
- `EatGoal` - Makan untuk mengurangi hunger
- `SleepGoal` - Tidur untuk restore energy

### Actions (Cara mencapai goal)
- `PlantCropAction` - Tanam benih (2s)
- `WaterCropAction` - Siram tanaman (1.5s)
- `HarvestCropAction` - Panen hasil (2s)
- `EatAction` - Makan makanan (1s)
- `SleepAction` - Tidur hingga energy > 80 (~3s)

### WorldKeys (Status yang dipantau)
- `HungerLevel` (0-100)
- `EnergyLevel` (0-100)
- `CropGrowthStage` (0-3)
- `HasFood` (0/1)
- `CropNeedsWater` (0/1)

---

## 🛠️ Setup Checklist

### Scene Setup (10 menit)
- [ ] 1. Buat GoapRunner + FarmerAgentTypeFactory
- [ ] 2. Buat Farmer GameObject
- [ ] 3. Add 7 Crashkonijn components
- [ ] 4. Add 3 custom scripts (NPCStats, CropBehaviour, FarmerBrain)
- [ ] 5. Buat Crop child object
- [ ] 6. Buat Bed + Storage GameObjects
- [ ] 7. Setup Tags (Bed, Storage)
- [ ] 8. Test Play!

### Required Components (Farmer GameObject)
```
✓ AgentBehaviour
✓ GoapActionProvider
✓ ActionReceiver
✓ AgentMoveBehaviour
✓ Rigidbody2D
✓ Collider2D
✓ NPCStats
✓ CropBehaviour
✓ FarmerBrain
```

---

## 🐛 Common Issues & Fixes

| Problem | Solution |
|---------|----------|
| NPC tidak bergerak | Cek AgentMoveBehaviour terpasang + Rigidbody2D dynamic |
| NullReferenceException | Pastikan semua component terpasang di Farmer |
| Goal tidak resolve | Cek GoapRunner → FarmerAgentTypeFactory registered |
| Crop tidak grow | Assign growthSprites (4 sprites) di CropBehaviour |
| Tag not found | Buat tag "Bed" dan "Storage" di Project Settings |

---

## 📊 Stats Reference

### NPCStats Default Values
```csharp
Hunger = 0f          // Increase +5 every 10s
Energy = 100f        // Decrease -3 every 10s
FoodCount = 0        // Manual (from harvest/eat)
```

### CropBehaviour Growth Stages
```
0 = Empty (tanah kosong)
1 = Planted (baru ditanam)
2 = Growing (sedang tumbuh)
3 = Ready (siap panen)
```

---

## 🎮 Testing Scenarios

### Test 1: Normal Cycle
1. Play → NPC plant crop
2. Wait → NPC water twice
3. Wait → NPC harvest (Food +1)
4. Loop

### Test 2: Hunger Trigger
1. Inspector → Set Hunger = 75
2. NPC should harvest first (if crop ready)
3. Then eat

### Test 3: Energy Depletion
1. Inspector → Set Energy = 20
2. NPC should sleep
3. Energy restores to 80+

---

## 🔍 Debug Commands

### View Current State (Inspector)
Add `NPCDebugDisplay` component untuk real-time stats

### Console Logs
Setiap action menampilkan log:
```
[PlantCropAction] Tanaman ditanam!
[WaterCropAction] Tanaman disiram! Stage: 2
[HarvestCropAction] Panen berhasil! Food: 1
[EatAction] Makan! Hunger: 20, Food: 0
[SleepAction] NPC mulai tidur...
```

### GOAP Graph Viewer
`Window → GOAP → Graph Viewer`
- Pilih Farmer
- Lihat real-time action graph
- Lihat world state values

---

## ⚙️ Tuning Parameters

### Brain Decision (FarmerBrain.cs)
```csharp
if (stats.Hunger > 70f)        // Ubah threshold hunger
if (stats.Energy < 30f)        // Ubah threshold energy
```

### Stats Rate (NPCStats Inspector)
```
hungerIncreaseRate = 5f        // Lapar lebih cepat
energyDecreaseRate = 3f        // Lelah lebih cepat
```

### Action Cost (FarmingCapabilityFactory.cs)
```csharp
.SetBaseCost(2)                // Ubah di builder
```

### Action Duration (Actions/*.cs)
```csharp
if (data.Timer < 2f)           // Ubah di Perform()
```

---

## 📝 Code Snippets

### Manual Goal Request
```csharp
var provider = GetComponent<GoapActionProvider>();
provider.RequestGoal<HarvestingGoal>();
```

### Check Current Goal
```csharp
var currentGoal = provider.CurrentGoal?.GetType().Name;
Debug.Log($"Current Goal: {currentGoal}");
```

### Modify Stats at Runtime
```csharp
var stats = GetComponent<NPCStats>();
stats.IncreaseHunger(50);
stats.DecreaseEnergy(30);
stats.AddFood(5);
```

### Force Crop Growth
```csharp
var crop = GetComponent<CropBehaviour>();
crop.SetGrowthStage(3); // Langsung matang
```

---

## 🚀 Next Steps After Basic Works

### Easy Enhancements
1. Add particle effects saat water/harvest
2. Add sound effects untuk setiap action
3. Add UI bar untuk Hunger/Energy
4. Add animation state changes

### Medium Enhancements
1. Multiple crops (array of CropBehaviour)
2. NPC pilih crop terdekat yang perlu attention
3. Add more food types
4. Add cooking system

### Advanced Enhancements
1. Multiple NPCs dengan shared world state
2. NPC trading resources
3. Building construction
4. Day/night cycle influence behavior

---

## 📚 Documentation Links

- **Setup Guide**: `SETUP_GUIDE.md` - Step-by-step Unity setup
- **Design Doc**: `DESIGN_DOCUMENT.md` - System architecture
- **Crashkonijn Docs**: `Assets/Script/Documentation/` - Package reference

---

## 🎯 Learning Path

### Day 1: Basic Understanding
- Read SETUP_GUIDE.md
- Setup 1 NPC dengan 5 goals
- Test semua scenarios

### Day 2: Customization
- Modify stats rates
- Change action durations
- Tune brain thresholds
- Add visual feedback

### Day 3: Enhancement
- Add multiple crops
- Implement crop selection logic
- Add UI elements
- Add particle/sound effects

### Day 4: Scale
- Add multiple NPCs
- Implement shared sensors
- Add resource competition
- Add social interactions

---

**🎮 Selamat belajar GOAP! Jika ada error, cek SETUP_GUIDE.md bagian Troubleshooting.**
