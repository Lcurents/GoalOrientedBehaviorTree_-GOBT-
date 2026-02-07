using System.Collections.Generic;
using System.Linq;
using FarmingGoap.Behaviours;
using UnityEngine;

namespace FarmingGoap.Managers
{
    /// <summary>
    /// Singleton manager untuk crop allocation dan auction system
    /// Manages multi-agent cooperation via bidding mechanism
    /// </summary>
    public class CropManager : MonoBehaviour
    {
        public static CropManager Instance { get; private set; }

        [Header("Debug")]
        [SerializeField] private bool enableDebugLog = true;

        // Track crop reservations: Crop → Agent
        private Dictionary<CropBehaviour, GameObject> cropReservations = new Dictionary<CropBehaviour, GameObject>();
        
        // Pending bids: Crop → List<(Agent, Utility)>
        private Dictionary<CropBehaviour, List<CropBid>> pendingBids = new Dictionary<CropBehaviour, List<CropBid>>();
        
        // Track last auction winners to avoid duplicate logs
        private Dictionary<CropBehaviour, GameObject> lastAuctionWinners = new Dictionary<CropBehaviour, GameObject>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            
            // DIAGNOSTIC: Log all crops found at startup
            StartCoroutine(DiagnosticCheckCrops());
        }
        
        private System.Collections.IEnumerator DiagnosticCheckCrops()
        {
            yield return new WaitForSeconds(0.5f); // Wait for scene to fully load
            
            var allCrops = FindObjectsByType<CropBehaviour>(FindObjectsSortMode.None);
            
            UnityEngine.Debug.Log($"========== CROPMANAGER DIAGNOSTIC ==========");
            UnityEngine.Debug.Log($"[CropManager] Total crops found in scene: {allCrops.Length}");
            
            if (allCrops.Length == 0)
            {
                UnityEngine.Debug.LogError($"[CropManager] ❌ NO CROPS FOUND! Add CropBehaviour to crops in scene!");
            }
            else
            {
                for (int i = 0; i < allCrops.Length; i++)
                {
                    var crop = allCrops[i];
                    UnityEngine.Debug.Log(
                        $"[CropManager] Crop #{i+1}: " +
                        $"Name='{crop.gameObject.name}', " +
                        $"InstanceID={crop.GetInstanceID()}, " +
                        $"Position={crop.transform.position}"
                    );
                }
                
                // Verify all crops have unique instances
                var uniqueIDs = new System.Collections.Generic.HashSet<int>();
                foreach (var crop in allCrops)
                {
                    uniqueIDs.Add(crop.GetInstanceID());
                }
                
                if (uniqueIDs.Count == allCrops.Length)
                {
                    UnityEngine.Debug.Log($"[CropManager] ✅ All crops have UNIQUE instances (tracking will work correctly)");
                }
                else
                {
                    UnityEngine.Debug.LogError($"[CropManager] ❌ BUG: Some crops share instances!");
                }
            }
            
            UnityEngine.Debug.Log($"============================================");
        }

        /// <summary>
        /// Check if crop is available (not reserved by another agent)
        /// </summary>
        public bool IsCropAvailable(CropBehaviour crop, GameObject requestingAgent)
        {
            if (crop == null) return false;

            // Not reserved = available
            if (!cropReservations.ContainsKey(crop))
                return true;

            // Reserved by this agent = available
            if (cropReservations[crop] == requestingAgent)
                return true;

            // Reserved by different agent = not available
            return false;
        }

        /// <summary>
        /// Submit bid untuk crop dengan utility value
        /// Returns immediately, actual assignment happens in auction phase
        /// </summary>
        public void SubmitBid(CropBehaviour crop, GameObject agent, float utility, string goalType)
        {
            if (crop == null || agent == null) return;

            // RULE: Can't bid on crop already reserved by ANOTHER agent
            // This ensures first-come-first-served: existing reservations are never overridden
            if (cropReservations.ContainsKey(crop) && cropReservations[crop] != agent)
            {
                if (enableDebugLog)
                    UnityEngine.Debug.Log($"[Bid] {agent.name} → {crop.name} REJECTED (reserved by {cropReservations[crop].name})");
                return;
            }

            if (!pendingBids.ContainsKey(crop))
            {
                pendingBids[crop] = new List<CropBid>();
            }

            var bid = new CropBid
            {
                agent = agent,
                utility = utility,
                goalType = goalType
            };

            pendingBids[crop].Add(bid);

            // DIAGNOSTIC: Log bid with crop instance info
            if (enableDebugLog && (!cropReservations.ContainsKey(crop) || cropReservations[crop] != agent))
            {
                UnityEngine.Debug.Log(
                    $"[Bid] {agent.name} → {crop.name} " +
                    $"(U={utility:F3}, {goalType}, CropID={crop.GetInstanceID()})"
                );
            }
            
            // CRITICAL FIX: Run auction immediately so GOAP can plan with correct reservations!
            // Don't wait for Update() - agents need reservations BEFORE GOAP planning starts
            RunAuctionImmediate();
        }

