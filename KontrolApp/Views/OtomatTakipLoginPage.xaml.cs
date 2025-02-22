using KontrolApp.Services;

namespace KontrolApp.Views;

public partial class OtomatTakipLoginPage : ContentPage
{
    private readonly AuthenticationService _authService = new AuthenticationService();
    public OtomatTakipLoginPage()
	{
		InitializeComponent();
	}
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UserEntry.Text?.Trim();
        string password = PasswordEntry.Text?.Trim();
        string tableName = "users_otomatTakip"; // Kullanýlacak tablo adýný belirtiyoruz

        bool isLoggedIn = await _authService.LoginAsync(tableName, username, password);

        if (isLoggedIn)
        {
            await DisplayAlert("Hoþgeldiniz", $"Merhaba {username}!", "Tamam");
            await Navigation.PushAsync(new OtomatTakipPage());
        }
        else
        {
            await DisplayAlert("Hata", "Kullanýcý adý veya þifre hatalý.", "Tamam");
        }
    }
}