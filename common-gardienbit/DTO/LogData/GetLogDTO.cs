namespace common_gardienbit.DTO.Log
{
    public class GetLogDTO
    {
        public Guid logId { get; set; }
        public string logCliEntraId { get; set; } = string.Empty;
        public string logCliId { get; set; } = string.Empty;
        public string logVauId { get; set; } = string.Empty;
        public string logVauName { get; set; } = string.Empty;
        public string logPwpId { get; set; } = string.Empty;
        public DateTime logEntryDate { get; set; }
        public GetLogActionDTO? logAction { get; set; } 
    }
}
