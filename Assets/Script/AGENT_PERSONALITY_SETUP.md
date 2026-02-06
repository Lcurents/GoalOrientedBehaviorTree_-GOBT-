# 🎭 Agent Personality Configuration Guide

## Overview

Setiap agent sekarang memiliki **personality** yang berbeda melalui **utility weights** yang dapat diatur di Inspector!

### Formula Utility:
```
U(goal) = (1.0 × GoalBenefit) - (WeightEnergy × EnergyCost/100) - (WeightHunger × HungerCost/100)
```

---

## 📊 Setup untuk 3 Agents (Sesuai Gambar)

### **Agent A - "Harvester Specialist"**
```
Inspector → NPCStats:
├─ Stats:
│  ├─ Hunger: 20
│  └─ Energy: 80
│
└─ Utility Weights - Agent Personality:
   ├─ Weight Energy: 0.2
   ├─ Weight Hunger: 0.1
   ├─ Goal Benefit Planting: 0.4
   ├─ Goal Benefit Watering: 0.3
   └─ Goal Benefit Harvesting: 0.8 ⭐ (HIGHEST!)
```

**Karakteristik:**
- ⭐ **Harvesting Expert** (benefit 0.8)
- Low hunger sensitivity (0.1)
- Moderate energy sensitivity (0.2)
- **Best for:** Panen crops yang sudah matang

---

### **Agent B - "Planting Specialist"**
```
Inspector → NPCStats:
├─ Stats:
│  ├─ Hunger: 40
│  └─ Energy: 60
│
└─ Utility Weights - Agent Personality:
   ├─ Weight Energy: 0.1
   ├─ Weight Hunger: 0.6 ⚠️ (High hunger sensitivity!)
   ├─ Goal Benefit Planting: 0.7 ⭐ (HIGHEST!)
   ├─ Goal Benefit Watering: 0.2
   └─ Goal Benefit Harvesting: 0.1
```

**Karakteristik:**
- ⭐ **Planting Expert** (benefit 0.7)
- Very high hunger sensitivity (0.6) - Akan sering makan!
- Low energy sensitivity (0.1)
- **Best for:** Tanam bibit baru di lahan kosong

---

### **Agent C - "Balanced Worker"**
```
Inspector → NPCStats:
├─ Stats:
│  ├─ Hunger: 50
│  └─ Energy: 50
│
└─ Utility Weights - Agent Personality:
   ├─ Weight Energy: 0.3
   ├─ Weight Hunger: 0.2
   ├─ Goal Benefit Planting: 0.5
   ├─ Goal Benefit Watering: 0.3
   └─ Goal Benefit Harvesting: 0.6
```

**Karakteristik:**
- 🔄 **Balanced worker** - Bisa semua task
- Moderate energy sensitivity (0.3)
- Moderate hunger sensitivity (0.2)
- **Best for:** Harvesting (0.6) dan secondary Planting (0.5)

---

## 🎯 Expected Behavior

### Scenario 1: 3 Empty Crops (All Stage 0)

**Planting Utilities:**
```
Agent A: U = (1.0 × 0.4) - penalties = ~0.38
Agent B: U = (1.0 × 0.7) - penalties = ~0.60 ⭐ WINS!
Agent C: U = (1.0 × 0.5) - penalties = ~0.45
```

**Result:** Agent B (planting specialist) wins the planting task!

### Scenario 2: 1 Crop Ready to Harvest (Stage 3)

**Harvesting Utilities:**
```
Agent A: U = (1.0 × 0.8) - penalties = ~0.76 ⭐ WINS!
Agent B: U = (1.0 × 0.1) - penalties = ~0.08
Agent C: U = (1.0 × 0.6) - penalties = ~0.56
```

**Result:** Agent A (harvester specialist) gets the harvest task!

### Scenario 3: Mixed Crops

```
Crop 1: Stage 0 (empty)
Crop 2: Stage 1 (needs water)
Crop 3: Stage 3 (ready harvest)

Expected Distribution:
- Agent A → Crop 3 (Harvesting, highest U for Agent A)
- Agent B → Crop 1 (Planting, highest U for Agent B)
- Agent C → Crop 2 (Watering, only available task)
```

**Emergent specialization without hardcoding!** 🎉

---

## 🔬 Testing Tips

