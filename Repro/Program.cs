using System;
using System.IO;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin;

class Program
{
    static void Main(string[] args)
    {
        string path = @"d:\VNVTStoreApp\VNVTStore.Backend\src\VNVTStore.API\firebase-service-account.json";
        Console.WriteLine($"Reading from: {path}");

        if (!File.Exists(path))
        {
            Console.WriteLine("File not found!");
            return;
        }

        try 
        {
            string json = File.ReadAllText(path);
            Console.WriteLine($"File length: {json.Length}");
            
            // Try 1: Standard FromJson
            Console.WriteLine("Attempting GoogleCredential.FromJson...");
            try 
            {
                var cred = GoogleCredential.FromJson(json);
                Console.WriteLine("SUCCESS: GoogleCredential.FromJson worked!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FAILED: {ex.Message}");
                if (ex.InnerException != null) Console.WriteLine($"Inner: {ex.InnerException.Message}");
            }

            // manual checks
            var doc = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, string>>(json);
            if (doc.ContainsKey("private_key"))
            {
                string key = doc["private_key"];
                Console.WriteLine($"Key length: {key.Length}");
                Console.WriteLine($"Starts with header: {key.Contains("-----BEGIN PRIVATE KEY-----")}");
                Console.WriteLine($"Ends with footer: {key.Contains("-----END PRIVATE KEY-----")}");
                
                // Check for escapings
                Console.WriteLine($"Contains literal \\n: {key.Contains("\\n")}");
                Console.WriteLine($"Contains real newline: {key.Contains("\n")}");

                // Print first 50 chars of body
                string body = key.Replace("-----BEGIN PRIVATE KEY-----", "").Replace("-----END PRIVATE KEY-----", "").Trim();
                Console.WriteLine($"First 50 chars of body: {body.Substring(0, Math.Min(50, body.Length))}");
                
                // Check Base64 validity
                string cleanBody = body.Replace("\n", "").Replace("\r", "").Replace(" ", "");
                try {
                    byte[] data = Convert.FromBase64String(cleanBody);
                    Console.WriteLine($"Base64 Decode SUCCESS. Bytes: {data.Length}");
                } catch (Exception ex) {
                    Console.WriteLine($"Base64 Decode FAILED: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Global Error: {ex}");
        }
    }
}
