using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Monkies
{
    public partial class MainListViewPage : ContentPage
    {
        public MainListViewPage()
        {
            InitializeComponent();

            DummyData.Init();
            this.BindingContext = DummyData.Monkies;
        }

        public void CreateClicked(object sender, EventArgs args)
        {
            Navigation.PushAsync(new CreateMonkeyPage());
        }

        public void SearchBarTextChanged(object sender, TextChangedEventArgs args)
        {
            DummyData.Search((sender as SearchBar).Text);
        }
    }
}