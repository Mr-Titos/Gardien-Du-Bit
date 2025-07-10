using System.Security.Cryptography;
using System.Text;

public class TokenGenerator
{
    public static string GenerateSecureToken(int length = 200)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var token = new StringBuilder();
        using (var rng = RandomNumberGenerator.Create())
        {
            var data = new byte[sizeof(uint)];

            while (token.Length < length)
            {
                rng.GetBytes(data);
                uint value = BitConverter.ToUInt32(data, 0);
                token.Append(chars[(int)(value % (uint)chars.Length)]);
            }
        }

        return token.ToString();
    }
}