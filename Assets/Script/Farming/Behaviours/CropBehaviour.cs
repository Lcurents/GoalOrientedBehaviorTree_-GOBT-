using UnityEngine;

namespace FarmingGoap.Behaviours
{
    public class CropBehaviour : MonoBehaviour
    {
        [Header("Crop State")]
        [SerializeField] private int growthStage = 0; // 0=Empty, 1=Planted, 2=Growing, 3=Ready
        
        [Header("Growth Timing Settings")]
        [Tooltip("Waktu tiap fase dalam detik")]
        [SerializeField] private float growthTimePerStage = 10f; // MUDAH DIUBAH DI INSPECTOR!
        
        [Header("Visual (Optional)")]
        [SerializeField] private SpriteRenderer cropSprite;
        [SerializeField] private Sprite[] growthSprites; // 4 sprites untuk setiap stage

        private float growthTimer = 0f;
        private bool isGrowing = false;

        public int GrowthStage => growthStage;

        private void Start()
        {
            UpdateVisual();
        }

        private void Update()
        {
            // Auto-grow jika sedang dalam proses tumbuh (stage 1 atau 2)
            if (isGrowing && growthStage >= 1 && growthStage < 3)
            {
                growthTimer += Time.deltaTime;

                if (growthTimer >= growthTimePerStage)
                {
                    growthTimer = 0f;
                    growthStage++;
                    UpdateVisual();

                    if (growthStage >= 3)
                    {
                        isGrowing = false;
                    }

                    FarmLog.Crop(gameObject.name, $"Growth -> Stage {growthStage}");
                }
            }
        }

        public void SetGrowthStage(int stage)
        {
            growthStage = Mathf.Clamp(stage, 0, 3);
            UpdateVisual();
        }

        public void Plant()
        {
            // Tanam bibit
            growthStage = 1;
            isGrowing = true;
            growthTimer = 0f;
            UpdateVisual();
            FarmLog.Crop(gameObject.name, "Planted -> Stage 1");
        }

        public void WaterCrop()
        {
            // Siram tanaman = mulai proses pertumbuhan
            if (growthStage >= 1 && growthStage < 3)
            {
                isGrowing = true;
                FarmLog.Crop(gameObject.name, $"Watered at Stage {growthStage} | GrowthTimer={growthTimePerStage}s");
            }
        }

        public void Harvest()
        {
            // Reset ke empty
            growthStage = 0;
            isGrowing = false;
            growthTimer = 0f;
            UpdateVisual();
            FarmLog.Crop(gameObject.name, "Harvested -> Stage 0 (reset)");
        }

        private void UpdateVisual()
        {
            if (cropSprite != null && growthSprites != null && growthSprites.Length >= 4)
            {
                cropSprite.sprite = growthSprites[growthStage];
            }
        }

        // Gizmo untuk debugging
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}
