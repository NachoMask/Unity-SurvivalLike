using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "SurvivalLike/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [SerializeField] private EnemyCharacter prefab;
    [SerializeField, Min(1)] private int maxHp = 1;
    [SerializeField, Min(0f)] private float moveSpeed = 0f;

    public EnemyCharacter Prefab => prefab;
    public int MaxHp => maxHp;
    public float MoveSpeed => moveSpeed;

    public bool TryValidateSettings(out string error)
    {
        if (prefab == null)
        {
            error = $"{nameof(prefab)} is Invalid";
            return false;
        }
        if (maxHp < 1)
        {
            error = $"{nameof(maxHp)} must be at least 1";
            return false;
        }
        if (moveSpeed < 0f)
        {
            error = $"{nameof(moveSpeed)} must be at least 0.0";
            return false;
        }

        error = null;
        return true;
    }
}
