using SQLite;
using MyTeleprompter.Models;

namespace MyTeleprompter.Services
{
    public class DatabaseService
    {
        SQLiteAsyncConnection _database;

        async Task Init()
        {
            if (_database != null) return;

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "MyNotes.db");
            _database = new SQLiteAsyncConnection(dbPath);
            await _database.CreateTableAsync<Note>();
        }

        public async Task<List<Note>> GetNotesAsync()
        {
            await Init();
            return await _database.Table<Note>().ToListAsync();
        }

        public async Task<int> SaveNoteAsync(Note note)
        {
            await Init();
            if (note.Id != 0)
                return await _database.UpdateAsync(note);
            else
                return await _database.InsertAsync(note);
        }

        public async Task<int> DeleteNoteAsync(Note note)
        {
            await Init();
            return await _database.DeleteAsync(note);
        }
    }
}
