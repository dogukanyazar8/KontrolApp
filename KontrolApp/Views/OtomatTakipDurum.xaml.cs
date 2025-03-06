using KontrolApp.Services;

namespace KontrolApp.Views;

public partial class OtomatTakipDurum : ContentPage
{
    private SupabaseService _supabaseService = new SupabaseService();
    private const string MakineTable = "makineListesi"; // Güncellenecek tablo
    private const string SiparisTable = "extreme_siparisler"; // Yeni tablo adı
    private const string NoColumn = "extreme_order_no"; // Sipariş numarası kolonu
    private const string ColorColumn = "renk"; // Renk kolonu
    private const string ModelColumn = "model_kodu"; // Model kolonu
    private const string QuantityColumn = "planlanan_kesim_adeti"; // Sipariş miktarı kolonu
    private const string StatusColumn = "otomat_durumu";

    private string _selectedOtomatName;
    private int _selectedOtomatId;
    private DateTime _selectedDateTime;
    private Dictionary<string, (string model, int miktar)> _siparisBilgiMapping = new Dictionary<string, (string, int)>();

    public OtomatTakipDurum(int otomatId, string otomatName)
    {
        InitializeComponent();

        _selectedOtomatId = otomatId;
        _selectedOtomatName = otomatName;
        _selectedDateTime = DateTime.Now;

        // Seçilen otomatın bilgisini göster
        OtomatBilgiLabel.Text = $"Makine Adı: {_selectedOtomatName}";

        // Başlangıç tarihi ve saati ayarla
        BaslangicTarihiLabel.Text = _selectedDateTime.ToString("dd MMMM yyyy"); // Değiştirilemez
        BaslangicSaatiPicker.Time = _selectedDateTime.TimeOfDay; // Kullanıcı değiştirebilir

        // Varsayılan olarak Sipariş Modeli etiketi "Model" olacak
        SiparisModelLabel.Text = "Model";

        // Sipariş numaralarını yükle
        LoadSiparisNo();
    }

    private async void LoadSiparisNo()
    {
        var otomatData = await _supabaseService.GetDataAsync(MakineTable, "id",
            new[] { "id", StatusColumn, "siparis_no", "renk", "model_adi", "siparis_adet" });

        if (otomatData == null || otomatData.Count == 0)
            return;

        var otomat = otomatData.FirstOrDefault(o =>
            o.ContainsKey("id") && o["id"] != null &&
            o["id"].ToString().Trim() == _selectedOtomatId.ToString());

        if (otomat == null)
            return;

        string otomatDurumu = otomat.ContainsKey(StatusColumn) && otomat[StatusColumn] != null
            ? otomat[StatusColumn].ToString().Trim()
            : "";

        if (string.Equals(otomatDurumu, "Devam Ediyor", StringComparison.OrdinalIgnoreCase))
        {
            string siparisNo = otomat.ContainsKey("siparis_no") && otomat["siparis_no"] != null ? otomat["siparis_no"].ToString().Trim() : "";
            string renk = otomat.ContainsKey("renk") && otomat["renk"] != null ? otomat["renk"].ToString().Trim() : "";
            string modelAdi = otomat.ContainsKey("model_adi") && otomat["model_adi"] != null ? otomat["model_adi"].ToString().Trim() : "";
            int siparisMiktar = otomat.ContainsKey("siparis_adet") && otomat["siparis_adet"] != null ? Convert.ToInt32(otomat["siparis_adet"]) : 0;

            if (!string.IsNullOrEmpty(siparisNo) && !string.IsNullOrEmpty(renk))
            {
                string comboboxText = $"{siparisNo} - {renk}";
                SiparisNoPicker.Items.Clear();
                SiparisNoPicker.Items.Add(comboboxText);
                SiparisNoPicker.SelectedIndex = 0;
                SiparisNoPicker.IsEnabled = false;
            }

            SiparisModelLabel.Text = $"Sipariş Modeli: {modelAdi}";
            UretimMiktariLabel.Text = $"Üretim Miktarı [ Sipariş Miktarı: {siparisMiktar} ]";
            return;
        }

        var siparisList = await _supabaseService.GetDataAsync(SiparisTable, "id",
            new[] { NoColumn, ColorColumn, ModelColumn, QuantityColumn });

        if (siparisList == null || siparisList.Count == 0)
            return;

        _siparisBilgiMapping.Clear();
        SiparisNoPicker.Items.Clear();

        foreach (var siparis in siparisList)
        {
            if (siparis.ContainsKey(NoColumn) && siparis[NoColumn] != null &&
                siparis.ContainsKey(ColorColumn) && siparis[ColorColumn] != null &&
                siparis.ContainsKey(ModelColumn) && siparis[ModelColumn] != null &&
                siparis.ContainsKey(QuantityColumn) && siparis[QuantityColumn] != null)
            {
                string siparisNo = siparis[NoColumn]?.ToString().Trim();
                string renk = siparis[ColorColumn]?.ToString().Trim();
                string siparisModel = siparis[ModelColumn]?.ToString().Trim();
                int siparisMiktar = Convert.ToInt32(siparis[QuantityColumn]);

                if (!string.IsNullOrEmpty(siparisNo) && !string.IsNullOrEmpty(renk))
                {
                    string comboboxText = $"{siparisNo} - {renk}";
                    _siparisBilgiMapping[comboboxText] = (siparisModel, siparisMiktar);
                    SiparisNoPicker.Items.Add(comboboxText);
                }
            }
        }

        SiparisNoPicker.IsEnabled = true;
        SiparisNoPicker.SelectedIndexChanged += (sender, e) =>
        {
            if (SiparisNoPicker.SelectedItem != null)
            {
                string selectedText = SiparisNoPicker.SelectedItem.ToString();
                if (_siparisBilgiMapping.ContainsKey(selectedText))
                {
                    var siparisBilgi = _siparisBilgiMapping[selectedText];
                    SiparisModelLabel.Text = $"Sipariş Modeli: {siparisBilgi.model}";
                    UretimMiktariLabel.Text = $"Üretim Miktarı [ Sipariş Miktarı: {siparisBilgi.miktar} ]";
                }
            }
            else
            {
                SiparisModelLabel.Text = "Model";
                UretimMiktariLabel.Text = "Üretim Miktarı";
            }
        };
    }
    private async void OnGuncelleClicked(object sender, EventArgs e)
    {
        string selectedText = SiparisNoPicker.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(selectedText))
        {
            await DisplayAlert("Hata", "Lütfen bir sipariş numarası seçin!", "Tamam");
            return;
        }

