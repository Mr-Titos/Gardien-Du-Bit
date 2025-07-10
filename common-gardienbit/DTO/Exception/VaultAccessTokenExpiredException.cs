namespace common_gardienbit.DTO.Exception
{
    public class VaultAccessTokenExpiredException : AbstractException
    {
        public VaultAccessTokenExpiredException()
        {
            this.statusCode = 403;
            this.message = $"Token expired";
        }
    }
}