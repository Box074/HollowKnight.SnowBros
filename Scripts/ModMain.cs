
namespace SnowBrosMod;

class SnowBros : ModBaseWithSettings<SnowBros, Settings, object> , IGlobalSettings<Settings>
{
    public static Dictionary<string, Sprite> sprites = new();
    private void LoadImages()
    {
        foreach(var v in typeof(SnowBros).Assembly.GetManifestResourceNames().Where(x => x.EndsWith(".png")))
        {
            var tex = LoadTexture2D(v);
            tex.filterMode = FilterMode.Point;
            if(v != "SnowBall.png") sprites.Add(v, Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f), 80));
            else 
            {
                for(int i = 0; i < 4 ; i++)
                {
                    sprites.Add("SnowBall_" + (i + 1).ToString(), Sprite.Create(tex, new Rect(i * 42, 0, 42, 41), 
                        new Vector2(0.5f, 0.5f), 42, 0, SpriteMeshType.FullRect));
                }
            }
        }
    }
    public override void Initialize()
    {
        LoadImages();
        ModHooks.HeroUpdateHook += () =>
        {
            if(Input.GetKeyDown(KeyCode.Alpha9))
            {
                if(HeroScript.instance == null) return;
                HeroScript.instance.enabled = !HeroScript.instance.enabled;
            }
        };
    }

    [FsmPatcher(false, "", "", "Bench Control")]
    private static void ModifyBenchCanRest(FSMPatch patch)
    {
        patch.EditState("Can Rest?")
            .ForEachFsmStateActions<CallMethodProper>(
                x =>
                {
                    if(x.methodName.Value == "CanInput")
                    {
                        var store = x.storeResult;
                        return FSMHelper.CreateMethodAction(
                            (act) =>
                            {
                                if(HeroScript.instance is null)
                                {
                                    store.SetValue(HeroController.instance.CanInput());
                                    return;
                                }
                                store.SetValue(HeroScript.instance.enabled ? 
                                    HeroScript.instance.CanSit() : HeroController.instance.CanInput());
                            }
                        );
                    }
                    return x;
                }
            );
    }
}
