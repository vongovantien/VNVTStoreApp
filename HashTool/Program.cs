using System;
using BCrypt.Net;

namespace HashTool
{
    class Program
    {
        static void Main(string[] args)
        {
            try {
                string password = "admin123";
                string hash = BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
                Console.WriteLine("HASH_Generate_Result: " + hash);
            } catch (Exception ex) {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }
}
