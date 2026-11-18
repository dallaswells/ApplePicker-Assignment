using UnityEngine;
public class FallingItem : MonoBehaviour
{
    [UnityEngine.Serialization.FormerlySerializedAs("isBranch")] public bool isBomb;
    private bool consumed;
    public bool TryConsume()
    {
        if (consumed || !gameObject.activeInHierarchy) return false;
        consumed = true;
        foreach (var collider in GetComponentsInChildren<Collider2D>()) collider.enabled = false;
        return true;
    }
}
