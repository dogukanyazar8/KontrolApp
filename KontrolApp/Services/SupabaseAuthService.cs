using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KontrolApp.Services
{
    public class SupabaseAuthService
    {
        private readonly string _supabaseUrl = "https://ztfvlymtvlexfvltpewp.supabase.co";
        private readonly string _supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inp0ZnZseW10dmxleGZ2bHRwZXdwIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzkwNTY1ODUsImV4cCI6MjA1NDYzMjU4NX0.Ui8Q0QsB7tJEDZdMHYIfH5-xg-ADdQTT6Fnv0cEPBDE";
        private readonly HttpClient _httpClient;

        public SupabaseAuthService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _supabaseAnonKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("apikey", _supabaseAnonKey);
        }

        public async Task<bool> LoginAsync(string tableName, string usernameColumn, string passwordColumn, string username, string password)
        {
            try
            {
                string url = $"{_supabaseUrl}/rest/v1/{tableName}?{usernameColumn}=eq.{username}&select={passwordColumn}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"❌ Login hatası: {response.ReasonPhrase}");
                    return false;
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var users = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(jsonResponse);

                if (users.Count == 0)
                {
                    Console.WriteLine("❌ Kullanıcı bulunamadı.");
                    return false;
                }

                var storedPassword = users[0][passwordColumn]?.ToString();
                if (storedPassword == password)
                {
                    Console.WriteLine("Giriş başarılı!");
                    return true;
                }
                else
                {
                    Console.WriteLine("Şifre hatalı.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                return false;
            }
        }
    }
}