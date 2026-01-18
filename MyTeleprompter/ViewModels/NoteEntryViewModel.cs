using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyTeleprompter.Models;
using MyTeleprompter.Services;
using MyTeleprompter.Views;

namespace MyTeleprompter.ViewModels
{
    [QueryProperty(nameof(Note), "Note")]
    public partial class NoteEntryViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        [ObservableProperty]
        Note note;

        public NoteEntryViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
            Note = new Note { CreatedAt = DateTime.Now }; // Default empty note
        }

        [RelayCommand]
        async Task Save()
        {
            if (string.IsNullOrWhiteSpace(Note.Title))
            {
                await Shell.Current.DisplayAlert("Error", "Please enter a title", "OK");
                return;
            }

            Note.CreatedAt = DateTime.Now;
            await _databaseService.SaveNoteAsync(Note);
            await Shell.Current.DisplayAlert("Success", "Note Saved!", "OK");
        }

        [RelayCommand]
        async Task Delete()
        {
            if (Note.Id == 0) return; // Can't delete unsaved note

            bool confirm = await Shell.Current.DisplayAlert("Delete?", "Are you sure?", "Yes", "No");
            if (confirm)
            {
                await _databaseService.DeleteNoteAsync(Note);
                await Shell.Current.GoToAsync(".."); // Go back
            }
        }

        [RelayCommand]
        async Task StartTeleprompter()
        {
            // Auto-save before starting
            await Save();

            var navParam = new Dictionary<string, object>
            {
                { "Note", Note }
            };
            // Navigate to the Teleprompter Page provided in the previous answer
            await Shell.Current.GoToAsync(nameof(TeleprompterPage), navParam);
        }
    }
}
