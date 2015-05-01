﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MovieCatalogLibrary.MovieClasses
{
    /// <summary>
    /// Use as an imcomplete alternative to the full Movie class.
    /// </summary>
    public class CompactMovie
    {
        public int MID { get; set; }
        public int userRating { get; set; }
        public int posterNum { get; set; }
    }
}
