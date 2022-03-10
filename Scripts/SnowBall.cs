
namespace SnowBrosMod;

class SnowBall : MonoBehaviour
{
    public class SnowBallEvent : MonoBehaviour
    {
        public Action onUpdate;
        public SnowBall snowBall;

        private void Update()
        {
            if (!snowBall.gameObject.activeSelf) onUpdate?.Invoke();
        }
        private IEnumerator DelayKill(HealthManager hm)
        {
            yield return new WaitForSeconds(0.05f);
            hm.ApplyExtraDamage(999999);
            yield return null;
            if(hm.hp <= 0 && !hm.isDead) hm.Die(null, AttackTypes.Generic, true);
            Destroy(snowBall);
        }
    }
    public bool isKick = false;
    public float lastKickTime = 0;
    public bool setDie = false;
    public SpriteRenderer sr;
    public BoxCollider2D col;
    public Rigidbody2D rig;
    public GameObject bindEnemy;
    public GameObject snowBallObj;
    public SnowBallEvent snowBallEvent;
    public Collider2D enemyCol;
    public int level;
    public bool kickR;
    private void Awake()
    {
        snowBallObj = new GameObject("Snow Ball");
        snowBallObj.transform.position = transform.position;
        snowBallObj.layer = (int)PhysLayers.ENEMIES;

        snowBallEvent = snowBallObj.AddComponent<SnowBallEvent>();
        snowBallEvent.snowBall = this;
        snowBallEvent.onUpdate += Update;

        sr = snowBallObj.AddComponent<SpriteRenderer>();
        col = snowBallObj.AddComponent<BoxCollider2D>();
        col.isTrigger = false;
        col.enabled = false;
        rig = snowBallObj.AddComponent<Rigidbody2D>();
        rig.isKinematic = true;
        rig.sharedMaterial = new()
        {
            friction = 0
        };

        snowBallObj.SetActive(false);
    }
    private void Start()
    {
        foreach (var v in bindEnemy.GetComponents<PlayMakerFSM>())
        {
            v.Fsm.RestartOnEnable = false;
        }
    }
    public void UpdateLevel(int level)
    {
        this.level = level;
        if (level == 0)
        {
            snowBallObj.SetActive(false);
            return;
        }
        else
        {
            snowBallObj.SetActive(true);
        }
        if (level >= 4)
        {
            col.enabled = true;
            rig.isKinematic = false;
            rig.gravityScale = 1;
        }
        else
        {
            rig.isKinematic = true;
            rig.gravityScale = 0;
            col.enabled = false;
        }

        sr.sprite = SnowBros.sprites["SnowBall_" + Mathf.Clamp(level, 1, 4).ToString()];
        sr.drawMode = SpriteDrawMode.Sliced;
    }
    private void Update()
    {
        if (enemyCol == null)
        {
            enemyCol = bindEnemy?.GetComponent<Collider2D>();
            Vector3 pos = enemyCol.bounds.center;
            pos.z = transform.position.z - 0.1f;
            snowBallObj.transform.position = pos;
        }
        if (bindEnemy == null || enemyCol == null || (bindEnemy.GetComponent<HealthManager>()?.isDead ?? true))
        {
            bindEnemy?.SetActive(false);
            Destroy(this);
            return;
        }
        if (level >= 4)
        {
            bindEnemy.SetActive(false);
            bindEnemy.transform.position = col.bounds.center;
        }
        else
        {
            Vector3 pos = enemyCol.bounds.center;
            pos.z = transform.position.z - 0.1f;
            snowBallObj.transform.position = pos;
            var m = Mathf.Max(enemyCol.bounds.size.x, enemyCol.bounds.size.y);
            col.size = new Vector2(m, m);
            sr.size = col.size;
        }
        if (isKick && Time.time - lastKickTime >= 0.25f)
        {
            if ((Mathf.Abs(rig.velocity.x) <= 0.1f || (kickR && rig.velocity.x < 0)) && !setDie)
            {
                var hm = bindEnemy?.GetComponent<HealthManager>();
                if (hm != null)
                {
                    bindEnemy.SetActive(true);
                    hm.hp = 0;
                    snowBallEvent.StartCoroutine("DelayKill", hm);
                }
                setDie = true;
            }
        }
    }
    
    public void NextLevel()
    {
        UpdateLevel(level + 1);
    }
    private void OnDestroy()
    {
        bindEnemy?.SetActive(true);
        Destroy(snowBallObj);
    }

}
