using UnityEngine;

namespace FarmingGoap.Managers
{
    /// <summary>
    /// Shared shovel storage with single-reservation behavior.
    /// Attach to Storage GameObject (tag: Storage).
    /// </summary>
    public class ShovelStorage : MonoBehaviour
    {
        public static ShovelStorage Instance { get; private set; }

        [Header("Capacity")]
        [SerializeField] private int maxShovels = 1;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer shovelSprite;

        private int availableShovels;
        private GameObject reservedAgent;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            availableShovels = Mathf.Max(0, maxShovels);

            if (shovelSprite == null)
            {
                var child = transform.Find("ShovelSprite");
                if (child != null)
                    shovelSprite = child.GetComponent<SpriteRenderer>();
            }

            UpdateVisual();
        }

        public bool TryReserve(GameObject agent)
        {
            if (agent == null)
                return false;

            if (reservedAgent == agent)
                return true;

            if (availableShovels <= 0 || reservedAgent != null)
                return false;

            reservedAgent = agent;
            availableShovels = Mathf.Max(0, availableShovels - 1);
            UpdateVisual();
            return true;
        }

        public void Return(GameObject agent)
        {
            if (agent == null)
                return;

            if (reservedAgent != agent)
                return;

            reservedAgent = null;
            availableShovels = Mathf.Min(maxShovels, availableShovels + 1);
            UpdateVisual();
        }

        public bool IsReservedBy(GameObject agent)
        {
            return reservedAgent == agent;
        }

        public bool IsAvailable()
        {
            return availableShovels > 0 && reservedAgent == null;
        }

        private void UpdateVisual()
        {
            if (shovelSprite != null)
            {
                shovelSprite.enabled = availableShovels > 0;
            }
        }
    }
}
