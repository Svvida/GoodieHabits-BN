namespace Domain.Exceptions
{
    public class InvalidArgumentException : AppException
    {
        public InvalidArgumentException(string message) : base(message, 400) { }
    }
}
