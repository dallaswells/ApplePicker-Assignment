using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BoundaryBehavior : MonoBehaviour
{
    public int score;
    public int highScore;
    public GameObject basketPrefab;
    public bool IsPlaying { get { return rules.IsPlaying; } }
    private readonly GameRules rules = new GameRules();
    private readonly List<GameObject> baskets = new List<GameObject>();
    private Tree spawnApple;
    private AudioSource[] sounds;
    private Text scorekeeper, highscoretext, roundText, endText;
    private GameObject startPanel, endPanel;
    void Start()
    {
        spawnApple = FindObjectOfType<Tree>();
        sounds = GetComponents<AudioSource>();
        scorekeeper = GameObject.Find("scoreOutput").GetComponent<Text>();
        highscoretext = GameObject.Find("highScoreOutput").GetComponent<Text>();
        highScore = PlayerPrefs.GetInt("HS",1000);
        BuildUI();
        spawnApple.StopDrops();
        startPanel.SetActive(true); endPanel.SetActive(false);
        roundText.text = "Round 1";
        RefreshScore();
    }
    public void StartGame()
    {
        if (IsPlaying) return;
        ClearFalling();
        foreach (var hoop in baskets) if (hoop != null) { hoop.SetActive(false); Destroy(hoop); }
        foreach (var sound in sounds) sound.Stop();
        baskets.Clear(); rules.Begin(); score = 0;
        for (int i=0;i<GameRules.BasketCount;i++)
        {
            var hoop = Instantiate(basketPrefab);
            hoop.transform.position = new Vector3(0,transform.position.y + .5f + i*.85f,0);
            baskets.Add(hoop);
        }
        startPanel.SetActive(false); endPanel.SetActive(false);
        roundText.text = "Round 1"; RefreshScore();
        spawnApple.ResetTree(); spawnApple.BeginRound();
        EventSystem.current?.SetSelectedGameObject(null);
    }
    public void CatchItem(FallingItem item)
    {
        if (!IsPlaying) return;
        if (item.isBomb) { rules.CatchBomb(); EndGame("You caught a bomb!", 2); }
        else { rules.CatchApple(); score = rules.Score; PlaySound(0); RefreshScore(); }
        if (item != null) { item.gameObject.SetActive(false); Destroy(item.gameObject); }
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsPlaying) return;
        var item = collision.gameObject.GetComponent<FallingItem>();
        if (item == null || !item.TryConsume()) return;
        if (item.isBomb) { item.gameObject.SetActive(false); Destroy(item.gameObject); return; }
        rules.MissApple();
        int last = baskets.Count - 1;
        if (last >= 0) { baskets[last].SetActive(false); Destroy(baskets[last]); baskets.RemoveAt(last); }
        ClearFalling();
        if (!IsPlaying) EndGame("You used all four baskets.");
        else { PlaySound(1); roundText.text = "Round " + rules.Round; spawnApple.BeginRound(); }
    }
    void EndGame(string reason, int soundIndex = 1)
    {
        spawnApple.StopDrops(); ClearFalling(); PlaySound(soundIndex);
        roundText.text = "Game Over";
        endText.text = reason + "\nScore: " + score;
        endPanel.SetActive(true); SaveHighScore();
        EventSystem.current?.SetSelectedGameObject(endPanel.GetComponentInChildren<Button>().gameObject);
    }
    void ClearFalling()
    {
        foreach (var item in FindObjectsOfType<FallingItem>())
        { item.TryConsume(); item.gameObject.SetActive(false); Destroy(item.gameObject); }
    }
    void RefreshScore()
    { highScore = Mathf.Max(highScore,score); scorekeeper.text = score.ToString(); highscoretext.text = highScore.ToString(); }
    void PlaySound(int index) { if (sounds.Length > index) sounds[index].Play(); }
    void SaveHighScore() { PlayerPrefs.SetInt("HS",highScore); PlayerPrefs.Save(); }
    void OnApplicationQuit() { SaveHighScore(); }
    void BuildUI()
    {
        var canvas = scorekeeper.canvas.transform;
        roundText = Label(canvas,"RoundDisplay","Round 1",new Vector2(0,-40),new Vector2(400,60),34);
        roundText.rectTransform.anchorMin = roundText.rectTransform.anchorMax = new Vector2(.5f,1);
        startPanel = Panel(canvas,"StartScreen");
        Label(startPanel.transform,"Title","APPLE PICKER",new Vector2(0,115),new Vector2(700,90),54);
        Label(startPanel.transform,"Instructions","Move your mouse to catch apples.\nFour baskets. Four rounds. Avoid the bombs!",new Vector2(0,10),new Vector2(950,100),25);
        var start = MakeButton(startPanel.transform,"StartButton","Start",new Vector2(0,-110));
        start.onClick.AddListener(StartGame);
        endPanel = Panel(canvas,"GameOverScreen");
        Label(endPanel.transform,"EndTitle","GAME OVER",new Vector2(0,115),new Vector2(700,90),48);
        endText = Label(endPanel.transform,"EndReason","",new Vector2(0,15),new Vector2(850,100),28);
        var restart = MakeButton(endPanel.transform,"RestartButton","Restart",new Vector2(0,-110));
        restart.onClick.AddListener(StartGame);
        roundText.transform.SetAsLastSibling();
        EventSystem.current?.SetSelectedGameObject(start.gameObject);
    }
    static GameObject Panel(Transform parent,string name)
    {
        var go = new GameObject(name,typeof(RectTransform),typeof(Image)); go.transform.SetParent(parent,false);
        var rect = go.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = new Color(.04f,.08f,.15f,.95f); return go;
    }
    static Text Label(Transform parent,string name,string value,Vector2 pos,Vector2 size,int fontSize)
    {
        var go = new GameObject(name,typeof(RectTransform),typeof(Text)); go.transform.SetParent(parent,false);
        var text = go.GetComponent<Text>();
#if UNITY_2022_2_OR_NEWER
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
#else
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
#endif
        text.rectTransform.sizeDelta = size; text.rectTransform.anchoredPosition = pos;
        text.text = value; text.fontSize = fontSize; text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter; text.raycastTarget = false; return text;
    }
    static Button MakeButton(Transform parent,string name,string label,Vector2 pos)
    {
        var go = new GameObject(name,typeof(RectTransform),typeof(Image),typeof(Button)); go.transform.SetParent(parent,false);
        var rect = go.GetComponent<RectTransform>(); rect.sizeDelta = new Vector2(240,65); rect.anchoredPosition = pos;
        go.GetComponent<Image>().color = new Color(.15f,.48f,.28f);
        var button = go.GetComponent<Button>(); button.targetGraphic = go.GetComponent<Image>();
        Label(go.transform,"Label",label,Vector2.zero,new Vector2(240,65),30); return button;
    }
}


