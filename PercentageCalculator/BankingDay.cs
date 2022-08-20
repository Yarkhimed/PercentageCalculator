namespace PercentageCalculator
{
    internal record BankingDay
    {
        public DateTime Day { get; set; }
        public decimal Credit { get; set; }
        public decimal Debit { get; set; }
        public decimal MinimumBalance { get; set; }
        public decimal EndDayBalance { get; set; }
        public List<Transaction> Transactions { get; set; }
    }
}
