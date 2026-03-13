using UnityEngine;

namespace FarmingGoap.Behaviours
{
    /// <summary>
    /// Visual holder for shovel sprite on an agent.
    /// </summary>
    public class ShovelCarrier : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer shovelSprite;
        [SerializeField] private NPCStats stats;
        [SerializeField] private bool autoSyncWithStats = true;

        private void Awake()
        {
            if (stats == null)
                stats = GetComponent<NPCStats>();

            if (shovelSprite == null)
            {
                var child = transform.Find("ShovelSprite");
                if (child != null)
                    shovelSprite = child.GetComponent<SpriteRenderer>();
            }
        }

        private void Update()
        {
            if (!autoSyncWithStats || stats == null)
                return;

            SetHeld(stats.HasShovel > 0);
        }

        public void SetHeld(bool held)
        {
            if (shovelSprite != null)
            {
                shovelSprite.enabled = held;
            }
        }
    }
}
