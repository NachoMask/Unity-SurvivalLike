using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "SurvivalLike/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [SerializeField] private EnemyCharacter prefab;
    [SerializeField, Min(1)] private int maxHp = 1;
    [SerializeField, Min(0f)] private float moveSpeed = 1f;

    public EnemyCharacter Prefab => prefab;
    public int MaxHp => maxHp;
    public float MoveSpeed => moveSpeed;
}
