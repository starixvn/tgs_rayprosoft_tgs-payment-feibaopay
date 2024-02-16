namespace Tgs.Payment.FeibaoPay;

public class ApiFailException : Exception
{
    public ApiFailException(string? message) : base(message)
    {
    }
}
