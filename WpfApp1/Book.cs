using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace WpfApp1
{
    // класс для хранения информации о книге
    public class Book
    {
        public int Id { get; set; }
        public string FilePath { get; set; } = String.Empty;
        public string Title { get; set; } = String.Empty;
        public string Format { get; set; } = String.Empty;
        public string LastChapter { get; set; } = String.Empty;
        public double LastWord { get; set; } = 0;
        public bool HasCover { get; set; }

        // возвращает список объектов из json
        static public List<Book> loadBooks(string jsonPath)
        {
            if (!File.Exists(jsonPath))
            {
                var emptyList = new int[] { };
                var jsonString = JsonSerializer.Serialize(emptyList);
                File.WriteAllText(jsonPath, jsonString);
            }
            string jsonText = File.ReadAllText(jsonPath);
            return JsonSerializer.Deserialize<List<Book>>(jsonText);
        }

        // перемещает информацию одного объекта в другой
        static public void MoveInfoBook(Book emptyBook, Book fullBook)
        {
            emptyBook.Id = fullBook.Id;
            emptyBook.FilePath = fullBook.FilePath;
            emptyBook.Title = fullBook.Title;
            emptyBook.Format = fullBook.Format;
            emptyBook.LastChapter = fullBook.LastChapter;
            emptyBook.LastWord = fullBook.LastWord;
            emptyBook.HasCover = fullBook.HasCover;
        }
    }
}
