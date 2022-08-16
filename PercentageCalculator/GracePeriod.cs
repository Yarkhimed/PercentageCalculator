namespace PercentageCalculator
{
    internal class GracePeriod
    {
        GracePeriod(DateTime startDate, Account account)
        {
            StartDate = startDate;
            DateTime temp = startDate.AddMonths(2);
            Deadline = new DateTime(temp.Year, temp.Month, 1).AddDays(-1);
        }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime Deadline { get; set; }
        public bool Fulfilled { get; set; }
        public decimal SumToRepay { get; set; }
        public decimal Overpayment { get; set; }
    }
}
