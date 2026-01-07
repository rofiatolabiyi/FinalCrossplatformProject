using Microsoft.Maui.Storage;
using System.IO;


namespace CrossplatFinal;

public partial class SettingsPage : ContentPage
{
    public SettingsPage()
    {
        InitializeComponent();

        // load saved values (defaults)
        SoundSwitch.IsToggled = Preferences.Get("sound", true);
        MusicSwitch.IsToggled = Preferences.Get("music", true);
        DifficultyPicker.SelectedIndex = Preferences.Get("difficulty", 1);
    }

    // choosing images for char
    private async void ChangeCharacter_Clicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Pick a character image",
            FileTypes = FilePickerFileType.Images
        });

        if (result == null)
            return;

        // save image 
        string localPath = Path.Combine(FileSystem.AppDataDirectory, "player.png");

        using var sourceStream = await result.OpenReadAsync();
        using var localFileStream = File.Create(localPath);
        await sourceStream.CopyToAsync(localFileStream);

        // ave path
        Preferences.Set("player_image", localPath);
    }

    private void Save_Clicked(object sender, EventArgs e)
    {
        Preferences.Set("sound", SoundSwitch.IsToggled);
        Preferences.Set("music", MusicSwitch.IsToggled);
        Preferences.Set("difficulty", DifficultyPicker.SelectedIndex);
    }

    private async void Back_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}


