﻿using System;

namespace GerenciadorDeCursos.Models
{
    public class Course
    {
        public Guid Id  { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate {get; set; }
        public string Status { get; set; }
    }
}