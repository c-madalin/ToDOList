using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows.Input;
using ToDo.Models;
using ToDo.utils;

namespace ToDo.ViewModels
{
    class ToDoViewModel : INotifyPropertyChanged
    {
        private string FilePath = "res\\todo.json";

        public ObservableCollection<ToDoItem> ToDoItems { get; set; } = new();

        private string _newItemTitle;
        public string NewItemTitle
        {
            get => _newItemTitle;
            set
            {
                _newItemTitle = value;
                OnPropertyChanged(nameof(NewItemTitle));
            }
        }

        public ICommand AddCommand { get; set; }
        public ICommand LoadCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        public ToDoViewModel()
        {
            AddCommand = new RelayCommand(_ => AddNewItem(), _ => !string.IsNullOrWhiteSpace(NewItemTitle));
            LoadCommand = new RelayCommand(_ => LoadItems());
            SaveCommand = new RelayCommand(_ => SaveItems());
        }

        private void AddNewItem()
        {
            ToDoItems.Add(new ToDoItem
            {
                Title = NewItemTitle,
                Description = "Description", // Poți lega și un câmp Description din UI, dacă vrei
                IsDone = false
            });
            NewItemTitle = string.Empty;
            SaveToFile(); // Salvează automat după adăugare
        }

        private void SaveItems() => SaveToFile();
        private void LoadItems() => LoadFromFile();

        private void SaveToFile()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(FilePath)); // creează folderul dacă nu există
                var json = JsonSerializer.Serialize(ToDoItems, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch (Exception ex)
            {
                // Poți loga sau arăta un mesaj de eroare
                Console.WriteLine("Eroare la salvare: " + ex.Message);
            }
        }

        private void LoadFromFile()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    var json = File.ReadAllText(FilePath);
                    var items = JsonSerializer.Deserialize<ObservableCollection<ToDoItem>>(json);
                    ToDoItems.Clear();
                    foreach (var item in items)
                        ToDoItems.Add(item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Eroare la încărcare: " + ex.Message);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
