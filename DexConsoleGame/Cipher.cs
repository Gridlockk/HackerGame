namespace DexConsoleGame;

public class Cipher
{
    public static string Decrypt(string text, int key)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        char[] result = new char[text.Length];
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (c >= 'a' && c <= 'z')
                result[i] = (char)('a' + (c - 'a' - key + 26) % 26);
            else if (c >= 'A' && c <= 'Z')
                result[i] = (char)('A' + (c - 'A' - key + 26) % 26);
            else
                result[i] = c;
        }
        return new string(result);
    }
}