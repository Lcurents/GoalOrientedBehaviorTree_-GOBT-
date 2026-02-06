# 🌾 Farming GOAP System

Sistem GOAP (Goal-Oriented Action Planning) untuk NPC farming di Unity menggunakan package **Crashkonijn GOAP**.

---

## 📋 Deskripsi

NPC farmer yang dapat:
- 🌱 **Menanam** (Planting) tanaman
- 💧 **Menyiram** (Watering) tanaman untuk pertumbuhan
- 🌾 **Memanen** (Harvesting) hasil panen
- 🍎 **Makan** (Eat) untuk mengurangi hunger
- 😴 **Tidur** (Sleep) untuk restore energy

NPC memiliki atribut **Hunger** dan **Energy** yang berubah pasif seiring waktu, dan akan otomatis memilih goal berdasarkan prioritas kebutuhan.

---

## 📁 Struktur File

```
Farming/
├── WorldKeys/              # 5 keys - HungerLevel, EnergyLevel, CropGrowthStage, HasFood, CropNeedsWater
├── TargetKeys/             # 4 keys - CropTarget, StorageTarget, BedTarget, WaterSourceTarget
├── Goals/                  # 5 goals - Planting, Watering, Harvesting, Eat, Sleep
├── Actions/                # 5 actions - PlantCropAction, WaterCropAction, HarvestCropAction, EatAction, SleepAction
├── Sensors/
│   ├── World/             # 5 sensors - Membaca world state (hunger, energy, crop, food)
│   └── Target/            # 3 sensors - Menemukan posisi target (crop, storage, bed)
├── Capabilities/           # FarmingCapabilityFactory - Konfigurasi GOAP
├── AgentTypes/             # FarmerAgentTypeFactory - Agent type definition
├── Brain/                  # FarmerBrain - Priority-based goal selector
├── Behaviours/             # NPCStats, CropBehaviour - Helper components
├── Debug/                  # NPCDebugDisplay - Inspector debugging
├── Testing/                # NPCTestController - Manual testing tools
│
├── README.md              # 👈 File ini
├── SETUP_GUIDE.md         # 📖 Panduan setup lengkap di Unity
├── DESIGN_DOCUMENT.md     # 📊 Dokumentasi desain sistem
└── QUICK_REFERENCE.md     # ⚡ Quick reference & troubleshooting
```

---

## 🚀 Quick Start

### 1. Baca Dokumentasi
Pilih sesuai kebutuhan:
- **Baru mulai?** → Baca [SETUP_GUIDE.md](SETUP_GUIDE.md)
- **Mau paham sistem?** → Baca [DESIGN_DOCUMENT.md](DESIGN_DOCUMENT.md)
- **Cari referensi cepat?** → Baca [QUICK_REFERENCE.md](QUICK_REFERENCE.md)

### 2. Setup di Unity Scene
Ikuti step-by-step di [SETUP_GUIDE.md](SETUP_GUIDE.md):
1. Buat GoapRunner
2. Buat Farmer NPC
3. Setup Crop, Bed, Storage
4. Test!

### 3. Test & Debug
- Gunakan `NPCDebugDisplay` untuk monitor stats real-time
- Gunakan `NPCTestController` untuk test scenarios
- Lihat Console logs untuk track actions

---

## 🎯 Cara Kerja

### Priority-Based Goal Selection
NPC memilih goal berdasarkan prioritas:

```
1. Hunger > 70 → EatGoal (jika punya food)
2. Energy < 30 → SleepGoal
3. Crop matang (stage 3) → HarvestingGoal
4. Crop perlu air (stage 1-2) → WateringGoal
5. Crop kosong (stage 0) → PlantingGoal
```

### Action Flow
Setiap goal diresolve ke action melalui GOAP planner:

```
PlantingGoal
    └→ PlantCropAction (2s)
        └→ Crop stage 0 → 1

WateringGoal
    └→ WaterCropAction (1.5s)
        └→ Crop stage +1 (max 3)

HarvestingGoal
    └→ HarvestCropAction (2s)
        └→ Crop → 0, Food +1

EatGoal
    └→ EatAction (1s)
        └→ Hunger -50, Food -1

SleepGoal
    └→ SleepAction (~3s)
        └→ Energy → 80+
```

---

## 📊 Components

