# 🔍 Troubleshooting: Multiple Agents Not Working

## Symptoms:
- ✅ All agents go to same crop (Crop C)
- ✅ Only Farmer A shows current goal/action in debug
- ✅ Farmer B & C standing idle near crops
- ✅ GetWateringCanAction spam (terus mengambil air)

---

## 🐛 Root Cause: Shared Components

### **Problem 1: Shared Behavior Tree Asset**

**WRONG:**
```
Farmer A → BehaviorTree component → External Behavior: FarmerBehaviorTree.asset
Farmer B → BehaviorTree component → External Behavior: FarmerBehaviorTree.asset (SAME!)
Farmer C → BehaviorTree component → External Behavior: FarmerBehaviorTree.asset (SAME!)
```

**Why broken:**
- External Behavior Tree Assets are SHARED across all agents
- Variables in BT (like lastSelectedGoal, lastTargetCrop) are shared
- Only last agent to update wins
- Other agents become "frozen"

**FIX:**
```
Farmer A → BehaviorTree component → External Behavior: FarmerA_BehaviorTree.asset
Farmer B → BehaviorTree component → External Behavior: FarmerB_BehaviorTree.asset
Farmer C → BehaviorTree component → External Behavior: FarmerC_BehaviorTree.asset
```

**Steps:**
1. Project window → Find `FarmerBehaviorTree.asset`
2. Duplicate: Ctrl+D → Rename to `FarmerA_BehaviorTree.asset`
3. Duplicate again → Rename to `FarmerB_BehaviorTree.asset`
4. Duplicate again → Rename to `FarmerC_BehaviorTree.asset`
5. Hierarchy → Select Farmer A
6. Inspector → BehaviorTree component → External Behavior → Assign `FarmerA_BehaviorTree.asset`
7. Repeat for Farmer B & C

---

### **Problem 2: Prefab References**

**If you duplicated agents from prefab:**

```
Hierarchy:
├─ Farmer (Prefab instance)
├─ Farmer (1) (Prefab instance)  ← Still linked to original!
└─ Farmer (2) (Prefab instance)  ← Still linked to original!
```

**Components may reference same instances:**
- NPCStats shares values
- GoapActionProvider receives same events
- BehaviorTree uses same asset

**FIX:**
1. Select all Farmer duplicates (Farmer (1), Farmer (2))
2. Right-click → Prefab → Unpack Completely
3. Rename: Farmer A, Farmer B, Farmer C
4. Assign unique BT assets (see above)

---

### **Problem 3: Shared GoapActionProvider Settings**

**Check:**
1. Select Farmer A
2. Inspector → GoapActionProvider
3. Check `Agent Type` → Should be unique per agent OR properly configured

**If using same AgentType:**
- Multiple agents share same capability set (OK)
- BUT they should still have separate receivers

**Verify:**
- Each agent should have SEPARATE AgentBehaviour component
- Each AgentBehaviour creates separate receiver instance

---

## 🔧 Diagnostic Script

**Add `DebugAgentStatus.cs` to ALL agents:**

```
1. Hierarchy → Select Farmer A
2. Inspector → Add Component → DebugAgentStatus
3. Repeat for Farmer B, C
4. Play mode → Check Inspector
```

**Expected output (GOOD):**
```
Farmer A:
  Instance ID GOAP: 12345
  Instance ID Stats: 67890
  Instance ID BT: 11111
  Current Goal: PlantingGoal
  Current Action: MoveToTargetAction

Farmer B:
  Instance ID GOAP: 54321 ← DIFFERENT!
  Instance ID Stats: 98760 ← DIFFERENT!
  Instance ID BT: 22222 ← DIFFERENT!
  Current Goal: PlantingGoal
  Current Action: MoveToTargetAction

Farmer C:
  Instance ID GOAP: 11223
  Instance ID Stats: 33445
  Instance ID BT: 44444
  Current Goal: PlantingGoal
  Current Action: MoveToTargetAction
```

**Bad output (BROKEN):**
```
Farmer A:
  Instance ID BT: 11111
  Current Goal: PlantingGoal

Farmer B:
  Instance ID BT: 11111 ← SAME AS A! SHARED!
  Current Goal: None ← Not running!

Farmer C:
  Instance ID BT: 11111 ← SAME AS A! SHARED!
  Current Goal: None
```

---

## ✅ Verification Checklist

- [ ] Each agent has UNIQUE Behavior Tree asset
- [ ] Each agent unpacked from prefab (not prefab instances)
- [ ] Each agent has separate AgentBehaviour component
- [ ] Each agent has separate NPCStats component
- [ ] DebugAgentStatus shows DIFFERENT instance IDs for all agents
- [ ] Console shows bids from ALL agents (not just one)
- [ ] All agents show current goal in debug display

---

## 🎯 Expected Console Output (After Fix)

```
[Farmer A] GOAP=12345, Stats=67890, BT=11111
[Farmer B] GOAP=54321, Stats=98760, BT=22222
[Farmer C] GOAP=11223, Stats=33445, BT=44444

[Bid] Farmer A → Crop A (U=0.520, Planting)
[Bid] Farmer B → Crop B (U=0.803, Planting)
[Bid] Farmer C → Crop C (U=0.607, Planting)

[Auction] Crop A: Farmer A (U=0.520)
[Auction] Crop B: Farmer B (U=0.803)
[Auction] Crop C: Farmer C (U=0.607)

[Sensor] Farmer A → Crop A (owned)
[Sensor] Farmer B → Crop B (owned)
[Sensor] Farmer C → Crop C (owned)
```

---

## 🚨 Common Mistakes

### **1. Using External Behavior Tree (Shared Asset)**
- ❌ All agents → FarmerBehaviorTree.asset
- ✅ Each agent → FarmerA/B/C_BehaviorTree.asset

### **2. Duplicating with Ctrl+D in Hierarchy**
- ❌ Creates prefab instances (linked)
- ✅ Unpack prefab first, THEN duplicate

### **3. Not checking Instance IDs**
- ❌ Assume components are separate
- ✅ Verify with DebugAgentStatus script

### **4. Shared External Behavior variables**
- ❌ BT tasks store state in shared variables
- ✅ Each BT asset has separate variable storage

---

## 📖 Unity Behavior Designer External Behaviors

**How External Behaviors work:**

```
Behavior Tree Component:
├─ Behavior Source: External
└─ External Behavior: Asset reference

Asset file (FarmerBehaviorTree.asset):
├─ Task Tree (Selector, Sequences, Actions)
└─ Variables (lastSelectedGoal, lastTargetCrop, etc.)
    ↑
    These are SHARED if same asset used!
```

**When multiple GameObjects use SAME asset:**
- Variable values are SHARED
- Last GameObject to update wins
- Other GameObjects read stale values
- Race condition / frozen behavior

**Solution:**
- Duplicate asset per agent
- Each agent gets independent variable storage

---

## 🎓 For Skripsi

**Explain in thesis:**

"Pada implementasi awal, semua agent menggunakan External Behavior Tree asset yang sama. Hal ini menyebabkan variable state (seperti lastSelectedGoal) di-sharing antar agents. Solusi yang diterapkan adalah membuat duplicate asset untuk setiap agent, sehingga masing-masing memiliki state storage yang independen."

**Demo correction:**
1. Show broken version (all use same asset)
2. Show debug output (only 1 agent active)
3. Explain shared variable problem
4. Show fix (duplicate assets)
5. Show working version (all agents active)

---

**Next Step:** Create unique BT assets for each agent!
