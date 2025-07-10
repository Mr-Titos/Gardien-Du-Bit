namespace common_gardienbit.DTO.Exception
{
    public class WrongVaultPasswordException : AbstractException
    {
        public WrongVaultPasswordException()
        {
            this.statusCode = 403;
            this.message = "Password missing or incorrect";
        }
    }
}