namespace ProjectBreach.Core
{
    public interface IDamageable
    {
        int MaxHP { get; }
        int CurrentHP { get; }
        bool IsDead { get; }
        void TakeDamage(int amount);
        void Die();
    }
}