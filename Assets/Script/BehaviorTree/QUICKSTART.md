# 🚀 Quick Start: Setup Behavior Tree

## Step-by-Step Setup (5 menit)

### 1️⃣ Buat Behavior Tree Asset
```
Project Panel → Right-click → Create → Behavior Designer → Behavior Tree
Nama: FarmerBehaviorTree
```

### 2️⃣ Buka Behavior Designer Editor
```
Top Menu → Window → Behavior Designer → Editor
```

### 3️⃣ Build Tree (Ikuti Gambar)

**Tree Structure:**
```
Root (Selector) [sudah ada]
├─ Survival (Sequence) [ADD]
│  ├─ SurvivalConditional [ADD]
│  └─ SelectSurvivalGoal [ADD]
└─ Farming (Sequence) [ADD]
   ├─ FarmingConditional [ADD]
   └─ SelectFarmingGoal [ADD]
```

**Cara ADD:**
1. **Klik Root** → Add Child → Composites → **Sequence** → Rename "Survival"
2. **Klik Root** → Add Child → Composites → **Sequence** → Rename "Farming"
3. **Klik Survival** → Add Child → GOAP → Survival → **SurvivalConditional**
4. **Klik Survival** → Add Child → GOAP → Survival → **SelectSurvivalGoal**
5. **Klik Farming** → Add Child → GOAP → Farming → **FarmingConditional**
6. **Klik Farming** → Add Child → GOAP → Farming → **SelectFarmingGoal**

### 4️⃣ Configure Parameters (PENTING!)

**Untuk SEMUA 4 tasks:**
1. Klik task di tree → Lihat Inspector
2. Cari field **`agentObject`**
3. Klik dropdown → Pilih **GameObject**
4. ✅ Enable **"Use Self"**

**SurvivalConditional:**
- hungerThreshold: 70
- energyThreshold: 30

**SelectSurvivalGoal:**
- eatThreshold: 70
- sleepThreshold: 30

**FarmingConditional:**
- alwaysAllow: True (sudah default)

**SelectFarmingGoal:**
- (tidak ada parameter tambahan)

### 5️⃣ Attach ke Agent GameObject

**Pilih Farmer di Hierarchy:**
```
Add Component → Behavior Tree

Settings:
- External Behavior: [Drag FarmerBehaviorTree asset]
- Update Interval: 0.2
- Restart When Complete: ✅ TRUE
```

### 6️⃣ Disable Code-Based Selection

**Pilih Farmer di Hierarchy** → Find component **FarmerBrain**:
```
☐ Use Code Based Selection (UNCHECK ini!)
```

**Atau** kalau mau test dua-duanya:
- ✅ Use Code Based Selection → Pakai kode FarmerBrain (hybrid utility)
- ☐ Use Code Based Selection → Pakai Behavior Tree

### 7️⃣ Test!

**Play Mode:**
1. Window → Behavior Designer → Editor
2. Pilih Farmer di Hierarchy
3. Lihat tree animasi:
   - **Hijau** = Success (node running)
   - **Merah** = Failure (kondisi tidak terpenuhi)
   - **Biru** = Sedang di-evaluate

**Console Logs:**
```
[BT] Survival Check: URGENT (Hunger=75, Energy=20)
[BT] Survival Planner: SleepGoal selected (Energy=20)
```

Atau:
```
[BT] Farming Utilities: Planting=-999.000, Watering=-999.000, Harvesting=0.386
[BT] Farming Planner: HarvestingGoal selected (U=0.386)
```

---

## 🎯 Decision Logic

### Survival Priority (Left Branch)
```
IF Hunger > 70 OR Energy < 30:
    IF Hunger > 70 AND FoodCount > 0:
        → EatGoal (Priority 1)
    ELSE IF Energy < 30:
        → SleepGoal (Priority 2)
```

### Farming Utility (Right Branch)
```
IF Survival Safe:
    Calculate utilities:
        Planting   = 0.4 - cost (if crop stage = 0)
        Watering   = 0.3 - cost (if crop stage = 1-2)
        Harvesting = 0.8 - cost (if crop stage = 3)
    
    → Pick MAX utility goal
```

---

## 🔧 Troubleshooting

### "Task not found in dropdown"
**Fix:** Recompile scripts
- Ctrl+R or wait for Unity auto-compile
- Close and reopen Behavior Designer Editor

### "NullReferenceException on agentObject"
**Fix:** Configure Use Self
- Klik task → Inspector
- agentObject → GameObject → ✅ Use Self

### "Tree tidak jalan (not executing)"
**Fix:** Check component settings
- Behavior Tree component enabled?
- External Behavior terisi?
- Update Interval > 0?

### "Selalu pilih Survival"
**Fix:** Adjust thresholds
- SurvivalConditional.hungerThreshold = 80 (lebih tinggi)
- SelectSurvivalGoal.eatThreshold = 80

### "Farming tidak jalan"
**Fix:** Check FarmerBrain toggle
- ☐ Use Code Based Selection (harus OFF!)
- Atau cek logs Console

---

## 📊 Comparison: Code vs Behavior Tree

| Feature | FarmerBrain (Code) | Behavior Tree |
|---------|-------------------|---------------|
| **Visual Editor** | ❌ Tidak | ✅ Visual nodes |
| **Easy Debug** | Console logs | ✅ Color-coded tree |
| **Performance** | ⚡ Cepat (every frame) | 🐢 Slower (0.2s interval) |
| **Flexibility** | Kode C# | Drag-drop tasks |
| **For Non-Programmer** | ❌ Sulit | ✅ Mudah |
| **Reusable** | Harus copy script | ✅ Asset reusable |

**Rekomendasi:**
- **Production:** Behavior Tree (visual debugging, easy tune)
- **Prototype:** FarmerBrain code (faster iteration)
- **Skripsi:** Behavior Tree (lebih impressive visual demo!)

---

## 🎓 Next Steps

1. ✅ Test kedua mode (toggle checkbox)
2. 📊 Compare performance (FPS, decision time)
3. 🎨 Screenshot tree untuk dokumentasi
4. 📝 Tulis di skripsi: "Hybrid BT+GOAP architecture"
5. 🚀 Extend dengan combat/social planner (easy!)
