
namespace SnowBrosMod;

class SnowBullet : MonoBehaviour
{
    private SpriteRenderer spr;
    private Rigidbody2D rig;
    private int currentFrame;
    private float lastUpdate;
    private void Awake() 
    {
        spr = gameObject.AddComponent<SpriteRenderer>();
        currentFrame = 1;
        gameObject.layer = (int)PhysLayers.HERO_ATTACK;
        var col = gameObject.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.5f, 0.5f);
        col.isTrigger = true;
        rig = gameObject.AddComponent<Rigidbody2D>();
    }
    private void Start() 
    {
        if(transform.localScale.x > Mathf.Epsilon)
        {
            rig.velocity = new Vector2(20, 8);
        }
        else
        {
            rig.velocity = new Vector2(-20, 8);
        }
    }
    private void OnTriggerEnter2D(Collider2D other) 
    {
        HitTaker.Hit(other.gameObject, new()
        {
            DamageDealt = 10,
            CircleDirection = true,
            Source = gameObject,
            Multiplier = 1,
            MagnitudeMultiplier = 1,
            AttackType = AttackTypes.Acid,
            SpecialType = SpecialTypes.Acid
        });
        if(other.gameObject.layer == (int)PhysLayers.TERRAIN) Destroy(gameObject);
    }
    private void Update() {
        
        if(Time.time - lastUpdate < (0.2f / 4))
        {
            return;
        }
        lastUpdate = Time.time;
        spr.sprite = SnowBros.sprites["Attack_" + currentFrame.ToString() + ".png"];
        spr.drawMode = SpriteDrawMode.Sliced;
        spr.size = new Vector2(0.5f, 0.5f);
        currentFrame++;
        if(currentFrame > 6) currentFrame = 1;
    }
}
