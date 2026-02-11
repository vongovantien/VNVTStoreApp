using System;
using System.IO;
using System.Text.Json;
using System.Linq;
using System.Security.Cryptography;

namespace TestFirebaseKeyProj;

public class Program
{
    public static void Main()
    {
        try
        {
            var json = File.ReadAllText(@"d:\VNVTStoreApp\older_firebase.json");
            using var doc = JsonDocument.Parse(json);
            var key = doc.RootElement.GetProperty("private_key").GetString();
            
            var base64 = key.Replace("-----BEGIN PRIVATE KEY-----", "")
                            .Replace("-----END PRIVATE KEY-----", "")
                            .Replace("\n", "")
                            .Replace("\r", "")
                            .Trim();
            
            // Base64 length 1623. MQ= ends at 1623.
            // MQ= decoded to 2 bytes?
            // Let's decode 1620 chars (405 blocks) first.
            var basePart = base64.Substring(0, 1620);
            var lastPart = base64.Substring(1620); // Length 3: "MQ="
            
            var bytes1620 = Convert.FromBase64String(basePart); // 1215 bytes
            
            // Try all 256 values for the 1216th and 1217th bytes?
            // Wait, MQ= is 2 bytes. M (6), Q (6), = (pad). 12 bits total? No.
            // 4 chars -> 3 bytes.
            // ...MQ= -> Invalid Base64 if it's 3 chars + padding.
            
            // Let's just decode the "known good" part and then try to append bytes.
            // If base64 length 1624 (with padding) gives 1217 bytes.
            // Let's try to find a base64 string that decodes to 1217 bytes and matches the header.
            
            byte[] prefix = Convert.FromBase64String(base64.Substring(0, 1620)); // 1215 bytes
            // We need 1217 bytes total? No, 1213 (body) + 4 (header) = 1217.
            // So we need 2 more bytes after 1215.
            
            Console.WriteLine("Brute-forcing 2 missing bytes...");
            int found = 0;
            for (int b1 = 0; b1 < 256; b1++) {
                for (int b2 = 0; b2 < 256; b2++) {
                    byte[] trial = new byte[1217];
                    Array.Copy(prefix, trial, 1215);
                    trial[1215] = (byte)b1;
                    trial[1216] = (byte)b2;
                    
                    try {
                        using var rsa = RSA.Create();
                        rsa.ImportPkcs8PrivateKey(trial, out _);
                        Console.WriteLine($"VALID KEY FOUND! Bytes: {b1}, {b2}");
                        
                        // Save the working key
                        var b64 = Convert.ToBase64String(trial);
                        var formattedKey = "-----BEGIN PRIVATE KEY-----\n";
                        for (int i = 0; i < b64.Length; i += 64)
                        {
                            int len = Math.Min(64, b64.Length - i);
                            formattedKey += b64.Substring(i, len) + "\n";
                        }
                        formattedKey += "-----END PRIVATE KEY-----\n";
                        File.WriteAllText("recovered_key.txt", formattedKey);
                        found++;
                    } catch { }
                }
            }
            Console.WriteLine($"Search finished. Found {found} valid keys.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
}
