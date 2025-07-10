namespace api_gardienbit.Models
{
    public class LogAction : IDataModel
    {
        #region Attributes
        public Guid LoaId { get; set; }
        public required string LoaName { get; set; }

        public ICollection<Log> Logs { get; set; } = [];

        #endregion
    }
}