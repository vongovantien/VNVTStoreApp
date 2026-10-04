using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace VNVTStore.Application.Services;

public interface ITotpService
{
    string GenerateSecretKey();
    string GenerateQrCodeUri(string email, string secretKey, string issuer = "VNVTStore");
    bool VerifyTotp(string secretKey, string code, int window = 1);
    List<string> GenerateRecoveryCodes(int count = 8);
}

public class TotpService : ITotpService
{
    private const string Base32Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ234567";

    public string GenerateSecretKey()
    {
        var bytes = new byte[20];
        RandomNumberGenerator.Fill(bytes);
        return ToBase32String(bytes);
    }

    public string GenerateQrCodeUri(string email, string secretKey, string issuer = "VNVTStore")
    {
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedEmail = Uri.EscapeDataString(email);
        return $"otpauth://totp/{encodedIssuer}:{encodedEmail}?secret={secretKey}&issuer={encodedIssuer}&algorithm=SHA1&digits=6&period=30";
    }

    public bool VerifyTotp(string secretKey, string code, int window = 1)
    {
        if (string.IsNullOrWhiteSpace(secretKey) || string.IsNullOrWhiteSpace(code))
            return false;

        code = code.Trim().Replace(" ", "").Replace("-", "");
        if (code.Length != 6 || !int.TryParse(code, out _))
            return false;

        byte[] keyBytes;
        try
        {
            keyBytes = FromBase32String(secretKey);
        }
        catch
        {
            return false;
        }

        var currentStep = DateTimeOffset.UtcNow.ToUnixTimeSeconds() / 30;

        for (long i = -window; i <= window; i++)
        {
            var calculatedCode = ComputeTotp(keyBytes, currentStep + i);
            if (CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(calculatedCode),
                    Encoding.UTF8.GetBytes(code)))
            {
                return true;
            }
        }

        return false;
    }

    public List<string> GenerateRecoveryCodes(int count = 8)
    {
        var codes = new List<string>(count);
        var bytes = new byte[4];
        for (int i = 0; i < count; i++)
        {
            RandomNumberGenerator.Fill(bytes);
            var hex = Convert.ToHexString(bytes);
            codes.Add($"{hex[..4]}-{hex[4..]}");
        }
        return codes;
    }

    private static string ComputeTotp(byte[] key, long step)
    {
        var counterBytes = BitConverter.GetBytes(step);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(counterBytes);
        }

        using var hmac = new HMACSHA1(key);
        var hash = hmac.ComputeHash(counterBytes);

        var offset = hash[^1] & 0x0F;
        var binary = ((hash[offset] & 0x7F) << 24)
                   | ((hash[offset + 1] & 0xFF) << 16)
                   | ((hash[offset + 2] & 0xFF) << 8)
                   | (hash[offset + 3] & 0xFF);

        var otp = binary % 1_000_000;
        return otp.ToString("D6");
    }

    private static string ToBase32String(byte[] data)
    {
        var result = new StringBuilder((data.Length * 8 + 4) / 5);
        int buffer = 0, bitsLeft = 0;

        foreach (var b in data)
        {
            buffer = (buffer << 8) | b;
            bitsLeft += 8;
            while (bitsLeft >= 5)
            {
                bitsLeft -= 5;
                result.Append(Base32Alphabet[(buffer >> bitsLeft) & 0x1F]);
            }
        }

        if (bitsLeft > 0)
        {
            result.Append(Base32Alphabet[(buffer << (5 - bitsLeft)) & 0x1F]);
        }

        return result.ToString();
    }

    private static byte[] FromBase32String(string base32)
    {
        base32 = base32.Trim().TrimEnd('=').ToUpperInvariant();
        var output = new List<byte>((base32.Length * 5) / 8);
        int buffer = 0, bitsLeft = 0;

        foreach (var c in base32)
        {
            var val = Base32Alphabet.IndexOf(c);
            if (val < 0) throw new FormatException($"Illegal character '{c}' in Base32 string");

            buffer = (buffer << 5) | val;
            bitsLeft += 5;

            if (bitsLeft >= 8)
            {
                bitsLeft -= 8;
                output.Add((byte)((buffer >> bitsLeft) & 0xFF));
            }
        }

        return output.ToArray();
    }
}
