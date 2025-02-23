using KontrolApp.Services;
namespace KontrolApp.Views;

public partial class KesimTakipDurum : ContentPage
{
    private SupabaseService _supabaseService = new SupabaseService();
    private const string MakineTable = "makineListesi"; // Güncellenecek tablo
    private const string SiparisTable = "siparisler";
    private const string NoColumn = "siparis_no";
    private const string ModelColumn = "siparis_model";
    private const string StatusColumn = "otomat_durumu";

    private string _selectedOtomatName;
    private int _selectedOtomatId;
    private DateTime _selectedDateTime;
    private Dictionary<string, string> _siparisModelMapping = new Dictionary<string, string>();

    public KesimTakipDurum(int otomatId, string otomatName)
    {
        InitializeComponent();

        _selectedOtomatId = otomatId;
        _selectedOtomatName = otomatName;
        _selectedDateTime = DateTime.Now;

        // Seçilen otomatýn bilgisini göster
        OtomatBilgiLabel.Text = $"Makine Adý: {_selectedOtomatName}";

        // Baþlangýç tarihi ve saati ayarla
        BaslangicTarihiLabel.Text = _selectedDateTime.ToString("dd MMMM yyyy"); // Deðiþtirilemez
        BaslangicSaatiPicker.Time = _selectedDateTime.TimeOfDay; // Kullanýcý deðiþtirebilir

        // Varsayýlan olarak Sipariþ Modeli etiketi "Model" olacak
        SiparisModelLabel.Text = "Model";

        // Sipariþ numaralarýný yükle
        LoadSiparisNo();
    }

    private async void LoadSiparisNo()
    {
        var siparisList = await _supabaseService.GetDataAsync(SiparisTable, "id", new[] { NoColumn, ModelColumn });

        if (siparisList != null && siparisList.Count > 0)
        {
            _siparisModelMapping.Clear();
            SiparisNoPicker.Items.Clear();

            foreach (var siparis in siparisList)
            {
                if (siparis.ContainsKey(NoColumn) && siparis.ContainsKey(ModelColumn))
                {
                    string siparisNo = siparis[NoColumn]?.ToString();
                    string siparisModel = siparis[ModelColumn]?.ToString();

                    if (!string.IsNullOrEmpty(siparisNo))
                    {
                        _siparisModelMapping[siparisNo] = siparisModel;
                        SiparisNoPicker.Items.Add(siparisNo);
                    }
                }
            }
        }
    }

    private void OnSiparisNoSelected(object sender, EventArgs e)
    {
        if (SiparisNoPicker.SelectedItem != null)
        {
            string selectedNo = SiparisNoPicker.SelectedItem.ToString();

            if (_siparisModelMapping.ContainsKey(selectedNo))
            {
                SiparisModelLabel.Text = $"Sipariþ Modeli: {_siparisModelMapping[selectedNo]}";
            }
            else
            {
                SiparisModelLabel.Text = "Model";
            }
        }

    }
}