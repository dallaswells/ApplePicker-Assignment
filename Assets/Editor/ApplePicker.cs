
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


//11:22 before turn in due, so many errors were fixed
public static class CherryPickerSetup
{
    const string Root = "Assets/Generated";

    [MenuItem("Cherry Picker/Create assignment scene")]
    public static void Create()
    {
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Directory.CreateDirectory(Root);
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Prefabs");
        AddTag("cherry");
        SetLayer(8, "Tree");
        SetLayer(9, "Cherry");

        Physics2D.IgnoreLayerCollision(8, 9, true);

        Sprite treeSprite = LoadSprite("tree.png", 2f);
        Sprite fruitSprite = LoadSprite("apple.png", .45f);
        Sprite basketSprite = LoadSprite("basket.png", .8f);
        Sprite bombSprite = LoadSprite("bomb.png", .65f);
        AudioClip drop = MakeSound("drop-replacement", 700, 180, .18f);
        AudioClip catchSound = MakeSound("catch-replacement", 900, 1300, .16f);
        AudioClip missSound = LoadSound("spongebob-fail.mp3");
        AudioClip bombSound = LoadSound("oh-great-heavenssss.mp3");

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camObject.tag = "MainCamera";
        camObject.transform.position = new Vector3(0, 0, -10);
        Camera camera = camObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(.18f,.26f,.41f);

        var apple = new GameObject("apple", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(Rigidbody2D));
        apple.AddComponent<FallingItem>();
        apple.tag = "cherry";
        apple.layer = 9;
        apple.GetComponent<SpriteRenderer>().sprite = fruitSprite;
        apple.GetComponent<SpriteRenderer>().sortingOrder = 2;
        apple.GetComponent<CircleCollider2D>().radius = .18f;
        apple.GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var soundChild = new GameObject("Drop sound", typeof(AudioSource));
        soundChild.transform.SetParent(apple.transform, false);
        ConfigureAudio(soundChild.GetComponent<AudioSource>(), drop, true);
        GameObject applePreFab = PrefabUtility.SaveAsPrefabAsset(apple, "Assets/Prefabs/apple.prefab");
        UnityEngine.Object.DestroyImmediate(apple);

        var bomb = new GameObject("bomb", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(FallingItem));
        bomb.layer = 9;
        bomb.GetComponent<FallingItem>().isBomb = true;
        bomb.GetComponent<SpriteRenderer>().sprite = bombSprite;
        bomb.GetComponent<SpriteRenderer>().sortingOrder = 2;
        bomb.GetComponent<BoxCollider2D>().size = new Vector2(.42f,.5f);
        bomb.GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        GameObject BombPrefab = PrefabUtility.SaveAsPrefabAsset(bomb, "Assets/Prefabs/bomb.prefab");
        UnityEngine.Object.DestroyImmediate(bomb);

        var basket = new GameObject("basket", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(BasketBehavior));
        basket.GetComponent<SpriteRenderer>().sprite = basketSprite;
        basket.GetComponent<BoxCollider2D>().size = new Vector2(.75f,.38f);
        basket.GetComponent<BoxCollider2D>().offset = new Vector2(0,-.16f);
        GameObject basketPrefab = PrefabUtility.SaveAsPrefabAsset(basket, "Assets/Prefabs/basket.prefab");
        UnityEngine.Object.DestroyImmediate(basket);

        var tree = new GameObject("tree", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D), typeof(Tree));
        tree.layer = 8;
        tree.transform.position = new Vector3(0,3,0);
        tree.GetComponent<SpriteRenderer>().sprite = treeSprite;
        tree.GetComponent<BoxCollider2D>().size = new Vector2(1.8f,1.9f);
        Rigidbody2D treeBody = tree.GetComponent<Rigidbody2D>();
        treeBody.gravityScale = 0;
        treeBody.mass = .0001f;
        treeBody.constraints = RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
        tree.GetComponent<Tree>().applePreFab = applePreFab;
        tree.GetComponent<Tree>().BombPrefab = BombPrefab;

        var bounds = new GameObject("Bounds", typeof(EdgeCollider2D), typeof(BoundaryBehavior));
        bounds.transform.position = new Vector3(0,-5,0);
        bounds.GetComponent<EdgeCollider2D>().points = new [] {
            new Vector2(-8.8f,10), new Vector2(-8.8f,0), new Vector2(8.8f,0), new Vector2(8.8f,10)
        };
        bounds.GetComponent<BoundaryBehavior>().basketPrefab = basketPrefab;
        ConfigureAudio(bounds.AddComponent<AudioSource>(), catchSound, false);
        ConfigureAudio(bounds.AddComponent<AudioSource>(), missSound, false);
        ConfigureAudio(bounds.AddComponent<AudioSource>(), bombSound, false);

