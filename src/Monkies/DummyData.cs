using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Monkies
{
    public class DummyData
    {
		public static ObservableCollection<Object> Monkies = new ObservableCollection<object>();
        public DummyData()
        {
        }

        public static void Init()
        {
            var monkey = new Monkey
            {
                Name = "Orangutan",
                Description = "Native to Indonesia and Malaysia, orangutans are currently found in only the rainforests of Borneo and Sumatra. "
            };

            Add(monkey);
        }
			
        internal static void Add(Monkey monkey)
        {
            Monkies.Add(monkey);
        }
    }
}