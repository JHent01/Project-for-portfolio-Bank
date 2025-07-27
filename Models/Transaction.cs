namespace Bank.Models
{
    public record class Transaction
    (
        int Id,
         TransactionType TransactionType,
        decimal Amount,
        DateTime TransactionDate,
      
        string NumberAccount,
        decimal BalanceAfterTransaction
    );
}
