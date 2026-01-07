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

        // sound volume
        double soundVol = Preferences.Get("sound_volume", 0.8);
        SoundVolumeSlider.Value = soundVol;
        SoundVolumeValueLabel.Text = $"{(int)(soundVol * 100)}%";

        // music volume
        double musicVol = Preferences.Get("music_volume", 0.5);
        MusicVolumeSlider.Value = musicVol;
        MusicVolumeValueLabel.Text = $"{(int)(musicVol * 100)}%";
    }

    private async void ChangeCharacter_Clicked(object sender, EventArgs e)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Pick a character image",
            FileTypes = FilePickerFileType.Images
        });

        if (result == null)
            return;

        string localPath = Path.Combine(FileSystem.AppDataDirectory, "player.png");

        using var sourceStream = await result.OpenReadAsync();
        using var localFileStream = File.Create(localPath);
        await sourceStream.CopyToAsync(localFileStream);

        Preferences.Set("player_image", localPath);
    }

    private void SoundVolumeSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        Preferences.Set("sound_volume", e.NewValue);
        SoundVolumeValueLabel.Text = $"{(int)(e.NewValue * 100)}%";
    }

    private void MusicVolumeSlider_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        Preferences.Set("music_volume", e.NewValue);
        MusicVolumeValueLabel.Text = $"{(int)(e.NewValue * 100)}%";
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



