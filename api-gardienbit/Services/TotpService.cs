using api_gardienbit.Models;
using OtpNet;
using QRCoder;

namespace api_gardienbit.Services
{
    public class TotpService
    {
        public string GetQrCode(Vault vault)
        {
            string secret = Base32Encoding.ToString(vault.VauTOTP);

            string issuer = "Gardien du Bit";
            string uri = $"otpauth://totp/{issuer}:{vault.VauName}?secret={secret}&issuer={issuer}";

            // Génère le QR Code en image
            using var qrGenerator = new QRCodeGenerator();
            using var qrData = qrGenerator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new SvgQRCode(qrData);
            string svgImage = qrCode.GetGraphic(4);

            return svgImage;
        }
    }
}