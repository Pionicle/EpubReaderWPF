using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using VersOne.Epub;
using System.IO;
using System.Text.Json;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для SecondWindow.xaml
    /// </summary>
    public partial class SecondWindow : Window
    {
        //
        List<Button> buttons = new List<Button>();
        string settingsJsonPath = "settings.json";
        EpubBook book;
        Book currentBook = new Book();
        SettingsApp settingsApp;
        Button selectedButton;

        // создание окна
        public SecondWindow(Book book1)
        {
            book = EpubReader.ReadBook(book1.FilePath);
            Book.MoveInfoBook(currentBook, book1);
            InitializeComponent();
            PrintTableOfContents();
            grid1.SizeChanged += MainWindow_SizeChanged;
            SetStylePage();
            Title = book.Title;
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
                Width = settingsApp.SecondWindowWidth;
                Height = settingsApp.SecondWindowHeight;
                Left = settingsApp.SecondWindowLeft;
                Top = settingsApp.SecondWindowTop;
                if (settingsApp.SecondWindowIsMaximized)
                {
                    WindowState = WindowState.Maximized;
                }
            }
        }

        // сохранение настроек из settings.json
        private void SaveSettings()
        {
            settingsApp.SecondWindowWidth = Width;
            settingsApp.SecondWindowHeight = Height;
            settingsApp.SecondWindowLeft = Left;
            settingsApp.SecondWindowTop = Top;
            settingsApp.SecondWindowIsMaximized = WindowState == WindowState.Maximized;

            string json = JsonSerializer.Serialize(settingsApp);

            File.WriteAllText(settingsJsonPath, json);
        }

        // установка стилей для richTextbox (страница текста)
        public void SetStylePage()
        {            
            try
            {
                string jsonText = File.ReadAllText("settings.json");
                settingsApp = JsonSerializer.Deserialize<SettingsApp>(jsonText);
            }
            catch
            {
                settingsApp = new SettingsApp();
            }
            finally
            {
                Color backgroundColor = (Color)ColorConverter.ConvertFromString(settingsApp.Background);
                richtextbox1.Background = new SolidColorBrush(backgroundColor);

                Color foregroundColor = (Color)ColorConverter.ConvertFromString(settingsApp.Foreground);
                richtextbox1.Foreground = new SolidColorBrush(foregroundColor);

                richtextbox1.FontSize = settingsApp.FontSize;
            }
        }

        // сохранение стилей для richTextbox (страница текста)
        public void SaveStylePage()
        {
            string json = JsonSerializer.Serialize(settingsApp);

            File.WriteAllText("settings.json", json);
        }

        // загрузка последней главы и места где читали
        private void SelectedPage()
        {   
            if (currentBook.LastChapter != "")
            {
                richtextbox1.Document.Blocks.Clear();
                string hiddenValue = currentBook.LastChapter;
                Paragraph paragraph = new Paragraph(new Run(PrintChapter(hiddenValue)));
                richtextbox1.Document.Blocks.Add(paragraph);
                foreach (Button button in buttons)
                {
                    if (hiddenValue == button.Tag.ToString())
                    {
                        selectedButton = button;
                        button.Background = new SolidColorBrush(Colors.LightSkyBlue);
                        break;
                    }
                }
                return;
            }
            else
            {
                try
                {
                    Button firstButton = treeview1.Items[0] as Button;

                    if (firstButton != null)
                    {
                        firstButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                    }
                }
                catch
                {
                    MessageBox.Show("У книги отсутствует оглавление");
                }
            }            
        }

        // ограничение минимального размера окна
        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double newHeight = e.NewSize.Height;
            richtextbox1.Height = newHeight;
            richtextbox1.Width = newHeight / 1.38;
            MinWidth = richtextbox1.Width + treeview1.Width + 20;
        }

        // загрузка всех глав книг
        private void PrintTableOfContents()
        {
            foreach (EpubNavigationItem navigationItem in book.Navigation)
            {                                                
                PrintNavigationItem(navigationItem);
            }
            SelectedPage();
        }

        // загрузка оглавления в виде кнопок 
        private void PrintNavigationItem(EpubNavigationItem navigationItem)
        {
            void Button_Click(object sender, RoutedEventArgs e)
            {
                
                Button btn = (Button)sender;
                string hiddenValue = btn.Tag as string;
                if (sender is Button clickedButton)
                {                    
                    if (selectedButton != null && selectedButton == clickedButton)
                    {
                        return;
                    }
                    else
                    {
                        richtextbox1.ScrollToHome();
                        if (selectedButton != null)
                        {
                            selectedButton.Background = new SolidColorBrush(Colors.LightGray);
                        }

                        clickedButton.Background = new SolidColorBrush(Colors.LightSkyBlue);

                        selectedButton = clickedButton;
                    }
                }

                richtextbox1.Document.Blocks.Clear();

                // Создаем новый параграф и добавляем его в FlowDocument
                Paragraph paragraph = new Paragraph(new Run(PrintChapter(hiddenValue)));
                richtextbox1.Document.Blocks.Add(paragraph);

                // сохранение текущей главы
                currentBook.LastChapter = hiddenValue;

                string jsonString = File.ReadAllText("Library.json");

                List<Book> objectsList = JsonSerializer.Deserialize<List<Book>>(jsonString);

                Book targetObject = objectsList.Find(obj => obj.Id == currentBook.Id);
                if (targetObject != null)
                {
                    targetObject.LastChapter = currentBook.LastChapter;
                }

                string updatedJson = JsonSerializer.Serialize(objectsList);

                File.WriteAllText("Library.json", updatedJson);
            }

            Button button = new Button();
            button.Content = navigationItem.Title;
            button.Tag = navigationItem.Link.ContentFilePath;
            button.HorizontalContentAlignment = HorizontalAlignment.Left;
            button.ToolTip = navigationItem.Title;
            button.Width = 150;
            button.Height = 30;
            button.Margin = new Thickness(0, 0, 0, 5);

            button.Click += Button_Click;

            treeview1.Items.Add(button);
            buttons.Add(button);

            foreach (EpubNavigationItem nestedNavigationItem in navigationItem.NestedItems)
            {
                PrintNavigationItem(nestedNavigationItem);
            }
        }

        // показ текста
        private string PrintChapter(string currentFilePath)
        {
            string result = "";
            foreach (EpubLocalTextContentFile textContentFile in book.ReadingOrder)
            {
                result += PrintTextContentFile(textContentFile, currentFilePath);
            }
            return result;
        }
        
        // показ текста главы
        private string PrintTextContentFile(EpubLocalTextContentFile textContentFile, string currentFilePath)
        {
            if (textContentFile.FilePath != currentFilePath)
                return "";
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(textContentFile.Content);
            StringBuilder sb = new StringBuilder();
            foreach (HtmlNode node in htmlDocument.DocumentNode.SelectNodes("//text()"))
            {
                sb.AppendLine(node.InnerText.Trim());
            }
            string contentText = sb.ToString();
            return contentText;
        }

        // изменение размера текста
        private void ChangeFontSize_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is string fontSize)
            {
                double size = Convert.ToDouble(fontSize);
                richtextbox1.FontSize = size;
                settingsApp.FontSize = size;
                SaveStylePage();
            }
        }

        // изменение цвета richTextbox (страница)
        private void ChangeBackgroundColor_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Tag is string colors)
            {
                string background = colors.Substring(0, colors.LastIndexOf("#"));
                string foreground = colors.Substring(colors.LastIndexOf("#"));

                Color backgroundColor = (Color)ColorConverter.ConvertFromString(background);
                richtextbox1.Background = new SolidColorBrush(backgroundColor);

                Color foregroundColor = (Color)ColorConverter.ConvertFromString(foreground);
                richtextbox1.Foreground = new SolidColorBrush(foregroundColor);

                settingsApp.Background = background;
                settingsApp.Foreground = foreground;
                SaveStylePage();
            }
        }

        // показ контекстного меню при нажатии на текст ПКМ
        private void richtextbox1_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.RightButton == MouseButtonState.Pressed)
            {
                // Отображаем контекстное меню
                ContextMenu contextMenu = richtextbox1.ContextMenu;
                if (contextMenu != null)
                {
                    contextMenu.PlacementTarget = richtextbox1;
                    contextMenu.IsOpen = true;
                }

                // Предотвращаем стандартное контекстное меню
                e.Handled = true;
            }
        }
        
        // загрузка окна
        private void richtextbox1_Loaded(object sender, RoutedEventArgs e)
        {
            richtextbox1.ScrollToVerticalOffset(currentBook.LastWord);
        }

        // закрытие окна
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            double originalVerticalOffset = richtextbox1.VerticalOffset;

            richtextbox1.ScrollToVerticalOffset(originalVerticalOffset);
            currentBook.LastWord = originalVerticalOffset;

            string jsonString = File.ReadAllText("library.json");

            List<Book> objectsList = JsonSerializer.Deserialize<List<Book>>(jsonString);

            Book targetObject = objectsList.Find(obj => obj.Id == currentBook.Id);
            if (targetObject != null)
            {
                targetObject.LastWord = currentBook.LastWord;
            }

            string updatedJson = JsonSerializer.Serialize(objectsList);

            File.WriteAllText("library.json", updatedJson);
            SaveSettings();
            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