        var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280,720);
        scaler.matchWidthOrHeight = .5f;
        MakeText(canvasObject.transform, "scoreOutput", "0", new Vector2(1,0), new Vector2(-90,30), new Vector2(150,40), TextAnchor.MiddleRight, 26);
        MakeText(canvasObject.transform, "highScoreOutput", "1000", new Vector2(0,0), new Vector2(240,30), new Vector2(130,40), TextAnchor.MiddleLeft, 26);
        MakeText(canvasObject.transform, "High score label", "High Score:", new Vector2(0,0), new Vector2(90,30), new Vector2(165,40), TextAnchor.MiddleLeft, 26);
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        PlayerSettings.companyName = "TutorialReconstruction";
        PlayerSettings.productName = "Apple Picker Assignment";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        EditorSettings.defaultBehaviorMode = EditorBehaviorMode.Mode2D;
        if (SceneView.lastActiveSceneView != null) SceneView.lastActiveSceneView.in2DMode = true;
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/MainScene.unity");
        EditorBuildSettings.scenes = new [] {new EditorBuildSettingsScene("Assets/Scenes/MainScene.unity", true)};
        AssetDatabase.SaveAssets();
        Debug.Log("CHERRY_PICKER_SETUP_OK: Scene, prefabs, custom artwork, audio and build settings created.");
    }

    static Sprite LoadSprite(string filename, float worldHeight)
    {
        string path = "Assets/UserAssets/" + filename;
        AssetDatabase.ImportAsset(path);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        if (importer == null) throw new FileNotFoundException("Missing sprite", path);
        int width, height;
        importer.GetSourceTextureWidthAndHeight(out width, out height);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = height / worldHeight;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static AudioClip LoadSound(string filename)
    {
        string path = "Assets/UserAssets/" + filename;
        AssetDatabase.ImportAsset(path);
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
        if (clip == null) throw new FileNotFoundException("Missing audio clip", path);
        return clip;
    }

    static AudioClip MakeSound(string name, float start, float end, float duration)
    {
        string path = Root + "/" + name + ".wav";
        int count = (int)(44100 * duration);
        using (var writer = new BinaryWriter(File.Create(path)))
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36+count*2);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16);
            writer.Write((short)1); writer.Write((short)1); writer.Write(44100); writer.Write(88200);
            writer.Write((short)2); writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(count*2);
            double phase = 0;
            for(int i=0;i<count;i++)
            {
                double t=(double)i/count;
                phase += 2*Math.PI*(start+(end-start)*t)/44100;
                double envelope=Math.Min(1,t*30)*Math.Pow(1-t,2);
                writer.Write((short)(Math.Sin(phase)*envelope*9000));
            }
        }
        AssetDatabase.ImportAsset(path);
        return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }

    static void ConfigureAudio(AudioSource source, AudioClip clip, bool autoplay)
    {
        source.clip = clip; source.playOnAwake = autoplay; source.loop = false; source.spatialBlend = 0; source.volume = .5f;
    }

    static void MakeText(Transform parent,string name,string value,Vector2 anchor,Vector2 pos,Vector2 size,TextAnchor align,int fontSize)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Text));
        go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>();
        rect.anchorMin=rect.anchorMax=anchor; rect.sizeDelta=size; rect.anchoredPosition=pos;
        var text=go.GetComponent<Text>();
#if UNITY_2022_2_OR_NEWER
        text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
        text.font=Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
        text.text=value; text.fontSize=fontSize; text.alignment=align; text.color=Color.white; text.raycastTarget=false;
    }

    static void AddTag(string tag)
    {
        var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var tags=settings.FindProperty("tags");
        for(int i=0;i<tags.arraySize;i++) if(tags.GetArrayElementAtIndex(i).stringValue==tag) return;
        tags.InsertArrayElementAtIndex(tags.arraySize); tags.GetArrayElementAtIndex(tags.arraySize-1).stringValue=tag;
        settings.ApplyModifiedProperties();
    }

    static void SetLayer(int index,string name)
    {
        var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        settings.FindProperty("layers").GetArrayElementAtIndex(index).stringValue=name;
        settings.ApplyModifiedProperties();
    }
}

