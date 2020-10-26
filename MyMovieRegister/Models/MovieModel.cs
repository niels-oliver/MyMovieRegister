using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyMovieRegister.Models
{
    //Model containing a database entity and selected movie. Used to display information in the view.

    public class MovieModel
    {
        public MovieDatabaseEntities movieDB { get; set; }

        public Movie selectedMovie { get; set; }
    }
}