
namespace SnowBrosMod;

class SnowBall : MonoBehaviour
{
    public SpriteRenderer sr;
    public BoxCollider2D col;
    public Rigidbody2D rig;
    public GameObject bindEnemy;
    private void Awake()
    {
        sr = gameObject.AddComponent<SpriteRenderer>();
        col = gameObject.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.enabled = false;
        rig = gameObject.AddComponent<Rigidbody2D>();
        rig.isKinematic = true;
    }
    public void UpdateLevel(int level)
    {
        if (level == 0)
        {
            gameObject.SetActive(false);
            return;
        }
        else
        {
            gameObject.SetActive(true);
        }
        sr.sprite = SnowBros.sprites["SnowBall_" + Mathf.Clamp(level, 1, 4).ToString()];
        col.size = sr.bounds.size;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == (int)PhysLayers.TERRAIN)
        {
            HitTaker.Hit(bindEnemy,
                new()
                {
                    DamageDealt = 10,
                    Source = gameObject,
                    CircleDirection = true,
                    Multiplier = 1,
                    MagnitudeMultiplier = 1,
                    AttackType = AttackTypes.Nail,
                    SpecialType = SpecialTypes.None
                });
        }
    }
}
