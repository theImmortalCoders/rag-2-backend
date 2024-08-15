using System.Text;

namespace rag_2_backend.Utils;

public class TokenGenerationUtil
{
    public static string CreatePassword(int length)
    {
        const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        var res = new StringBuilder();
        var rnd = new Random();
        while (0 < length--)
        {
            res.Append(valid[rnd.Next(valid.Length)]);
        }

        return res.ToString();
    }
}