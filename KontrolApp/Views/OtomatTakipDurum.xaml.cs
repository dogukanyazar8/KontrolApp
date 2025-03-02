using KontrolApp.Services;

namespace KontrolApp.Views;

public partial class OtomatTakipDurum : ContentPage
{
    private SupabaseService _supabaseService = new SupabaseService();
    private const string MakineTable = "makineListesi"; // Güncellenecek tablo
    private const string SiparisTable = "extreme_siparisler"; // Yeni tablo adý
    private const string NoColumn = "extreme_order_no"; // Sipariþ numarasý kolonu
    private const string ColorColumn = "renk"; // Renk kolonu
    private const string ModelColumn = "model_kodu"; // Model kolonu
    private const string QuantityColumn = "planlanan_kesim_adeti"; // Sipariþ miktarý kolonu
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
        var siparisList = await _supabaseService.GetDataAsync(SiparisTable, "id", new[] { NoColumn, ColorColumn, ModelColumn, QuantityColumn });

        if (siparisList != null && siparisList.Count > 0)
        {
            _siparisBilgiMapping.Clear();
            SiparisNoPicker.Items.Clear();

            foreach (var siparis in siparisList)
            {
                if (siparis.ContainsKey(NoColumn) && siparis.ContainsKey(ColorColumn) && siparis.ContainsKey(ModelColumn) && siparis.ContainsKey(QuantityColumn))
                {
                    string siparisNo = siparis[NoColumn]?.ToString();
                    string renk = siparis[ColorColumn]?.ToString();
                    string siparisModel = siparis[ModelColumn]?.ToString();
                    int siparisMiktar = siparis.ContainsKey(QuantityColumn) ? Convert.ToInt32(siparis[QuantityColumn]) : 0;

                    if (!string.IsNullOrEmpty(siparisNo) && !string.IsNullOrEmpty(renk))
                    {
                        string comboboxText = $"{siparisNo} - {renk}";
                        _siparisBilgiMapping[comboboxText] = (siparisModel, siparisMiktar);
                        SiparisNoPicker.Items.Add(comboboxText);
                    }
                }
            }
        }
    }

    private void OnSiparisNoSelected(object sender, EventArgs e)
    {
        if (SiparisNoPicker.SelectedItem != null)
        {
            string selectedText = SiparisNoPicker.SelectedItem.ToString();

            if (_siparisBilgiMapping.ContainsKey(selectedText))
            {
                var siparisBilgi = _siparisBilgiMapping[selectedText];

                // Sipariþ Modelini güncelle
                SiparisModelLabel.Text = $"Sipariþ Modeli: {siparisBilgi.model}";

                // Üretim Miktarý baþlýðýný güncelle
                UretimMiktariLabel.Text = $"Üretim Miktarý [ Sipariþ Miktarý: {siparisBilgi.miktar} ]";
            }
        }
        else
        {
            SiparisModelLabel.Text = "Model";
            UretimMiktariLabel.Text = "Üretim Miktarý";
        }
    }
    private async void OnGuncelleClicked(object sender, EventArgs e)
    {
        string selectedText = SiparisNoPicker.SelectedItem?.ToString();
        if (string.IsNullOrEmpty(selectedText))
        {
            await DisplayAlert("Hata", "Lütfen bir sipariþ numarasý seçin!", "Tamam");
            return;
        }

        string[] parts = selectedText.Split(" - ");
        string selectedNo = parts.Length > 0 ? parts[0] : "";
        string selectedColor = parts.Length > 1 ? parts[1] : "";
        string selectedModel = _siparisBilgiMapping.ContainsKey(selectedText) ? _siparisBilgiMapping[selectedText].model : null;
        int planlananMiktar = _siparisBilgiMapping.ContainsKey(selectedText) ? _siparisBilgiMapping[selectedText].miktar : 0;

        // Üretim miktarýný kontrol et
        if (!int.TryParse(UretimMiktariEntry.Text, out int uretimMiktari))
        {
            await DisplayAlert("Hata", "Lütfen geçerli bir üretim miktarý girin!", "Tamam");
            return;
        }

        if (uretimMiktari <= 0)
        {
            await DisplayAlert("Hata", "Üretim miktarý negatif veya sýfýr olamaz!", "Tamam");
            return;
        }

        if (uretimMiktari > planlananMiktar)
        {
            await DisplayAlert("Hata", $"Üretim miktarý, planlanan sipariþ miktarýndan fazla olamaz! (Maksimum: {planlananMiktar})", "Tamam");
            return;
        }

        // Saat kontrolleri
        TimeSpan baslangicSaati = BaslangicSaatiPicker.Time;
        TimeSpan bitisSaati = BitisSaatiPicker.Time;

        if (bitisSaati <= baslangicSaati)
        {
            await DisplayAlert("Hata", "Bitiþ saati, baþlangýç saatinden önce veya ayný olamaz!", "Tamam");
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
            await DisplayAlert("Baþarýlý", $"Otomat '{_selectedOtomatName}' durumu güncellendi!\n\nSipariþ No: {selectedNo}\nModel: {selectedModel}\nSipariþ Adet: {planlananMiktar}\nÜretim Miktarý: {uretimMiktari}\nBaþlangýç Saati: {baslangicSaati}", "Tamam");

            // ? `RefreshData()` çaðrýlýyor
            if (Application.Current.MainPage.Navigation.NavigationStack.OfType<OtomatTakipPage>().FirstOrDefault() is OtomatTakipPage takipPage)
            {
                await takipPage.RefreshData(); // ? Listeyi yenile
            }

            await Navigation.PopAsync(); // Sayfayý kapat
        }
        else
        {
            await DisplayAlert("Hata", "Güncelleme baþarýsýz!", "Tamam");
        }
    }
}
