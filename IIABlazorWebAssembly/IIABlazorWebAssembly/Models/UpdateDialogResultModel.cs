namespace IIABlazorWebAssembly.Models
{
    public class UpdateDialogResultModel
    {
        private Guid _vauId;
        public Guid vauId
        {
            get { return _vauId; }
            set { _vauId = value; }
        }

        private string? _vauName;
        public string? vauName
        {
            get { return _vauName; }
            set { _vauName = value; }
        }

        private bool _vauFavorite;
        public bool vauFavorite
        {
            get { return _vauFavorite; }
            set { _vauFavorite = false; }
        }

        private string? _vauPassword;
        public string? vauPassword
        {
            get { return _vauPassword; }
            set { _vauPassword = value; }
        }
    }
}
