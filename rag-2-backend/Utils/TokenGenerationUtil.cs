#region

using System.Text;

#endregion

namespace rag_2_backend.Utils;

public abstract class TokenGenerationUtil
{
    public static string GenerateToken(int length)
    {
        const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        var res = new StringBuilder();
        var rnd = new Random();
        while (0 < length--) res.Append(valid[rnd.Next(valid.Length)]);

        return res.ToString();
    }
}