### Required (dari Crashkonijn)
- `AgentBehaviour` - Core GOAP agent
- `GoapActionProvider` - Manages goals/actions
- `ActionReceiver` - Receives commands
- `AgentMoveBehaviour` - Movement controller

### Custom Scripts
- `NPCStats` - Hunger, Energy, Food tracking
- `CropBehaviour` - Crop growth state
- `FarmerBrain` - Goal selection AI

### Optional
- `NPCDebugDisplay` - Inspector debugging
- `NPCTestController` - Manual testing

---

## 🎮 Testing

### Inspector Testing (Play Mode)
1. Add `NPCTestController` ke Farmer
2. Toggle checkboxes untuk test:
   - `setHungry` - Force hunger to 100
   - `setTired` - Force energy to 0
   - `setCropReady` - Force crop to stage 3
   - `resetStats` - Reset semua stats

### Keyboard Shortcuts (Play Mode)
- `H` - Set Hungry
- `E` - Set Tired
- `C` - Set Crop Ready
- `R` - Reset Stats
- `F` - Add 5 Food

### Console Logs
Setiap action menampilkan log progress:
```
[PlantCropAction] Tanaman ditanam!
[WaterCropAction] Tanaman disiram! Stage: 2
[HarvestCropAction] Panen berhasil! Food: 1
```

---

## ⚙️ Configuration

### Tuning Parameters

**Brain Thresholds** ([FarmerBrain.cs](Brain/FarmerBrain.cs)):
```csharp
stats.Hunger > 70f    // Trigger eat
stats.Energy < 30f    // Trigger sleep
```

**Passive Rates** (NPCStats Inspector):
```
hungerIncreaseRate = 5f    // +5 per 10 detik
energyDecreaseRate = 3f    // -3 per 10 detik
```

**Action Costs** ([FarmingCapabilityFactory.cs](Capabilities/FarmingCapabilityFactory.cs)):
```csharp
.SetBaseCost(2)    // Ubah di builder
```

---

## 🐛 Troubleshooting

| Problem | Solution |
|---------|----------|
| NPC tidak bergerak | Cek `AgentMoveBehaviour` + `Rigidbody2D` |
| NullReferenceException | Cek semua component terpasang |
| Goal tidak resolve | Cek `FarmerAgentTypeFactory` di GoapRunner |
| Crop tidak grow | Assign 4 sprites di `CropBehaviour` |

Detail lengkap di [QUICK_REFERENCE.md](QUICK_REFERENCE.md)

---

## 📚 Dokumentasi Lengkap

1. **[SETUP_GUIDE.md](SETUP_GUIDE.md)** - Step-by-step Unity setup
2. **[DESIGN_DOCUMENT.md](DESIGN_DOCUMENT.md)** - System architecture & flow
3. **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)** - Quick reference & troubleshooting

---

## 🚀 Next Steps

### Enhancement Ideas
1. ✅ Basic farming cycle (DONE)
2. 🔲 Multiple crops per NPC
3. 🔲 Dynamic crop selection (nearest)
4. 🔲 More resource types (wood, stone)
5. 🔲 Multiple NPCs dengan shared world state
6. 🔲 Building construction
7. 🔲 Day/night cycle
8. 🔲 NPC social interactions

### Learning Path
- **Day 1**: Setup & test basic system
- **Day 2**: Customize & tune parameters
- **Day 3**: Add visual/audio feedback
- **Day 4**: Scale to multiple NPCs

---

## 📝 Technical Details

### GOAP Implementation
- **Package**: Crashkonijn GOAP v3.1+
- **Config Method**: Code-based (via CapabilityFactory)
- **Goal Selection**: Priority-based (FarmerBrain)
- **Sensor Scope**: Local (per-agent)

### Performance
- **Goals**: 5
- **Actions**: 5
- **Sensors**: 8 (5 world + 3 target)
- **Update Rate**: Brain checks every 60 frames (~1 second)

---

## 👨‍💻 Author

Dibuat untuk project skripsi menggunakan:
- Unity 2022.2+
- Crashkonijn GOAP Package v3.1
- Cute Fantasy Asset Pack

---

## 📄 License

Free to use untuk project skripsi/akademik.

---

**🎮 Selamat mencoba! Jika ada pertanyaan, cek dokumentasi atau console logs untuk debugging.**
