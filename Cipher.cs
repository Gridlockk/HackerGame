
public class Cipher
{
    public static string Decrypt(string text, int key)
    {
        if (string.IsNullOrEmpty(text))
            return text ?? "";

        string result = "";
        foreach (char c in text)
        {
            if (c >= 'a' && c <= 'z')
            {
                int pos = ((c - 'a' - key) % 26 + 26) % 26;
                result += (char)('a' + pos);
            }
            else if (c >= 'A' && c <= 'Z')
            {
                int pos = ((c - 'A' - key) % 26 + 26) % 26;
                result += (char)('A' + pos);
            }
            else
            {
                result += c;
            }
        }
        return result;
    }
}