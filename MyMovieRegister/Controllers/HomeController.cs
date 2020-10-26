using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using System.Xml.Linq;
using MyMovieRegister.Models;

namespace MyMovieRegister.Controllers
{
    //Controller for main page.
    //Right now the only way to add a movie to the project's database is by adding them to the XML document. 
    //You are able to remove and edit movies through the view. This also updates the XML document.

    public class HomeController : Controller
    {
        MovieDatabaseEntities db = new MovieDatabaseEntities();
        string xmlPath = "~/App_Data/movies.xml";

        // GET: Index
        public ActionResult Index()
        {
            try
            {
                PopulateMovieDatabase();
            }
            catch
            {
                ViewBag.ErrorMessage = "Couldn't find or read XML file. Bummer.";
                return View("Error");
            }

            MovieModel viewMovieModel = new MovieModel();
            viewMovieModel.movieDB = db;
            return View(viewMovieModel);
        }

        [HttpPost]
        public ActionResult Index(int selectedMovieId)
        {
            MovieModel viewMovieModel = new MovieModel();
            viewMovieModel.movieDB = db;
            Movie selectedMovie = db.Movies.Find(selectedMovieId);
            viewMovieModel.selectedMovie = (Movie)selectedMovie;
            return View(viewMovieModel);
        }

        public ActionResult SaveChanges(int movieId, MovieModel editedMovieModel)
        {
            Movie movieToUpdate = db.Movies.Find(movieId);
            editedMovieModel.selectedMovie.id = movieId;
            UpdateXMLElement(movieToUpdate, editedMovieModel.selectedMovie);
            editedMovieModel.selectedMovie.posterUrl = GetPosterURL(editedMovieModel.selectedMovie.title);
            db.Entry(movieToUpdate).CurrentValues.SetValues(editedMovieModel.selectedMovie);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult DeleteMovie(int movieId)
        {
            Movie movieToDelete = db.Movies.Find(movieId);
            DeleteXMLElement(movieToDelete.title);
            db.Movies.Remove(movieToDelete);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        private void UpdateXMLElement(Movie movieToUpdate, Movie editedMovie)
        {
            XDocument xDoc = XDocument.Load(Server.MapPath(xmlPath));
            var selectedXMLElement = xDoc.Descendants("movie").Where(x => x.Element("title")?.Value == movieToUpdate.title).FirstOrDefault();
            selectedXMLElement.Element("title").SetValue(editedMovie.title);
            selectedXMLElement.Element("description").SetValue(editedMovie.description);
            selectedXMLElement.Element("length").SetValue(editedMovie.length);
            selectedXMLElement.Element("year").SetValue(editedMovie.year);
            selectedXMLElement.Element("genre").SetValue(editedMovie.genre);
            selectedXMLElement.Element("hasSeen").SetValue(editedMovie.hasSeen.ToString());
            selectedXMLElement.Element("isFavourite").SetValue(editedMovie.isFavourite.ToString());
            xDoc.Save(Server.MapPath(xmlPath));
        }

        private void DeleteXMLElement(string movieTitle)
        {
            XDocument xDoc = XDocument.Load(Server.MapPath(xmlPath));
            xDoc.Descendants("movie").Where(x => x.Element("title").Value == movieTitle).Remove();
            xDoc.Save(Server.MapPath(xmlPath));
        }

        private void PopulateMovieDatabase()
        {
            XDocument xDoc = XDocument.Load(Server.MapPath(xmlPath));
            List<Movie> movieList = xDoc.Descendants("movie").Select
               (movie =>
               new Movie
               {
                   title = movie.Element("title").Value,
                   description = movie.Element("description").Value,
                   length = movie.Element("length").Value,
                   year = movie.Element("year").Value,
                   genre = movie.Element("genre").Value,
                   hasSeen = bool.Parse(movie.Element("hasSeen").Value),
                   isFavourite = bool.Parse(movie.Element("isFavourite").Value)
               }).ToList();

            foreach (var i in movieList)
            {
                var v = db.Movies.Where(a => a.title.Equals(i.title
                    )).FirstOrDefault();
                if (v == null)
                {
                    i.posterUrl = GetPosterURL(i.title);
                    db.Movies.Add(i);
                    db.SaveChanges();
                }
            }
        }

        private string GetPosterURL(string movieTitle)
        {
            XDocument omdbXML = XDocument.Load("http://www.omdbapi.com/?t=" + movieTitle + "&r=xml&apikey=ab3f8807");

            try
            {
                return omdbXML.Root.Element("movie").Attribute("poster").Value;
            }
            catch
            {
                return "https://static.thenounproject.com/png/140281-200.png";
            }
        }
    }
}