        /// <summary>
        /// Run auction untuk semua pending bids
        /// Called immediately after bids to ensure reservations before GOAP planning
        /// </summary>
        private void RunAuctionImmediate()
        {
            if (pendingBids.Count == 0) return;

            // DIAGNOSTIC: Log how many unique crops have bids
            if (enableDebugLog)
            {
                var uniqueCropIDs = new System.Collections.Generic.HashSet<int>();
                foreach (var crop in pendingBids.Keys)
                {
                    uniqueCropIDs.Add(crop.GetInstanceID());
                }
                UnityEngine.Debug.Log($"[Auction] Processing bids for {pendingBids.Count} crops (Unique IDs: {uniqueCropIDs.Count})");
            }

            foreach (var kvp in pendingBids)
            {
                var crop = kvp.Key;
                var bids = kvp.Value;

                if (bids.Count == 0) continue;

                // Single bidder = auto win
                if (bids.Count == 1)
                {
                    var winner = bids[0];
                    bool winnerChanged = !lastAuctionWinners.ContainsKey(crop) || lastAuctionWinners[crop] != winner.agent;
                    
                    ReserveCrop(crop, winner.agent);
                    lastAuctionWinners[crop] = winner.agent;
                    
                    // Only log if winner changed
                    if (enableDebugLog && winnerChanged)
                        UnityEngine.Debug.Log($"[Auction] {crop.name}: {winner.agent.name} (U={winner.utility:F3})");
                }
                else
                {
                    // Multiple bidders = auction!
                    var winner = bids.OrderByDescending(b => b.utility).First();
                    bool winnerChanged = !lastAuctionWinners.ContainsKey(crop) || lastAuctionWinners[crop] != winner.agent;
                    
                    ReserveCrop(crop, winner.agent);
                    lastAuctionWinners[crop] = winner.agent;

                    // Only log competitive auctions or when winner changes
                    if (enableDebugLog && winnerChanged)
                    {
                        string bidDetails = string.Join(", ", bids.Select(b => $"{b.agent.name}(U={b.utility:F3})"));
                        UnityEngine.Debug.Log($"[Auction] {crop.name}: {winner.agent.name} WINS vs [{bidDetails}]");
                    }
                }
            }

            // Clear pending bids
            pendingBids.Clear();
        }

        /// <summary>
        /// Reserve crop untuk agent
        /// </summary>
        private void ReserveCrop(CropBehaviour crop, GameObject agent)
        {
            bool isNewReservation = !cropReservations.ContainsKey(crop) || cropReservations[crop] != agent;
            cropReservations[crop] = agent;
            
            // Log reservation for debugging multi-agent issues
            if (enableDebugLog && isNewReservation)
                UnityEngine.Debug.Log($"[CropManager] RESERVED: {agent.name} → {crop.name}");
        }

        /// <summary>
        /// Release crop reservation (called when action completes)
        /// </summary>
        public void ReleaseCrop(CropBehaviour crop, GameObject agent)
        {
            if (crop == null || agent == null) return;

            if (cropReservations.ContainsKey(crop) && cropReservations[crop] == agent)
            {
                cropReservations.Remove(crop);
                
                if (enableDebugLog)
                    UnityEngine.Debug.Log($"[CropManager] Released: {agent.name} released {crop.name}");
            }
        }

        /// <summary>
        /// Get all available crops for specific goal type
        /// </summary>
        public List<CropBehaviour> GetAvailableCrops(GameObject agent, System.Func<CropBehaviour, bool> filter)
        {
            var allCrops = FindObjectsByType<CropBehaviour>(FindObjectsSortMode.None);
            
            return allCrops
                .Where(c => IsCropAvailable(c, agent) && filter(c))
                .ToList();
        }

        /// <summary>
        /// Get reserved agent for crop (for debugging)
        /// </summary>
        public GameObject GetReservedAgent(CropBehaviour crop)
        {
            if (cropReservations.ContainsKey(crop))
                return cropReservations[crop];
            return null;
        }

        /// <summary>
        /// Check if crop is reserved by specific agent
        /// </summary>
        public bool IsReservedBy(CropBehaviour crop, GameObject agent)
        {
            if (crop == null || agent == null) return false;
            return cropReservations.ContainsKey(crop) && cropReservations[crop] == agent;
        }

        private void Update()
        {
            // Auction now runs immediately in SubmitBid()
            // Update() no longer needed for auction processing
        }

        // Data structure for bids
        private class CropBid
        {
            public GameObject agent;
            public float utility;
            public string goalType;
        }
    }
}
