namespace Tgs.Payment.FeibaoPay;

public class ApiFailException(string? message) : Exception(message)
{
}
