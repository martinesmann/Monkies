using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Couchbase.Lite;
using Newtonsoft.Json;

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
            Manager.SharedInstance.GetDatabase("cb").Delete();
            Manager.SharedInstance.GetDatabase("cb").Changed += DummyData_Changed; ;
           
            var monkey = new Monkey
            {
                Name = "Orangutan",
                Description = "Native to Indonesia and Malaysia, orangutans are currently found in only the rainforests of Borneo and Sumatra. "
            };

            Add(monkey);
        }

        private static void DummyData_Changed(object sender, DatabaseChangeEventArgs e)
        {
            var db = Manager.SharedInstance.GetDatabase("cb");

            if (e.IsExternal)
            {
                // this change came from external sync
            }

            var documents =
                e.Changes
                .ToList()
                .Select(item => item.DocumentId)
                .Select(id => db.GetDocument(id));

            foreach (var document in documents)
            {
                var exsisting = Monkies.Select(item => (item as Monkey).Name == document.UserProperties["name"].ToString());

                if (exsisting != null)
                {
                    Monkies.Remove(exsisting);
                }

                var monkey = JsonConvert.DeserializeObject<Monkey>(document.UserProperties["doc"].ToString());
                Monkies.Insert(0, monkey);
            }
        }

        internal static void Add(Monkey monkey)
        {
            //Monkies.Add(monkey);

            var db = Manager.SharedInstance.GetDatabase("cb");
            var doc = db.GetDocument(monkey.Name);

            var props = doc.Properties != null ? doc.Properties : new Dictionary<string, object>();

            props["name"] = monkey.Name;
            props["type"] = "monkey";
            props["doc"] = monkey;

            doc.PutProperties(props);
        }
    }
}