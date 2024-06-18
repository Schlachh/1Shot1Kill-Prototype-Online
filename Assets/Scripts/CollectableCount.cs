using UnityEngine;

public class CollectableCount : MonoBehaviour
{
    TMPro.TMP_Text text;
    int count;

    void Awake()
    {
        text = GetComponent<TMPro.TMP_Text>();
    }

    void OnEnable()
    {
        Collectable.OnCollected += OnCollectableCollected;
    }
    void OnDisable()
    {
        Collectable.OnCollected -= OnCollectableCollected;
    }
    void OnCollectableCollected()
    {
        text.text = (++count).ToString();
    }
}
