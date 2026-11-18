using UnityEngine;
public class BasketBehavior : MonoBehaviour
{
    private BoundaryBehavior manager;
    void Start() { manager = FindObjectOfType<BoundaryBehavior>(); }
    void Update()
    {
        if (manager == null || !manager.IsPlaying || Camera.main == null) return;
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 pos = transform.position;
        float limit = Mathf.Min(8.15f,Camera.main.orthographicSize * Camera.main.aspect - .65f);
        float mouseX = Mathf.Clamp(mousePosition.x,-limit,limit);
        pos.x = mouseX;
        transform.position = pos;
    }
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (manager == null || !manager.IsPlaying) return;
        var item = collision.gameObject.GetComponent<FallingItem>();
        if (item != null && item.TryConsume()) manager.CatchItem(item);
    }
}

