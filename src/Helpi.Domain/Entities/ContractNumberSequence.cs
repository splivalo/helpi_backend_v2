namespace Helpi.Domain.Entities
{
    public class ContractNumberSequence
    {
        public int Id { get; private set; }
        public int NextNumber { get; private set; } = 1000;

        // For EF Core
        private ContractNumberSequence() { }

        public ContractNumberSequence(int id = 1, int startingNumber = 1000)
        {
            Id = id;
            NextNumber = startingNumber;
        }

        public int GetNextNumber()
        {
            int currentNumber = NextNumber;
            NextNumber++;
            return currentNumber;
        }
    }
}
