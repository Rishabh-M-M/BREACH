using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// EnemyStats (ScriptableObject)
    /// GRASP: Protected Variations - enemy values externalized to asset files
    /// Create via: Assets → Create → ProjectBreach → EnemyStats
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyStats", menuName = "ProjectBreach/EnemyStats")]
    public class EnemyStats : ScriptableObject
    {
        [Header("Base Values")]
        public int hp = 100;
        public int damage = 10;
        public float moveSpeed = 2f;
        public float attackSpeed = 1f;
        public int coreDamage = 10;

        [Header("Type")]
        public bool isRanged = false;
    }
}