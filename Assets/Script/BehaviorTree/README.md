# Behavior Tree + GOAP Integration

## 📋 Setup Guide

### 1. Install Behavior Designer
- Import **Opsive Behavior Designer** dari Package Manager
- Pastikan package sudah di-import ke project

### 2. Buat Behavior Tree Asset
1. **Buat Behavior Tree:**
   - Klik kanan di Project → Create → Behavior Designer → Behavior Tree
   - Nama: `FarmerBehaviorTree`

2. **Buka Behavior Designer Editor:**
   - Window → Behavior Designer → Editor
   - Atau double-click asset `FarmerBehaviorTree`

### 3. Setup Tree Structure

#### Root Node (Selector)
- Default root adalah **Selector** → bagus!
- Selector = coba child kiri, jika gagal coba kanan

#### Left Branch: Survival (Sequence)
1. Add **Sequence** sebagai child pertama
2. Di dalam Sequence, tambahkan:
   - **SurvivalConditional** (Conditionals)
   - **SelectSurvivalGoal** (Actions)

#### Right Branch: Farming (Sequence)
1. Add **Sequence** sebagai child kedua
2. Di dalam Sequence, tambahkan:
   - **FarmingConditional** (Conditionals)
   - **SelectFarmingGoal** (Actions)

### 4. Configure Task Parameters

#### SurvivalConditional
- **agentObject**: Set ke `GameObject` (self reference)
- **hungerThreshold**: 70
- **energyThreshold**: 30

#### SelectSurvivalGoal
- **agentObject**: Set ke `GameObject` (self reference)
- **eatThreshold**: 70
- **sleepThreshold**: 30

#### FarmingConditional
- **alwaysAllow**: True (default)

#### SelectFarmingGoal
- **agentObject**: Set ke `GameObject` (self reference)

### 5. Attach ke Agent GameObject

1. Pilih **Farmer agent** di Hierarchy
2. Add Component → **Behavior Tree**
3. Di component Behavior Tree:
   - **Behavior**: Drag asset `FarmerBehaviorTree`
   - **Update Interval**: 0.2 (update setiap 0.2 detik)
   - **Restart When Complete**: True

### 6. Disable FarmerBrain (Opsional)

Karena sekarang Behavior Tree yang handle goal selection:
- Disable component `FarmerBrain` di agent
- ATAU hapus method `SelectGoal()` dari Update loop

### 7. Test

1. Play mode
2. Buka Window → Behavior Designer → Editor
3. Pilih agent di Hierarchy
4. Lihat visualisasi tree:
   - **Hijau** = Success
   - **Merah** = Failure
   - **Biru** = Running

## 🌳 Tree Logic

```
Root (Selector)                    ← Coba kiri dulu
├── Survival (Sequence)            ← Jika urgent
│   ├── SurvivalConditional        ← Hunger > 70 OR Energy < 30?
│   └── SelectSurvivalGoal         ← Eat > Sleep (priority)
└── Farming (Sequence)             ← Jika survival aman
    ├── FarmingConditional         ← Always true
    └── SelectFarmingGoal          ← Max utility (Planting/Watering/Harvesting)
```

### Decision Flow:
1. **Cek Survival** → Jika urgent (Hunger > 70 OR Energy < 30):
   - SelectSurvivalGoal → Priority: Eat > Sleep
   - Tree STOP (Selector sukses di kiri)

2. **Jika Survival Aman** → Selector coba kanan:
   - FarmingConditional → Always true
   - SelectFarmingGoal → Hitung utilities, pilih max

3. **Goal Selected** → GOAP planner handle execution

## 🎯 Advantages

### Behavior Tree:
- **Visual editor** untuk non-programmer
- **Reusable tasks** untuk multiple agents
- **Easy debugging** dengan color-coded nodes
- **Clear hierarchy** survival > farming

### GOAP:
- **Action planning** tetap automatic
- **Pathfinding** automatic (sensors)
- **State management** handled by conditions/effects

## 🔧 Customization

### Adjust Thresholds:
- Edit `SurvivalConditional.hungerThreshold`
- Edit `SelectSurvivalGoal.eatThreshold`

### Add New Planners:
1. Buat new Conditional (e.g., `CombatConditional`)
2. Buat new Action (e.g., `SelectCombatGoal`)
3. Add sebagai child ke-3 di Root Selector

### Change Priority:
- **Survival > Combat > Farming:**
  - Reorder children di Selector (kiri = priority tinggi)

## ⚠️ Troubleshooting

**"Task not found":**
- Rebuild scripts (Ctrl+R)
- Reimport Behavior Designer package

**"NullReferenceException":**
- Pastikan `agentObject` terisi di Inspector
- Cek apakah agent punya NPCStats + GoapActionProvider

**"Tree tidak jalan":**
- Pastikan Behavior Tree component enabled
- Cek Update Interval > 0
- Cek External Behavior terisi

**"Selalu pilih Survival":**
- Cek threshold terlalu rendah
- Debug log di SurvivalConditional

## 📝 Notes

- **Behavior Tree runs @ 0.2s interval** → tidak setiap frame
- **GOAP planner runs continuously** → actual action execution
- **Separation of concerns:** BT = strategy, GOAP = tactics
