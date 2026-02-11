using System;
using System.IO;
using System.Text.Json;

public class Program
{
    public static void Main()
    {
        try
        {
            var json = File.ReadAllText("VNVTStore.Backend/src/VNVTStore.API/firebase-service-account.json");
            using var doc = JsonDocument.Parse(json);
            var key = doc.RootElement.GetProperty("private_key").GetString();
            Console.WriteLine("Key length: " + key.Length);
            
            // Try to extract only the base64 part
            var base64 = key.Replace("-----BEGIN PRIVATE KEY-----", "")
                            .Replace("-----END PRIVATE KEY-----", "")
                            .Replace("\n", "")
                            .Replace("\r", "")
                            .Trim();
            
            Console.WriteLine("Base64 length: " + base64.Length);
            
            try
            {
                var bytes = Convert.FromBase64String(base64);
                Console.WriteLine("Decoding SUCCESS!");
            }
            catch (FormatException ex)
            {
                Console.WriteLine("Decoding FAILED: " + ex.Message);
                // Find first illegal char
                for (int i = 0; i < base64.Length; i++)
                {
                    char c = base64[i];
                    bool isValid = (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '+' || c == '/' || c == '=';
                    if (!isValid)
                    {
                        Console.WriteLine($"Illegal char at index {i}: '{c}' (code: {(int)c})");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
