using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Couchbase.Lite;
using Newtonsoft.Json;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

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
            // reset database to delete all local content to show sync is working.
            Manager.SharedInstance.GetDatabase("cb").Delete();
            Manager.SharedInstance.GetDatabase("cb").Changed += DummyData_Changed;

            var exsisting =
                Manager.SharedInstance.GetDatabase("cb")
                .CreateAllDocumentsQuery()
                .Run()
                // added check to filter out all none valid documents
                .Where(doc => doc.Document != null)
                .Where(doc => doc.Document.UserProperties != null)
                .Where(doc => doc.Document.UserProperties.ContainsKey("doc"))
                .Where(doc => doc.Document.UserProperties.ContainsKey("type"))
                .Where(doc => doc.Document.UserProperties["type"].ToString() == "monkey")
                .ToList();

            exsisting
                .ForEach(doc => Monkies.Add(JsonConvert.DeserializeObject<Monkey>(doc.Document.UserProperties["doc"].ToString())));

            // only add default data if database is empty
            if (!exsisting.Any())
            {
                var monkey = new Monkey
                {
                    Name = "Orangutan",
                    Description = "Native to Indonesia and Malaysia, orangutans are currently found in only the rainforests of Borneo and Sumatra. "
                };

                Add(monkey);
            }

            InitSync();

            InitQuery();
        }

        private static void InitQuery()
        {
            Manager.SharedInstance.GetDatabase("cb")
                .GetView("search-monkies-advanced")
                .SetMap((doc, emit) =>
                {
                    if (doc.ContainsKey("doc") && doc.ContainsKey("type") && doc["type"].ToString() == "monkey")
                    {
                        var item = doc["doc"] as JContainer;
                        foreach (var prop in item.Children().Where(p => p is JProperty).Cast<JProperty>().Select(p => new { name = p.Name, value = p.Value.ToString().ToLower() }))
                        {
                            var v = doc["type"].ToString() + "::" + prop.name;
                            emit(prop.value, v);
                        }
                    }
                },
                /* 
                replace Guid.NewGuid().ToString() with 
                static string value when not debugging to ensure 
                that the index is not rebuild on every run!
                */
                Guid.NewGuid().ToString());
        }

        public static void Search(string search)
        {
            var query = Manager.SharedInstance.GetDatabase("cb")
                .GetView("search-monkies-advanced")
                .CreateQuery();

            query.IndexUpdateMode = IndexUpdateMode.Before;
            if (!string.IsNullOrEmpty(search))
            {
                query.StartKey = search.ToLower();
                query.EndKey = query.StartKey.ToString() + '\uEFFF';
            }

            var results = query
                .Run()
                .ToList()
                .Select(doc => JsonConvert.DeserializeObject<Monkey>(doc.Document.UserProperties["doc"].ToString()))
                .GroupBy(item => item.Name)
                .Select(group => group.First());

            Monkies.Clear();

            if (results.Any())
            {
                foreach (var monkey in results)
                {
                    Monkies.Add(monkey);
                }
            }
        }

        private static void InitSync()
        {
            // this is all the code needed to initialize and continue sync'ing with SyncGateway.
            string syncGAtewayUrl = "http://try-cb.cloudapp.net:4984/sync_gateway/";
            string user = "couchbase";
            string pass = "letmein";

            // Pull any changes from Sync Gateway to the local database as changes happen
            var pull = Manager.SharedInstance.GetDatabase("cb").CreatePullReplication(new Uri(syncGAtewayUrl));
            pull.Authenticator = new Couchbase.Lite.Auth.BasicAuthenticator(user, pass);
            pull.Continuous = true;
            pull.Start();

            // Pull any local changes to SyncGateway as changes happen
            var push = Manager.SharedInstance.GetDatabase("cb").CreatePushReplication(new Uri(syncGAtewayUrl));
            push.Authenticator = new Couchbase.Lite.Auth.BasicAuthenticator(user, pass);
            push.Continuous = true;
            push.Start();
        }

        private static void DummyData_Changed(object sender, DatabaseChangeEventArgs e)
        {
            var db = Manager.SharedInstance.GetDatabase("cb");

            if (e.IsExternal)
            {
                // Debugger.Break();
                // this change came from external sync
            }

            var documents =
                e.Changes
                .ToList()
                .Select(item => item.DocumentId)
                .Select(id => db.GetDocument(id))
                // added check to filter out all none valid documents
                .Where(doc => doc.Properties != null)
                .Where(doc => doc.Properties.ContainsKey("doc"))
                .Where(doc => doc.Properties.ContainsKey("type"))
                .Where(doc => doc.Properties["type"].ToString() == "monkey");

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