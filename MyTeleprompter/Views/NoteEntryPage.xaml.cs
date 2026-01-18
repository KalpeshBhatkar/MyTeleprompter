namespace MyTeleprompter.Views;

public partial class NoteEntryPage : ContentPage
{
	public NoteEntryPage(ViewModels.NoteEntryViewModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}