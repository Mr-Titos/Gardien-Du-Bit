namespace common_gardienbit.DTO.Exception
{
    public abstract class AbstractException : System.Exception
    {
        public int statusCode;
        public string? message;
        public object Serialize()
        {
            return new { message, statusCode };
        }
    }
}
