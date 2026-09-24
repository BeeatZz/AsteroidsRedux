namespace Asteroids.Combat
{
    public interface IDamageable
    {
        void TakeDamage(int amount, bool awardScore = true);
    }
}
