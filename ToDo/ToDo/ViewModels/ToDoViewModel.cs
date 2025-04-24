using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using ToDo.Models;
using ToDo.Utils;

namespace ToDo.ViewModels
{
    public class ToDoViewModel : INotifyPropertyChanged
    {
        private readonly string _filePath = "res\\todo.json";
        public ObservableCollection<ToDoItem> ToDoItems { get; set; } = new();

        private string _newItemTitle;
        public string NewItemTitle
        {
            get => _newItemTitle;
            set
            {
                _newItemTitle = value;
                OnPropertyChanged(nameof(NewItemTitle));
                CommandManager.InvalidateRequerySuggested(); // Refresh command states
            }
        }

        private string _newItemDescription;
        public string NewItemDescription
        {
            get => _newItemDescription;
            set
            {
                _newItemDescription = value;
                OnPropertyChanged(nameof(NewItemDescription));
            }
        }

        public ICommand AddCommand { get; }
        public ICommand RemoveCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand ClearAllCommand { get; }

        public ToDoViewModel()
        {
            AddCommand = new RelayCommand(_ => AddNewItem(), _ => !string.IsNullOrWhiteSpace(NewItemTitle));
            RemoveCommand = new RelayCommand(param => RemoveItem(param as ToDoItem));
            LoadCommand = new RelayCommand(_ => LoadItems());
            SaveCommand = new RelayCommand(_ => SaveItems());
            ClearAllCommand = new RelayCommand(_ => ClearAllItems(), _ => ToDoItems.Count > 0);

            // Try to load items when application starts
            LoadItems();
        }

        private void AddNewItem()
        {
            ToDoItems.Add(new ToDoItem
            {
                Title = NewItemTitle,
                Description = NewItemDescription ?? string.Empty,
                IsDone = false,
                CreatedAt = DateTime.Now
            });

            NewItemTitle = string.Empty;
            NewItemDescription = string.Empty;
            
            SaveToFile(); // Auto-save
        }

        private void RemoveItem(ToDoItem item)
        {
            if (item != null)
            {
                ToDoItems.Remove(item);
                SaveToFile(); // Auto-save
            }
        }

        private void ClearAllItems()
        {
            // Ask for confirmation
            if (MessageBox.Show("Sigur doriți să ștergeți toate task-urile?", 
                                "Confirmare", 
                                MessageBoxButton.YesNo, 
                                MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                ToDoItems.Clear();
                SaveToFile(); // Auto-save
            }
        }

        private void SaveItems() => SaveToFile();

        private void LoadItems() => LoadFromFile();

        private void SaveToFile()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_filePath)); // Create directory if it doesn't exist
                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };
                
                var json = JsonSerializer.Serialize(ToDoItems, options);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la salvare: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadFromFile()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    var options = new JsonSerializerOptions 
                    { 
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };

                    var items = JsonSerializer.Deserialize<ObservableCollection<ToDoItem>>(json, options);
                    
                    if (items != null)
                    {
                        ToDoItems.Clear();
                        foreach (var item in items)
                            ToDoItems.Add(item);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Eroare la încărcare: {ex.Message}", "Eroare", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
