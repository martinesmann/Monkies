using System;
using Xamarin.Forms;

namespace Monkies
{
	public class CreateMonkeyPage : ContentPage
	{
        Entry nameEntry;
        Entry descriptionEntry;
        Button save;

        public CreateMonkeyPage()
        {

            Title = "Create Monkey";

            nameEntry = new Entry
            {
                 Placeholder = "Name"   
            };

            descriptionEntry = new Entry
            {
                    Placeholder = "Description"
            };

            save = new Button
            {
                    Text = "Save"
            };

            save.Clicked += delegate
            {
                    var monkey = new Monkey
                    {
                        Name = nameEntry.Text,
                        Description = descriptionEntry.Text
                    };

                    DummyData.Add(monkey);
                    this.Navigation.PopAsync();
            };

            Content = new StackLayout
            {
                    Padding = 20,
                    Children = {nameEntry, descriptionEntry, save}
            };
        }
	}
}