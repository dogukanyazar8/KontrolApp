using KontrolApp.Services;

namespace KontrolApp.Views;

public partial class KesimTakipLoginPage: ContentPage
{
    private SupabaseAuthService _authService = new SupabaseAuthService();

    private const string TableName = "users_otomatTakip";
    private const string UsernameColumn = "user_name";
    private const string PasswordColumn = "user_password";

    public KesimTakipLoginPage()
    {
        InitializeComponent();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            LoginErrorLabel.Text = "Kullanýcý adý ve þifre boþ olamaz.";
            LoginErrorLabel.IsVisible = true;
            return;
        }

        bool loginSuccess = await _authService.LoginAsync(TableName, UsernameColumn, PasswordColumn, username, password);

        if (loginSuccess)
        {
            await DisplayAlert("Baþarýlý", "Giriþ baþarýlý!", "Tamam");
            await Navigation.PushAsync(new KesimTakipPage());
        }
        else
        {
            LoginErrorLabel.Text = "Hatalý kullanýcý adý veya þifre.";
            LoginErrorLabel.IsVisible = true;
        }
    }
}