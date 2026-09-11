using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class CoursePath
    {
        public Guid CoursePathId { get; set; }
        public string? CourseName { get; set; }
        public string? CourseDescription { get; set; }

        public string? CourseCompetences { get; set; }

        public string? PathSubject { get; set; }

        public int PathCompletionTime { get; set; }

        public bool? CertificationCompletion { get; set; }

        public List<string>? ReferencedBook { get; set; }


    }
}
