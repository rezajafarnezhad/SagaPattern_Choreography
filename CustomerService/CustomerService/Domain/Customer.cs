namespace CustomerService.Domain;

public class Customer
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Credit { get; set; }


    public void UpdateCredit(decimal orderTotal)
    {
        Credit -= orderTotal;
    }
}