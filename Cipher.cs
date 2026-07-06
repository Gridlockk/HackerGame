namespace DexConsoleGame;
public class Cipher
{
    public static string Decrypt(string text, int key)
    {
        if (text == null || text == "")
        {
            return text;
        }

        string result = "";

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (c >= 'a' && c <= 'z')
            {
                int pos = c - 'a';
                pos = pos - key;

                while (pos < 0)
                {
                    pos += 26;
                }

                result += (char)('a' + pos);
            }
            else if (c >= 'A' && c <= 'Z')
            {
                int pos = c - 'A';
                pos = pos - key;

                while (pos < 0)
                {
                    pos += 26;
                }

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