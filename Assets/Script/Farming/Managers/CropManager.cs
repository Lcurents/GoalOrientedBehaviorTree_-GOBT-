using System.Collections.Generic;
using System.Linq;
using FarmingGoap.Behaviours;
using UnityEngine;
using System.Text;

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
            yield return new WaitForSeconds(0.5f);
            
            var allCrops = FindObjectsByType<CropBehaviour>(FindObjectsSortMode.None);
            
            var sb = new StringBuilder();
            sb.AppendLine($"CropManager initialized | {allCrops.Length} crops detected");
            
            if (allCrops.Length == 0)
            {
                FarmLog.SystemError("No crops found in scene! Add CropBehaviour to crop GameObjects.");
            }
            else
            {
                for (int i = 0; i < allCrops.Length; i++)
                {
                    var crop = allCrops[i];
                    sb.AppendLine($"    Crop #{i+1}: {crop.gameObject.name} | Pos={crop.transform.position}");
                }
                FarmLog.System(sb.ToString().TrimEnd());
            }
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
            // First-come-first-served: existing reservations are never overridden
            if (cropReservations.ContainsKey(crop) && cropReservations[crop] != agent)
            {
                if (enableDebugLog)
                    FarmLog.AuctionWarn($"{agent.name} -> {crop.name} REJECTED (reserved by {cropReservations[crop].name})");
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

            if (enableDebugLog && (!cropReservations.ContainsKey(crop) || cropReservations[crop] != agent))
            {
                FarmLog.Auction($"BID: {agent.name} -> {crop.name} | U={utility:F3} | Goal={goalType}");
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

            // Skip verbose processing log - auction results below are sufficient

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
                    
                    if (enableDebugLog && winnerChanged)
                        FarmLog.Auction($"RESULT: {crop.name} -> WINNER: {winner.agent.name} (U={winner.utility:F3}, {winner.goalType}) | No competitors");
                }
                else
                {
                    // Multiple bidders = competitive auction!
                    var sorted = bids.OrderByDescending(b => b.utility).ToList();
                    var winner = sorted[0];
                    bool winnerChanged = !lastAuctionWinners.ContainsKey(crop) || lastAuctionWinners[crop] != winner.agent;
                    
                    ReserveCrop(crop, winner.agent);
                    lastAuctionWinners[crop] = winner.agent;

                    if (enableDebugLog && winnerChanged)
                    {
                        string losers = string.Join(", ", sorted.Skip(1).Select(b => $"{b.agent.name}(U={b.utility:F3})"));
                        FarmLog.Auction($"RESULT: {crop.name} -> WINNER: {winner.agent.name} (U={winner.utility:F3}, {winner.goalType}) | LOSERS: {losers}");
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
            
            if (enableDebugLog && isNewReservation)
                FarmLog.Auction($"RESERVED: {agent.name} -> {crop.name}");
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
                    FarmLog.Auction($"RELEASED: {agent.name} released {crop.name}");
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
