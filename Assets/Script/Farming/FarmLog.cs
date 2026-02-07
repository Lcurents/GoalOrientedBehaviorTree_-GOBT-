namespace FarmingGoap
{
    /// <summary>
    /// Centralized logging utility for multi-agent farming system.
    /// Provides consistent, clean, and categorized log output for thesis reporting.
    /// 
    /// Categories:
    ///   [UTILITY]  - Utility calculation results per crop per agent
    ///   [AUCTION]  - Bid submissions and auction outcomes (winner/losers)
    ///   [GOAL]     - Goal selection and transitions
    ///   [ACTION]   - Action lifecycle (start, complete, interrupted)
    ///   [CROP]     - Crop state changes (plant, water, harvest, growth)
    ///   [RESOURCE] - Inventory and shared resource changes
    ///   [SENSOR]   - Target sensor readings
    ///   [SYSTEM]   - Initialization and diagnostics
    /// 
    /// Format: [CATEGORY] T=mm:ss | Context | Details
    /// </summary>
    public static class FarmLog
    {
        // ──────────────────────── TIME ────────────────────────
        private static string T
        {
            get
            {
                float t = UnityEngine.Time.time;
                int min = UnityEngine.Mathf.FloorToInt(t / 60f);
                int sec = UnityEngine.Mathf.FloorToInt(t % 60f);
                return $"{min:00}:{sec:00}";
            }
        }

        // ──────────────────── LOG METHODS ─────────────────────

        /// <summary>Utility calculation details (per crop evaluation)</summary>
        public static void Utility(string agent, string message)
        {
            UnityEngine.Debug.Log($"[UTILITY] T={T} | {agent} | {message}");
        }

        /// <summary>Auction bids and results (winner/loser)</summary>
        public static void Auction(string message)
        {
            UnityEngine.Debug.Log($"[AUCTION] T={T} | {message}");
        }

        /// <summary>Goal selection and transitions</summary>
        public static void Goal(string agent, string message)
        {
            UnityEngine.Debug.Log($"[GOAL] T={T} | {agent} | {message}");
        }

        /// <summary>Action lifecycle (start, complete, interrupted)</summary>
        public static void Action(string agent, string message)
        {
            UnityEngine.Debug.Log($"[ACTION] T={T} | {agent} | {message}");
        }

        /// <summary>Crop state changes</summary>
        public static void Crop(string crop, string message)
        {
            UnityEngine.Debug.Log($"[CROP] T={T} | {crop} | {message}");
        }

        /// <summary>Inventory and shared resource changes</summary>
        public static void Resource(string agent, string message)
        {
            UnityEngine.Debug.Log($"[RESOURCE] T={T} | {agent} | {message}");
        }

        /// <summary>Target sensor readings</summary>
        public static void Sensor(string agent, string message)
        {
            UnityEngine.Debug.Log($"[SENSOR] T={T} | {agent} | {message}");
        }

        /// <summary>System initialization and diagnostics</summary>
        public static void System(string message)
        {
            UnityEngine.Debug.Log($"[SYSTEM] T={T} | {message}");
        }

        // ──────────────────── WARNING VARIANTS ────────────────

        public static void AuctionWarn(string message)
        {
            UnityEngine.Debug.LogWarning($"[AUCTION] T={T} | {message}");
        }

        public static void ActionWarn(string agent, string message)
        {
            UnityEngine.Debug.LogWarning($"[ACTION] T={T} | {agent} | {message}");
        }

        public static void GoalWarn(string agent, string message)
        {
            UnityEngine.Debug.LogWarning($"[GOAL] T={T} | {agent} | {message}");
        }

        public static void SystemWarn(string message)
        {
            UnityEngine.Debug.LogWarning($"[SYSTEM] T={T} | {message}");
        }

        public static void SystemError(string message)
        {
            UnityEngine.Debug.LogError($"[SYSTEM] T={T} | {message}");
        }

        // ──────────────── HELPER: Utility Format ──────────────

        /// <summary>
        /// Format utility value: shows "-.---" for invalid (-999) values
        /// </summary>
        public static string U(float utility)
        {
            return utility <= -999f ? "-.---" : utility.ToString("F3");
        }
    }
}
