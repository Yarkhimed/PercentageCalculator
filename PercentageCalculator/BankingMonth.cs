namespace PercentageCalculator
{
    internal class BankingMonth
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal DailyInterest { get; set; }
        public bool IsInterestCharged { get; set; }
        public bool IsDebtRepaid { get; set; }
        public DateTime RepaymentDay { get; set; }
    }
}
