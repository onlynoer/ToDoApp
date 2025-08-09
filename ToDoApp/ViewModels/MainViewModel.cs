using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using ToDoApp.Models;

namespace ToDoApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly string _savePath;
        public ObservableCollection<object> Items { get; } = new ObservableCollection<object>();
        public RelayCommand AddGroupCommand { get; }
        public RelayCommand RemoveGroupCommand { get; }
        public RelayCommand AddTaskCommand { get; }
        public RelayCommand RemoveTaskCommand { get; }

        public MainViewModel()
        {
            //the path to the save file appdata then roaming then simpletodo
            _savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "SimpleToDo", "data.json");

            Items.Add(new AddGroupPlaceHolder());

            AddGroupCommand = new RelayCommand(_ => AddGroup());
            RemoveGroupCommand = new RelayCommand(p => RemoveGroup(p as ToDoGroup));
            AddTaskCommand = new RelayCommand(p => AddTask(p as ToDoGroup));
            RemoveTaskCommand = new RelayCommand(p => RemoveTask(p as ToDoItem));

            Load();
        }

        public void AddGroup()
        {
            var g = new ToDoGroup { Name = "New Group" };
            //inserts after placeholder and title stays first
            Items.Insert(Items.Count, g);
        }

        public void RemoveGroup(ToDoGroup group)
        {
            if (group == null) return;
            if (Items.Contains(group)) Items.Remove(group);
        }

        public void AddTask(ToDoGroup group)
        {
            if (group == null) return;
            var t = new ToDoItem { Text = "New Task", Parent = group };
            t.Parent = group;
            group.Tasks.Add(t);
        }

        public void RemoveTask(ToDoItem item)
        {
            if (item == null) return;
            item.Parent?.Tasks.Remove(item);

        }

        //helper classes to help save data
        private class SerializableGroup
        {
            public string Name { get; set; }
            public SerializableTask[] Tasks { get; set; }
            public SerializableGroup() { }
            public SerializableGroup(ToDoGroup g)
            {
                Name = g.Name;
                Tasks = g.Tasks.Select(t => new SerializableTask(t)).ToArray();
            }
        }

        private class SerializableTask
        {
            public string Text { get; set; }
            public bool IsDone { get; set; }
            public SerializableTask() { }
            public SerializableTask(ToDoItem t)
            {
                Text = t.Text;
                IsDone = t.IsDone;
            }
        }

        //events

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string p) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));

        //json save system for the data
        public void Save()
        {
            try
            {
                //SerializableGroup is a custom class
                var groups = Items.OfType<ToDoGroup>().Select(g => new SerializableGroup(g)).ToArray();
                var json = JsonSerializer.Serialize(groups, new JsonSerializerOptions { WriteIndented = true });
                var dir = Path.GetDirectoryName(_savePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(_savePath, json);
            }
            catch (Exception)
            {
                throw;
            }
        }

        //json load 
        public void Load()
        {
            try
            {
                if (!File.Exists(_savePath)) return;
                var json = File.ReadAllText(_savePath);
                var groups = JsonSerializer.Deserialize<SerializableGroup[]>(json);
                if (groups == null) return;

                foreach (var sg in groups)
                {
                    var g = new ToDoGroup { Name = sg.Name };
                    foreach (var st in sg.Tasks ?? Array.Empty<SerializableTask>())
                    {
                        var t = new ToDoItem { Text = st.Text, IsDone = st.IsDone, Parent = g };
                        g.Tasks.Add(t);
                    }
                    Items.Insert(Items.Count > 0 ? 1 : 0, g);
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
