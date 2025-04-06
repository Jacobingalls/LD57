using UnityEngine;

public class SizeProvider : MonoBehaviour
{
    public Vector3 size
    {
        get
        {
            var childSprite = GetComponentInChildren<SpriteRenderer>();
            if (childSprite == null)
            {
                return Vector3.zero;
            }
            return childSprite.sprite.bounds.size;
        }
    }
}
