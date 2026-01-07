namespace CrossplatFinal;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();

        // Load saved values (defaults)
        SoundSwitch.IsToggled = Preferences.Get("sound", true);
        MusicSwitch.IsToggled = Preferences.Get("music", true);
        DifficultyPicker.SelectedIndex = Preferences.Get("difficulty", 1); // 0 easy, 1 normal, 2 hard
    }

    private void Save_Clicked(object sender, EventArgs e)
    {
        Preferences.Set("sound", SoundSwitch.IsToggled);
        Preferences.Set("music", MusicSwitch.IsToggled);
        Preferences.Set("difficulty", DifficultyPicker.SelectedIndex);
    }

    private async void Back_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}