        string[] parts = selectedText.Split(" - ");
        string selectedNo = parts.Length > 0 ? parts[0] : "";
        string selectedColor = parts.Length > 1 ? parts[1] : "";
        string selectedModel = _siparisBilgiMapping.ContainsKey(selectedText) ? _siparisBilgiMapping[selectedText].model : null;
        int planlananMiktar = _siparisBilgiMapping.ContainsKey(selectedText) ? _siparisBilgiMapping[selectedText].miktar : 0;

        // Üretim miktarını kontrol et
        if (!int.TryParse(UretimMiktariEntry.Text, out int uretimMiktari))
        {
            await DisplayAlert("Hata", "Lütfen geçerli bir üretim miktarı girin!", "Tamam");
            return;
        }

        if (uretimMiktari <= 0)
        {
            await DisplayAlert("Hata", "Üretim miktarı negatif veya sıfır olamaz!", "Tamam");
            return;
        }

        if (uretimMiktari > planlananMiktar)
        {
            await DisplayAlert("Hata", $"Üretim miktarı, planlanan sipariş miktarından fazla olamaz! (Maksimum: {planlananMiktar})", "Tamam");
            return;
        }

        // Saat kontrolleri
        TimeSpan baslangicSaati = BaslangicSaatiPicker.Time;
        TimeSpan bitisSaati = BitisSaatiPicker.Time;

        if (bitisSaati <= baslangicSaati)
        {
            await DisplayAlert("Hata", "Bitiş saati, başlangıç saatinden önce veya aynı olamaz!", "Tamam");
            return;
        }

        // ? `makineListesi` tablosunu güncelle
        var updateData = new Dictionary<string, object>
    {
        { "otomat_durumu", "Devam Ediyor" },
        { "siparis_no", selectedNo },
        { "model_adi", selectedModel },
        { "siparis_adet", planlananMiktar },
        { "siparis_tamamlanan", uretimMiktari },
        { "renk", selectedColor }
    };

        bool updateSuccess = await _supabaseService.UpdateDataAsync(MakineTable, "id", _selectedOtomatId, updateData);

        if (updateSuccess)
        {
            await DisplayAlert("Başarılı", $"Otomat '{_selectedOtomatName}' durumu güncellendi!\n\nSipariş No: {selectedNo}\nModel: {selectedModel}\nSipariş Adet: {planlananMiktar}\nÜretim Miktarı: {uretimMiktari}\nBaşlangıç Saati: {baslangicSaati}", "Tamam");

            // ? `RefreshData()` çağrılıyor
            if (Application.Current.MainPage.Navigation.NavigationStack.OfType<OtomatTakipPage>().FirstOrDefault() is OtomatTakipPage takipPage)
            {
                await takipPage.RefreshData(); // ? Listeyi yenile
            }

            await Navigation.PopAsync(); // Sayfayı kapat
        }
        else
        {
            await DisplayAlert("Hata", "Güncelleme başarısız!", "Tamam");
        }
    }
}
