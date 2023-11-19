using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VersOne.Epub;
using Image = System.Drawing.Image;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // 
        List<Book> books;
        string libraryJsonPath = "library.json";
        string settingsJsonPath = "settings.json";
        SettingsApp settingsApp;
        List<Border> borders = new List<Border>();
        int coverWidth = 180;
        int coverHeight = 270;

        // создание формы
        public MainWindow()
        {
            books = Book.loadBooks(libraryJsonPath);
            InitializeComponent();
            CreateBorderList();
            UpdateGrid();
            LoadSettings();
        }

        // загрузка настроек из settings.json
        private void LoadSettings()
        {
            try
            {
                string jsonText = File.ReadAllText(settingsJsonPath);
                settingsApp = JsonSerializer.Deserialize<SettingsApp>(jsonText);
            }
            catch
            {
                settingsApp = new SettingsApp();
            }
            finally
            {
                Width = settingsApp.MainWindowWidth;
                Height = settingsApp.MainWindowHeight;
                Left = settingsApp.MainWindowLeft;
                Top = settingsApp.MainWindowTop;
                if (settingsApp.MainWindowIsMaximized)
                {
                    WindowState = WindowState.Maximized;
                }
            }
        }

        // сохранение настроек из settings.json
        private void SaveSettings()
        {
            settingsApp.MainWindowWidth = Width;
            settingsApp.MainWindowHeight = Height;
            settingsApp.MainWindowLeft = Left;
            settingsApp.MainWindowTop = Top;
            settingsApp.MainWindowIsMaximized = WindowState == WindowState.Maximized;

            string json = JsonSerializer.Serialize(settingsApp);

            File.WriteAllText(settingsJsonPath, json);
        }

        // сохранение книг в library.json
        public void SaveBooks()
        {
            string json = JsonSerializer.Serialize(books);
            File.WriteAllText(libraryJsonPath, json);
        }            

        // создание обложек для книг
        private void CreateBorderList()
        {
            borders.Clear();
            int borderWidth = 210;
            int borderHeight = 320;
            foreach (Book book in books)
            {
                Border border = new Border();
                border.Width = borderWidth;
                border.Height = borderHeight;
                border.BorderBrush = new SolidColorBrush(Colors.Black);

                Grid grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(270) });
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(30) });
                grid.ColumnDefinitions.Add(new ColumnDefinition());

                
                System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                
                SetImageSourceAsync();
                image.Margin = new Thickness(5);
                image.Height = 270;
                image.Width = 180;
                image.HorizontalAlignment = HorizontalAlignment.Center;
                image.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetRow(image, 0);
                Grid.SetColumn(image, 0);
                

                Label label = new Label();
                if (book.Title.Length > 20)
                    label.Content = $"{book.Title.Substring(0,20)}...";
                else
                    label.Content = book.Title;       
                label.Width = 180;
                label.Height = 30;
                label.HorizontalAlignment = HorizontalAlignment.Center;
                label.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetRow(label, 1);
                Grid.SetColumn(label, 0);

                grid.Children.Add(image);
                grid.Children.Add(label);

                border.MouseEnter += border_MouseEnter;
                border.MouseLeave += border_MouseLeave;
                image.MouseEnter += borderChildren_MouseEnter;
                image.MouseLeave += borderChildren_MouseLeave;
                label.MouseEnter += borderChildren_MouseEnter;
                label.MouseLeave += borderChildren_MouseLeave;

                border.MouseDown += border_Click;

                async Task SetImageSourceAsync()
                {
                    if (book.HasCover)
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.UriSource = new Uri($"./covers/{book.Id}.png", UriKind.Relative);
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();

                        await Task.Delay(1);

                        image.Source = bitmapImage;
                    }
                    else
                    {
                        BitmapImage bitmapImage = new BitmapImage();
                        bitmapImage.BeginInit();
                        bitmapImage.UriSource = new Uri($"./covers/defualt.png", UriKind.Relative);
                        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                        bitmapImage.EndInit();

                        await Task.Delay(2);

                        image.Source = bitmapImage;
                    }
                }

                void borderChildren_MouseEnter(object sender, EventArgs e)
                {
                    border.Background = new SolidColorBrush(Colors.LightSkyBlue);
                    border.BorderThickness = new Thickness(1);
                    border.CornerRadius = new CornerRadius(10);
                }

                void borderChildren_MouseLeave(object sender, EventArgs e)
                {
                    border.Background = new SolidColorBrush(Colors.Transparent);
                    border.BorderThickness = new Thickness(0);
                }

                void border_MouseEnter(object sender, EventArgs e)
                {
                    border.Background = new SolidColorBrush(Colors.LightSkyBlue);
                    border.BorderThickness = new Thickness(1);
                    border.CornerRadius = new CornerRadius(10);
                }

                void border_MouseLeave(object sender, EventArgs e)
                {
                    border.Background = new SolidColorBrush(Colors.Transparent);
                    border.BorderThickness = new Thickness(0);
                }

                void border_Click(object sender, MouseButtonEventArgs e)
                {
                    int indexBook = books.IndexOf(book);
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        if (File.Exists(books[indexBook].FilePath))
                        {
                            SecondWindow secondWindow = new SecondWindow(books[indexBook]);                            
                            secondWindow.Show();
                            this.Close();
                        }
                        else
                        {
                            MessageBoxResult result = MessageBox.Show("Удалить книгу?", "Книга не найдена", MessageBoxButton.YesNo, MessageBoxImage.Question);
                            if (result == MessageBoxResult.Yes)
                            {
                                books.Remove(book);
                                borders.Remove(border);
                                SaveBooks();
                                UpdateGrid();
                            }
                        }
                    }
                    else if (e.RightButton == MouseButtonState.Pressed)
                    {
                        ContextMenu contextMenu = new ContextMenu();

                        MenuItem menuItem2 = new MenuItem();
                        menuItem2.Header = "Удалить";
                        menuItem2.Click += MenuItem2_Click;
                        contextMenu.Items.Add(menuItem2);

                        contextMenu.IsOpen = true;

                        e.Handled = true;
                    }
                }

                void MenuItem2_Click(object sender, RoutedEventArgs e)
                {
                    MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить?", "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        string coverPath = $"./covers/{book.Id}.png";
                        if (File.Exists(coverPath))
                            File.Delete(coverPath);
                        books.Remove(book);
                        borders.Remove(border);
                        SaveBooks();
                        UpdateGrid();
                    }
                }

                border.Child = grid;
                borders.Add(border);
            }
        }

        // обновление обложек
        private void UpdateGrid()
        {
            grid1.Children.Clear();

            double availableWidth = this.Width - 50;
            int columnWidth = 210;
            int rowHeight = 320;

            int columnCount = (int)availableWidth / columnWidth;
            int rowCount = (int)Math.Ceiling((double)borders.Count / columnCount);

            if (columnCount < 1)
                columnCount = 1;

            if (rowCount < 1)
                rowCount = 1;

            grid1.ColumnDefinitions.Clear();
            grid1.RowDefinitions.Clear();

            for (int i = 0; i < columnCount; i++)
            {
                grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(columnWidth) });
            }

            for (int i = 0; i < rowCount; i++)
            {
                grid1.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(rowHeight) });
            }

            int rowIndex = 0;
            int columnIndex = 0;
            foreach (Border border in borders)
            {
                Grid.SetColumn(border, columnIndex);
                Grid.SetRow(border, rowIndex);
                Grid.SetRowSpan(border, 1);
                Grid.SetColumnSpan(border, 1);
                grid1.Children.Add(border);

                columnIndex++;
                if (columnIndex == columnCount)
                {
                    columnIndex = 0;
                    rowIndex++;
                }
            }
        }

        // чтение последнего id из библиотеки
        private int readLastId()
        {
            int id = 0;
            if (books.Count != 0)
            {
                id = books.Last().Id;
            }
            return id;
        }

        // выбор книги из директории в формате EPUB
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "EPUB files (*.epub)|*.epub";

            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                string filePath = openFileDialog.FileName;

                foreach (Book checkBook in books)
                {
                    if (checkBook.FilePath == filePath)
                    {
                        MessageBox.Show("книга уже добавлена");
                        return;
                    }
                }

                EpubBook epubBook = EpubReader.ReadBook(filePath);

                Book book = new Book();
                book.Format = "epub";
                book.Title = epubBook.Title;
                book.FilePath = epubBook.FilePath;
                book.Id = readLastId() + 1;

                if (epubBook.CoverImage == null)
                {
                    book.HasCover = false;
                }
                else
                {
                    book.HasCover = true;
                    byte[] imageBytes = epubBook.CoverImage;

                    using (MemoryStream memoryStream = new MemoryStream(imageBytes))
                    {
                        Image image = Image.FromStream(memoryStream);
                        int newWidth = coverWidth;
                        int newHeight = coverHeight;

                        Bitmap resizedImage = new Bitmap(image, newWidth, newHeight);
                        resizedImage.Save($"./covers/{book.Id}.png", System.Drawing.Imaging.ImageFormat.Png);
                    }
                }

                books.Add(book);

                SaveBooks();
                CreateBorderList();
                UpdateGrid();
            }
        }

        // изменение размеров окна
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateGrid();
        }

        // закрытие окна
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
        }
    }
}
