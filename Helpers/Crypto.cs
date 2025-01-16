using System.Security.Cryptography;
using System.Text;

namespace MyProject.Helpers;

public static class Crypto
  {
    private static readonly TripleDESCryptoServiceProvider tdProvider = new TripleDESCryptoServiceProvider();
    private static readonly UTF8Encoding stringHandler = new UTF8Encoding();
    private static readonly byte[] localKey = new byte[24]
    {
      (byte) 33,
      (byte) 8,
      (byte) 13,
      (byte) 55,
      (byte) 16,
      (byte) 91,
      (byte) 77,
      (byte) 24,
      (byte) 41,
      (byte) 11,
      (byte) 35,
      (byte) 81,
      (byte) 52,
      (byte) 3,
      (byte) 22,
      (byte) 68,
      (byte) 53,
      (byte) 46,
      (byte) 82,
      (byte) 76,
      (byte) 79,
      (byte) 1,
      (byte) 93,
      (byte) 72
    };
    private static readonly byte[] localVector = new byte[8]
    {
      (byte) 13,
      (byte) 68,
      (byte) 24,
      (byte) 99,
      (byte) 37,
      (byte) 9,
      (byte) 13,
      (byte) 49
    };

    public static string GenerateSALT()
    {
      byte[] numArray = new byte[6];
      new RNGCryptoServiceProvider().GetBytes(numArray);
      return Convert.ToBase64String(numArray);
    }

    public static string EncryptPassword(string username, string pwd, out string esalt)
    {
      string salt = Crypto.GenerateSALT();
      esalt = salt;
      return Convert.ToBase64String(new SHA384Managed().ComputeHash(new UTF8Encoding().GetBytes(username + salt + pwd)));
    }

    public static string EncryptPassword(string username, string pwd, string salt)
    {
      return Convert.ToBase64String(Crypto.EncryptPasswordAsByte(username, pwd, salt));
    }

    public static byte[] EncryptPasswordAsByte(string username, string pwd, string salt)
    {
      return new SHA384Managed().ComputeHash(new UTF8Encoding().GetBytes(username + salt + pwd));
    }

    public static string EncryptString(string plainText)
    {
      byte[] numArray = new byte[16];
      byte[] inArray = (byte[]) null;
      using (Aes aes = Aes.Create())
      {
        aes.Key = Encoding.UTF8.GetBytes("1765687535f811eb8c9954e1ad904c93");
        aes.IV = numArray;
        ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using (MemoryStream memoryStream = new MemoryStream())
        {
          using (CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, encryptor, CryptoStreamMode.Write))
          {
            using (StreamWriter streamWriter = new StreamWriter((Stream) cryptoStream))
              streamWriter.Write(plainText);
            inArray = memoryStream.ToArray();
          }
        }
      }
      return Convert.ToBase64String(inArray);
    }

    public static string DecryptString(string encryptedText)
    {
      byte[] numArray = new byte[16];
      byte[] buffer = Convert.FromBase64String(encryptedText);
      using (Aes aes = Aes.Create())
      {
        aes.Key = Encoding.UTF8.GetBytes("1765687535f811eb8c9954e1ad904c93");
        aes.IV = numArray;
        ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using (MemoryStream memoryStream = new MemoryStream(buffer))
        {
          using (CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, decryptor, CryptoStreamMode.Read))
          {
            using (StreamReader streamReader = new StreamReader((Stream) cryptoStream))
              return streamReader.ReadToEnd();
          }
        }
      }
    }

    public static string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password);

    public static bool VerifyPassword(string password, string passwordHash)
    {
      return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }

    public static string CreateMD5String(string inputString)
    {
      byte[] hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(inputString));
      StringBuilder stringBuilder = new StringBuilder();
      for (int index = 0; index < hash.Length; ++index)
        stringBuilder.Append(hash[index].ToString("X2"));
      return stringBuilder.ToString();
    }

    private static byte[] Transform(byte[] input, ICryptoTransform cryptoTransform)
    {
      MemoryStream memoryStream = new MemoryStream();
      CryptoStream cryptoStream = new CryptoStream((Stream) memoryStream, cryptoTransform, CryptoStreamMode.Write);
      cryptoStream.Write(input, 0, input.Length);
      cryptoStream.FlushFinalBlock();
      memoryStream.Position = 0L;
      byte[] buffer = new byte[(int) memoryStream.Length - 1 + 1];
      memoryStream.Read(buffer, 0, buffer.Length);
      memoryStream.Close();
      cryptoStream.Close();
      return buffer;
    }

    public static string EncryptSMSPassword(string text)
    {
      return Convert.ToBase64String(Crypto.Transform(Crypto.stringHandler.GetBytes(text), Crypto.tdProvider.CreateEncryptor(Crypto.localKey, Crypto.localVector)));
    }

    public static string DecryptSMSPassword(string encryptedPwd)
    {
      byte[] bytes = Crypto.Transform(Convert.FromBase64String(encryptedPwd), Crypto.tdProvider.CreateDecryptor(Crypto.localKey, Crypto.localVector));
      return Crypto.stringHandler.GetString(bytes);
    }
  }