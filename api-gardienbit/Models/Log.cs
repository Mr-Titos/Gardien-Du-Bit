using System.ComponentModel.DataAnnotations;

namespace api_gardienbit.Models
{
    public class Log : IDataModel
    {
        #region Attributes

        [Key]
        public Guid LogId { get; set; }
        public required string LogCliEntraId { get; set; }
        public required string LogCliId { get; set; }
        public required string LogVauId { get; set; }
        public required string LogVauName { get; set; }
        public required string LogPwpId { get; set; }
        public DateTime LogEntryDate { get; set; }
        public required LogAction LogAction { get; set; }
        #endregion
    }
}