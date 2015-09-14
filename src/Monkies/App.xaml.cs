using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Monkies
{
	public partial class App : Application
	{
		public App ()
		{
			InitializeComponent ();

			MainPage = new NavigationPage(new MainListViewPage());
		}
	}
}

