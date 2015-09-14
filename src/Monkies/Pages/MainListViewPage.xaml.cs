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
    }
}