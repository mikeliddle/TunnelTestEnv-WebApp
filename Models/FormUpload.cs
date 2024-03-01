using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Contains the models for the FormUpload page.
namespace FormUpload.Models
{
    public class Author
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]
        public string CountryOfOrigin { get; set; }
        [Required]
        public DateTime LastSubmissionTimestamp { get; set; }
    }

    public class DessertVote
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Dessert { get; set; }
        [Range(0, int.MaxValue)]
        public int Votes { get; set; } = 0;
    }

    public class UploadFile
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string FileName { get; set; }
    }

    public class FormData
    {
        [Key]
        public int Id { get; set; }
        public List<DessertVote> DessertVotes { get; set; }
        public string CurrentWeather { get; set; }
        public List<Author> AuthorsList { get; set; }

        public FormData()
        {
            DessertVotes = new List<DessertVote> {};
            CurrentWeather = "clear";
            AuthorsList = new List<Author> {};
        }

        public FormData(bool defaultDesserts = false)
        {
            DessertVotes = defaultDesserts ? new List<DessertVote> {
                new DessertVote { Dessert = "Peach Pie", Votes = 0 },
                new DessertVote { Dessert = "Chocolate Cookies", Votes = 0 },
                new DessertVote { Dessert = "Tiramisu", Votes = 0 }
            } : new List<DessertVote> {};
            CurrentWeather = "clear";
            AuthorsList = new List<Author> {};
        }

        public static FormData CreateFrom(IFormFileCollection files, List<DessertVote> dessertVotes, string currentWeather, List<Author> authorsList)
        {
            return new FormData
            {
                DessertVotes  = dessertVotes.Select(kv => new DessertVote { Dessert = kv.Dessert, Votes = kv.Votes }).ToList(),
                CurrentWeather = currentWeather,
                AuthorsList = authorsList
            };
        }
        public void IncrementDessertVote(string dessertChoice)
        {
            var dessert = DessertVotes.FirstOrDefault(d => d.Dessert == dessertChoice);
            if (dessert != null)
            {
                dessert.Votes++;
            }
            else
            {
                DessertVotes.Add(new DessertVote { Dessert = dessertChoice, Votes = 1 });
            }
        }
        public void AddAuthor(string name, string countryOfOrigin)
        {
            var existingAuthor = AuthorsList.FirstOrDefault(a => a.Name == name && a.CountryOfOrigin == countryOfOrigin);

            if (existingAuthor != null)
            {
                // If such an author exists, update the timestamp
                existingAuthor.LastSubmissionTimestamp = DateTime.Now;
            }
            else
            {
                // If no such author exists, add the new author
                AuthorsList.Add(new Author { Name = name, CountryOfOrigin = countryOfOrigin, LastSubmissionTimestamp = DateTime.Now });
            }
        }
        public void UpdateWeather(string weather)
        {
            CurrentWeather = weather;
        }
    }
}