using MudBlazor;

namespace IIABlazorWebAssembly.Services
{
    public class MudAlertService
    {
        public event Action<AlertMessage> OnAlert = default!;

        public void ShowAlert(string message, Severity severity = Severity.Info, Variant variant = Variant.Filled, bool showIcon = true)
        {
            OnAlert?.DynamicInvoke(new AlertMessage(message, severity, variant, showIcon));
        }

        public class AlertMessage(string message, Severity severity, Variant variant, bool showIcon)
        {
            public string Message { get; } = message;
            public Severity Severity { get; } = severity;
            public Variant Variant { get; } = variant;
            public bool ShowIcon { get; } = showIcon;
        }
    }
}
