using System.Collections.ObjectModel;
using System.Text;
using System;
using Supabase;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using KontrolApp.Services;
using System.Net.Http.Headers;
using System.Text.Json;
using Supabase.Gotrue;
using System.Text.Json.Serialization;

namespace KontrolApp;

public partial class HatTakipLoginPage : ContentPage
{
    private readonly AuthenticationService _authService = new AuthenticationService();
    public HatTakipLoginPage()
	{
        InitializeComponent();
    }
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UserEntry.Text?.Trim();
        string password = PasswordEntry.Text?.Trim();
        string tableName = "users_hatTakip"; // Kullanýlacak tablo adýný belirtiyoruz

        bool isLoggedIn = await _authService.LoginAsync(tableName, username, password);

        if (isLoggedIn)
        {
            await DisplayAlert("Hoþgeldiniz", $"Merhaba {username}!", "Tamam");
        }
        else
        {
            await DisplayAlert("Hata", "Kullanýcý adý veya þifre hatalý.", "Tamam");
        }
    }
}
