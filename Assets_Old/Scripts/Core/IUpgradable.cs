namespace ProjectBreach.Core
{
    public interface IUpgradable
    {
        int Level { get; }

        /// <summary>
        /// Apply one upgrade tier. Returns false when the upgrade cannot be applied
        /// (already at max level, for example) -- the caller must not charge for a
        /// rejected upgrade.
        /// </summary>
        bool TryApplyUpgrade(UpgradeData data);
    }
}
