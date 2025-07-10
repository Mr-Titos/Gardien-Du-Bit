namespace common_gardienbit.DTO.Exception
{
    public class ParsingEncryptedParameterException : AbstractException
    {
        public ParsingEncryptedParameterException(string[] parametersName)
        {
            this.statusCode = 400;
            this.message = $"Parameter {string.Join(",", parametersName)} incorrect while decrypting/parsing";
        }
    }
}