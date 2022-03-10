
namespace SnowBrosMod;

class AnimController : MonoBehaviour
{
    public SpriteRenderer renderer;
    public HeroScript script;
    public HeroController hc => script.hc;
    public static Dictionary<string, (float, bool)> clipInfo = new()
    {
        ["Run"] = (0.1f / 4, false),
        ["Die"] = (0.1f / 4, false),
        ["Walk"] = (0.4f / 12, true),
        ["Atk"] = (0.2f / 8, false),
        ["Jump"] = (0.05f, false),
        ["Kick"] = (0.1f, false)
    };
    public string currentSpr;
    public string currentClip;
    public int currentFrame;
    public float everyFrameTime = 0.1f;
    public bool isPlaying;
    public bool isLoop;
    private void Awake() 
    {
        renderer = gameObject.AddComponent<SpriteRenderer>();
    }
    public void SetSprite(Sprite sprite)
    {
        renderer.sprite = sprite;
        //renderer.drawMode = SpriteDrawMode.Sliced;
        transform.localPosition = new Vector3(0f, -0.75f, 0f);
        script.col.size = renderer.sprite.bounds.size;
    }
    public void SetSprite(string name)
    {
        if(HasSprite(name)) SetSprite(SnowBros.sprites[name + ".png"]);
    }
    public bool HasSprite(string name)
    {
        return SnowBros.sprites.ContainsKey(name + ".png");
    }
    public void Play(string name, bool loop = false)
    {
        if(!(loop && currentClip == name)) currentFrame = 1; 
        currentClip = name;
        isLoop = loop;
        isPlaying = true;
        if(clipInfo.TryGetValue(name, out var info))
        {
            everyFrameTime = info.Item1;
            transform.localScale = new Vector3(info.Item2 ? 1 : -1, 1, 1);
        }
        else
        {
            everyFrameTime = 0.1f;
            transform.localScale = new Vector3(1, 1, 1);
        }
    }

    private void UpdateSprite()
    {
        if(!isPlaying) return;
        var srN = currentClip + "_" + currentFrame.ToString();
        if(!HasSprite(srN))
        {
            if(isLoop)
            {
                currentFrame = 1;
                return;
            }
            isPlaying = false;
        }
        SetSprite(srN);
        currentFrame++;
    }
    float lastT = 0;
    private void Update()
    {
        if(Time.time - lastT < everyFrameTime)
        {
            return;
        }
        lastT = Time.time;
        UpdateSprite();
    }
}
