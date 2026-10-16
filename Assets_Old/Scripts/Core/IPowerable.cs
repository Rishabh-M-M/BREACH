namespace ProjectBreach.Core
{
    public interface IPowerable
    {
        int PowerCost { get; }
        bool IsActive { get; }
        void TurnOn();
        void TurnOff();
    }
}