### Test 1: Planting Competition
1. Create 3 empty crops (stage 0)
2. Start Play mode
3. **Expected:** Agent B should bid highest for planting
4. **Console:**
   ```
   [Bid] Agent B → Crop X (U=0.60, Planting)
   [Bid] Agent A → Crop Y (U=0.38, Planting)
   [Bid] Agent C → Crop Z (U=0.45, Planting)
   ```

### Test 2: Harvesting Competition
1. Manually set all crops to stage 3
2. Start Play mode
3. **Expected:** Agent A dominates harvesting
4. **Console:**
   ```
   [Bid] Agent A → Crop X (U=0.76, Harvesting) ⭐
   [Bid] Agent B → Same crop (U=0.08)
   [Auction] Crop X: Agent A WINS
   ```

### Test 3: Hunger Behavior
1. Watch Agent B (high hunger sensitivity)
2. As Hunger increases → Agent B switches to Survival faster than others
3. **Expected:**
   ```
   [Agent B] Hunger=65 → Survival mode (WH=0.6 makes it sensitive!)
   [Agent A] Hunger=75 → Still farming (WH=0.1)
   ```

---

## 📐 Utility Calculation Examples

### Agent A wants to Plant (Empty Crop):
```
GoalBenefit = 0.4
EnergyCost = 8 (planting constant)
HungerCost = 5 (planting constant)

U = (1.0 × 0.4) - (0.2 × 8/100) - (0.1 × 5/100)
U = 0.4 - 0.016 - 0.005
U = 0.379
```

### Agent B wants to Plant (Same Crop):
```
GoalBenefit = 0.7 (higher preference!)

U = (1.0 × 0.7) - (0.1 × 8/100) - (0.6 × 5/100)
U = 0.7 - 0.008 - 0.030
U = 0.662 ⭐ (Winner!)
```

### Agent A wants to Harvest:
```
GoalBenefit = 0.8 (highest!)
EnergyCost = 10 (harvest constant)
HungerCost = 8 (harvest constant)

U = (1.0 × 0.8) - (0.2 × 10/100) - (0.1 × 8/100)
U = 0.8 - 0.020 - 0.008
U = 0.772 ⭐ (Harvesting champion!)
```

---

## 🎓 For Skripsi

### Research Contributions:

1. **Agent Heterogeneity**
   - Different agents have different task preferences
   - Emergent specialization through utility weights
   - No hardcoded role assignments

2. **Utility-Based Cooperation**
   - Auction system allocates tasks to agents with highest utility
   - Fair distribution based on agent capabilities
   - Dynamic adaptation to changing conditions

3. **Personality Modeling**
   - WeightEnergy: How much agent cares about energy cost
   - WeightHunger: How much agent cares about hunger cost
   - GoalBenefit: Intrinsic preference for task type

4. **Realistic Behavior**
   - Agent B (high WH) needs frequent food breaks
   - Agent A (low WH) can work longer without eating
   - Agent C (balanced) adapts to any situation

### Demo Script:
```
Narrator: "Lihat 3 agent dengan personality berbeda"
[Show Inspector with different weights]

Narrator: "Agent B adalah 'Planting Specialist'"
[Agent B consistently wins planting auctions]

Narrator: "Agent A adalah 'Harvesting Expert'"
[Agent A dominates harvest tasks]

Narrator: "Mereka berkompetisi via auction untuk task terbaik"
[Show auction logs with different utilities]

Narrator: "Sistem otomatis mendistribusikan task berdasarkan expertise"
[3 agents working on different crops simultaneously]
```

---

## ✅ Verification Checklist

- [ ] Agent A Inspector: WE=0.2, WH=0.1, Planting=0.4, Watering=0.3, Harvesting=0.8
- [ ] Agent B Inspector: WE=0.1, WH=0.6, Planting=0.7, Watering=0.2, Harvesting=0.1
- [ ] Agent C Inspector: WE=0.3, WH=0.2, Planting=0.5, Watering=0.3, Harvesting=0.6
- [ ] Agent B wins planting bids (highest U for planting)
- [ ] Agent A wins harvesting bids (highest U for harvesting)
- [ ] Agent B enters Survival mode earlier (hunger sensitive)
- [ ] Console shows different utility values per agent

---

**System Complete!** Setiap agent sekarang punya personality unik! 🎉
