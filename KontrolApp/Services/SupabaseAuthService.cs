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
    public class User
    {
        [JsonPropertyName("user_name")]
        public string UserName { get; set; }

        [JsonPropertyName("user_password")]
        public string UserPassword { get; set; }
    }

    public class AuthenticationService
    {
        private const string SupabaseUrl = "https://ztfvlymtvlexfvltpewp.supabase.co";
        private const string SupabaseApiKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inp0ZnZseW10dmxleGZ2bHRwZXdwIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzkwNTY1ODUsImV4cCI6MjA1NDYzMjU4NX0.Ui8Q0QsB7tJEDZdMHYIfH5-xg-ADdQTT6Fnv0cEPBDE";

        /// <summary>
        /// Belirtilen tablo üzerinde kullanıcı doğrulaması yapar.
        /// </summary>
        /// <param name="tableName">Sorgulanacak tablo adı (örneğin "users_hatTakip")</param>
        /// <param name="username">Kullanıcı adı</param>
        /// <param name="password">Kullanıcı şifresi</param>
        /// <returns>Giriş başarılı ise true, aksi halde false döner.</returns>
        public async Task<bool> LoginAsync(string tableName, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            string encodedUsername = Uri.EscapeDataString(username);
            string encodedPassword = Uri.EscapeDataString(password);

            string requestUrl = $"{SupabaseUrl}/rest/v1/{tableName}?user_name=eq.{encodedUsername}&user_password=eq.{encodedPassword}";

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Clear();
                    client.DefaultRequestHeaders.Add("apikey", SupabaseApiKey);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", SupabaseApiKey);
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    HttpResponseMessage response = await client.GetAsync(requestUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        string jsonResponse = await response.Content.ReadAsStringAsync();

                        var users = JsonSerializer.Deserialize<List<User>>(jsonResponse, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        return (users != null && users.Count > 0);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
