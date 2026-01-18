namespace MyTeleprompter.Views;

public partial class NotesListPage : ContentPage
{

    private readonly ViewModels.NotesListViewModel _viewModel;

    public NotesListPage(ViewModels.NotesListViewModel vm)
    {
        InitializeComponent();
        BindingContext = _viewModel = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadNotesCommand.ExecuteAsync(null);
    }
}