namespace Bank.Models
{
    public record class Transaction
    (
        int Id,
         TransactionType TransactionType,
        decimal Amount,
        DateTime TransactionDate,
       // string Description,
        string NumberAccount,
        decimal BalanceAfterTransaction
    );
}
