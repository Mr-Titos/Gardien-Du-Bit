namespace common_gardienbit.DTO.Exception
{
    public class ParameterMissingOrIncorrectException : AbstractException
    {
        public ParameterMissingOrIncorrectException(string[] parametersName)
        {
            this.statusCode = 400;
            this.message = $"Parameter {string.Join(",", parametersName)} missing or incorrect";
        }
    }
}