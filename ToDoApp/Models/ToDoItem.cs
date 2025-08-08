using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace ToDoApp.Models
{
    public class ToDoItem : INotifyPropertyChanged
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        // parent reference used at runtime (ignored by JSON to avoid circular refs)
        [JsonIgnore]
        public ToDoGroup Parent { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string property) => PropertyChanged.Invoke(this, new PropertyChangedEventArgs(property));

        private string _text = "";
        public string Text
        {
            get => _text;
            set
            {
                if (_text == value) return;
                OnPropertyChanged(nameof(Text));
                
            }

        }

        private bool _IsDone;
        public bool IsDone
        {
            get => _IsDone;
            set
            {
                if (_IsDone == value) return;
                _IsDone = value;
                OnPropertyChanged(nameof(IsDone));
            }
        }

    }
}
