
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyTeleprompter.Models;
using MyTeleprompter.Services;
using MyTeleprompter.Views;
using System.Collections.ObjectModel;

namespace MyTeleprompter.ViewModels
{
    public partial class NotesListViewModel : ObservableObject
    {
        private readonly DatabaseService _databaseService;

        public ObservableCollection<Note> Notes { get; } = new();

        public NotesListViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        [RelayCommand]
        public async Task LoadNotes()
        {
            var notes = await _databaseService.GetNotesAsync();
            Notes.Clear();
            foreach (var note in notes)
            {
                Notes.Add(note);
            }
        }

        [RelayCommand]
        async Task AddNote()
        {
            // Navigate to empty NoteEntryPage
            await Shell.Current.GoToAsync(nameof(NoteEntryPage));
        }

        [RelayCommand]
        async Task EditNote(Note note)
        {
            // Navigate to NoteEntryPage passing the selected note
            var navParam = new Dictionary<string, object>
            {
                { "Note", note }
            };
            await Shell.Current.GoToAsync(nameof(NoteEntryPage), navParam);
        }
    }
}
