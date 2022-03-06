
namespace SnowBrosMod;

[AttachHeroController]
class HeroScript : MonoBehaviour
{
    public static HeroScript instance;
    public HeroController hc;
    public ReflectionObject hcr;
    public HeroAnimationController hcanim;
    public tk2dSpriteAnimator anim;
    public MeshRenderer renderer;
    public AnimController animCtrl;
    public BoxCollider2D col;
    public Rigidbody2D rig;
    public HeroActions ha;
    public readonly static Vector2 idleSize = new Vector2(0.5f, 1.2813f);
    private bool lastRun;
    private void OnEnable()
    {
        ResetState();
        isRespawn = false;

        ha = InputHandler.Instance.inputActions;
        hc = GetComponent<HeroController>();
        hcr = hc.CreateReflectionObject();
        hcanim = GetComponent<HeroAnimationController>();
        anim = GetComponent<tk2dSpriteAnimator>();
        renderer = GetComponent<MeshRenderer>();
        col = GetComponent<BoxCollider2D>();
        rig = GetComponent<Rigidbody2D>();

        animCtrl.gameObject.SetActive(true);

        hc.StopAnimationControl();
        hc.RelinquishControl();

        On.HeroController.CanTalk += HookCanTalk;
        ModHooks.TakeHealthHook += HookTakeHealth;

        renderer.enabled = false;
    }
    private bool HookCanTalk(On.HeroController.orig_CanTalk _, HeroController self)
    {
        return self.cState.onGround &&
            (animCtrl.currentClip == "Idle" || animCtrl.currentClip == "Run"
                || animCtrl.currentClip == "Walk");
    }
    private int HookTakeHealth(int orig)
    {
        if (orig > 0) Hurt();
        return orig;
    }
    bool isRespawn = false;
    float respawnTime = 0;
    float atkColdTime = 0;
    bool attacking = false;
    bool kicking = false;
    private void Hurt()
    {
        isRespawn = true;
        rig.velocity = new Vector2(0, 15);
        col.enabled = false;
        respawnTime = Time.time;
        ResetState();
    }
    private void ResetState()
    {
        attacking = false;
        kicking = false;
        atkColdTime = 0;
    }
    private void Fire()
    {
        var go = new GameObject("Snowball");
        go.AddComponent<SnowBullet>();
        go.transform.position = transform.position;
        go.transform.localScale = new Vector3(hc.cState.facingRight ? 1 : -1, 1, 1);
    }
    private void Kick(GameObject go)
    {
        HitTaker.Hit(go,
                        new()
                        {
                            DamageDealt = 10,
                            Source = hc.gameObject,
                            CircleDirection = true,
                            Multiplier = 1,
                            MagnitudeMultiplier = 1,
                            AttackType = AttackTypes.Nail,
                            SpecialType = SpecialTypes.None
                        });
    }
    public bool CanSit()
    {
        return true;
    }
    private void Update()
    {
        if(ha.left.IsPressed) hc.FaceLeft();
        else if(ha.right.IsPressed) hc.FaceRight();
        renderer.enabled = false;
        if (hc.CanInput())
        {
            hc.StopAnimationControl();
            hc.RelinquishControl();
        }

        if (isRespawn)
        {
            animCtrl.Play("Die", true);
            rig.velocity = new Vector2(0, 15);
            if (Time.time - respawnTime >= 0.5f)
            {
                rig.velocity = Vector2.zero;
                StartCoroutine(HeroController.instance.HazardRespawn());
                col.enabled = true;
                isRespawn = false;
            }
            return;
        }
        col.enabled = true;
        bool dosth = false;

        if (PlayerData.instance.atBench)
        {
            animCtrl.isPlaying = false;
            animCtrl.SetSprite("Run_4");
            return;
        }
        if (ha.attack.IsPressed && !attacking)
        {
            kicking = true;
            atkColdTime = Time.time;
            animCtrl.Play("Kick");
        }
        if (ha.dreamNail.IsPressed && !kicking)
        {
            if (Time.time - atkColdTime >= 0.15f)
            {
                attacking = true;
                atkColdTime = float.MaxValue;
            }
        }
        if (attacking)
        {
            if (animCtrl.currentClip != "Atk") animCtrl.Play("Atk");
            if (animCtrl.isPlaying)
            {
                if (animCtrl.currentFrame == 5) Fire();
                rig.velocity = Vector2.zero;
                return;
            }
            attacking = false;
            atkColdTime = Time.time;
        }
        if (kicking)
        {
            rig.velocity = Vector2.zero;
            if (animCtrl.isPlaying)
            {
                foreach (var v in Physics2D.BoxCastAll(col.bounds.center, col.bounds.size, 0,
                    hc.cState.facingRight ? Vector2.right : Vector2.left, 0.35f))
                {
                    if(v.transform.root.name == "Knight") continue;
                    Kick(v.collider.gameObject);
                }
                return;
            }
            if (Time.time - atkColdTime < 0.05f) return;
            if(Time.time - atkColdTime < 0.15f)
            {
                if(hc.cState.onGround)
                {
                    animCtrl.Play("Run", true);
                }
                else
                {
                    animCtrl.Play("Jump", true);
                }
                return;
            }
            kicking = false;
            atkColdTime = Time.time;
        }
        hcr.SetMemberData<float>("fallTimer", 0);
        if (ha.left.IsPressed)
        {
            hc.FaceLeft();
            lastRun = true;

            hc.acceptingInput = true;
            hcr.InvokeMethod("Move", (float)-1);
            hc.acceptingInput = false;
            dosth = true;
            if (hc.cState.onGround)
            {
                if (hc.cState.inWalkZone)
                {
                    animCtrl.Play("Walk", true);
                }
                else
                {
                    animCtrl.Play("Run", true);
                }

            }
        }
        else if (ha.right.IsPressed)
        {
            hc.FaceRight();
            lastRun = true;

            hc.acceptingInput = true;
            hcr.InvokeMethod("Move", (float)1);
            hc.acceptingInput = false;
            dosth = true;
            if (hc.cState.onGround)
            {
                if (hc.cState.inWalkZone)
                {
                    animCtrl.Play("Walk", true);
                }
                else
                {
                    animCtrl.Play("Run", true);
                }

            }
        }
        else
        {
            if (lastRun)
            {
                rig.velocity = Vector2.zero;
                hc.move_input = 0;
                hc.vertical_input = 0;
                lastRun = false;
            }
        }
        if (ha.jump.IsPressed)
        {
            dosth = true;
            hc.cState.jumping = true;
            animCtrl.Play("Jump", true);
        }
        renderer.enabled = false;

        if (!dosth && hc.cState.onGround)
        {
            animCtrl.Play("Run", true);
        }
        if (!hc.cState.onGround)
        {
            animCtrl.Play("Jump", true);
        }
    }
    private void OnDisable()
    {
        renderer.enabled = true;
        On.HeroController.CanTalk -= HookCanTalk;
        ModHooks.TakeHealthHook -= HookTakeHealth;
        animCtrl.gameObject.SetActive(false);
        hc.StartAnimationControl();
        hc.RegainControl();
    }
    private void Start()
    {
        instance = this;
        var go = new GameObject("Snow Man");
        go.transform.parent = hc.transform;
        go.transform.localPosition = Vector3.zero;
        animCtrl = go.AddComponent<AnimController>();
        animCtrl.script = this;
        go.SetActive(false);
        enabled = false;

    }

}
