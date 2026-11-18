using UnityEngine;
using UnityEngine.Serialization;
public class Tree : MonoBehaviour
{
    public float treeSpeed = 2f;
    [FormerlySerializedAs("cherryPrefab")]
    public GameObject applePreFab;
    [FormerlySerializedAs("branchPrefab")]
    public GameObject BombPrefab;
    public bool gameOver = true;
    private float direction = 1f;
    private float initialY;
    void Awake() { initialY = transform.position.y; }
    public void BeginRound()
    {
        //stopdropandrollll
        StopDrops(); gameOver = false;
        Invoke(nameof(DropApple), 1f);
        Invoke(nameof(DropBomb), Random.Range(8f,12f));
    }
    public void StopDrops() { gameOver = true; CancelInvoke(); }
    public void ResetTree()
    {
        StopDrops(); direction = 1f;
        transform.position = new Vector3(0,initialY,0);
        var body = GetComponent<Rigidbody2D>();
        if (body != null) { body.velocity = Vector2.zero; body.angularVelocity = 0; }
    }
    void DropApple() { if (gameOver) return; Spawn(applePreFab,false); Invoke(nameof(DropApple),1f); }
    void DropBomb() { if (gameOver) return; Spawn(BombPrefab,true); Invoke(nameof(DropBomb),Random.Range(8f,12f)); }
    void Spawn(GameObject appleToSpawn, bool bomb)
    {
        if (appleToSpawn == null) { Debug.LogError("Assign apple and bomb prefabs on tree."); return; }
        Vector3 spawnPosition = transform.position;
        var item = Instantiate(appleToSpawn,spawnPosition,Quaternion.identity);
        var falling = item.GetComponent<FallingItem>();
        if (falling == null) falling = item.AddComponent<FallingItem>();
        falling.isBomb = bomb;
        foreach (var own in GetComponentsInChildren<Collider2D>())
            foreach (var other in item.GetComponentsInChildren<Collider2D>()) Physics2D.IgnoreCollision(own,other);
    }
    void Update()
    {
        if (gameOver) return;
        float limit = Mathf.Min(7.8f,Camera.main.orthographicSize * Camera.main.aspect - 1f);
        Vector3 pos = transform.position;
        pos.x += Mathf.Abs(treeSpeed) * direction * Time.deltaTime;
        if (pos.x >= limit) { pos.x = limit; direction = -1f; }
        if (pos.x <= -limit) { pos.x = -limit; direction = 1f; }
        transform.position = pos;
    }
}

