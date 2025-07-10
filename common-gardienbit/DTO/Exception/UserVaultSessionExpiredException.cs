namespace common_gardienbit.DTO.Exception
{
    public class UserVaultSessionExpiredException : AbstractException
    {
        public UserVaultSessionExpiredException()
        {
            this.statusCode = 403;
            this.message = $"User session expired";
        }
    }
}