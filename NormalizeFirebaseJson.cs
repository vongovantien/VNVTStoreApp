using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;

public class Program
{
    public static void Main()
    {
        try
        {
            var path = @"d:\VNVTStoreApp\VNVTStore.Backend\src\VNVTStore.API\firebase-service-account.json";
            var json = File.ReadAllText(path);
            var options = new JsonSerializerOptions { WriteIndented = true };
            var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(json);
            
            if (dict.ContainsKey("private_key"))
            {
                var key = dict["private_key"].ToString();
                // Clean the key: remove headers, newlines, spaces
                var cleaned = key.Replace("-----BEGIN PRIVATE KEY-----", "")
                                 .Replace("-----END PRIVATE KEY-----", "")
                                 .Replace("\\n", "")
                                 .Replace("\n", "")
                                 .Replace("\r", "")
                                 .Replace(" ", "")
                                 .Trim();
                
                // Re-format as standard PEM
                var formattedKey = "-----BEGIN PRIVATE KEY-----\n";
                for (int i = 0; i < cleaned.Length; i += 64)
                {
                    int len = Math.Min(64, cleaned.Length - i);
                    formattedKey += cleaned.Substring(i, len) + "\n";
                }
                formattedKey += "-----END PRIVATE KEY-----\n";
                
                dict["private_key"] = formattedKey;
                
                var newJson = JsonSerializer.Serialize(dict, options);
                File.WriteAllText(path, newJson);
                Console.WriteLine("Normalization SUCCESS!");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
