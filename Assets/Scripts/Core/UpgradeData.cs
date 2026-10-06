using UnityEngine;

namespace ProjectBreach.Core
{
    /// <summary>
    /// Data for one upgrade tier (TeamDoc 8.5 / 9.4).
    ///
    /// GRASP: Information Expert + Indirection -- the numbers live in an asset so
    /// design can rebalance without touching code, and so the shop UI and the
    /// thing being upgraded do not need to know about each other.
    /// </summary>
    [CreateAssetMenu(fileName = "NewUpgradeData", menuName = "ProjectBreach/UpgradeData")]
    public class UpgradeData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string upgradeName = "Upgrade";
        [TextArea]
        [SerializeField] private string description = "";

        [Header("Cost")]
        [Tooltip("Upgrade points spent on purchase. Spec default: 50.")]
        [SerializeField] private int cost = 50;

        [Header("Bonuses")]
        [Tooltip("Flat HP added to the target. Spec default: +20.")]
        [SerializeField] private int hpBonus = 20;

        [Tooltip("Flat damage added to the target. Spec default: +10.")]
        [SerializeField] private int damageBonus = 10;

        [Header("Limits")]
        [Tooltip("Highest level this upgrade can be bought up to.")]
        [SerializeField] private int maxLevel = 3;

        public string UpgradeName => upgradeName;
        public string Description => description;
        public int Cost => cost;
        public int HpBonus => hpBonus;
        public int DamageBonus => damageBonus;
        public int MaxLevel => maxLevel;
    }
}
