namespace Lexico.Views;

public partial class WelcomePageView : ContentPage
{
	public WelcomePageView()
	{
		InitializeComponent();
	}

    private async void Button_Clicked(object sender, EventArgs e)
    {
        Application.Current!.MainPage = new NavigationPage(new HomePageView());
    }
}