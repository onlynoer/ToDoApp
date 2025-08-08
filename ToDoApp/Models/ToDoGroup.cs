using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace ToDoApp.Models
{
    public class ToDoGroup : INotifyPropertyChanged
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string property) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));

        private string _name = "New Group";
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public ObservableCollection<ToDoItem> Tasks { get; } = new ObservableCollection<ToDoItem>();
    }

    // simple marker object to render the "Add Group" tile as the first item
    public class AddGroupPlaceHolder { }
}
