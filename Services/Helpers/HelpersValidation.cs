using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Services.Helpers
{
    public static class HelpersValidation
    {
        public static void ModelValidation(object? obj)
        {
            // Checking if the given object is null
            if (obj == null)
            {
                throw new ArgumentNullException(nameof(obj));
            }

            // Creating the Validation Context for validating the rules attributes
            ValidationContext validationContext = new ValidationContext(obj);

            // Creating the list that will contains all our Validation Results
            List<ValidationResult> validationResults = new List<ValidationResult>();

            // Validating all the model properties and store it into the list
            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResults, true);

            // Checking if we receive at least one invalid properties
            if (!isValid)
            {
                throw new ArgumentException(validationResults.FirstOrDefault()?.ErrorMessage);
            }

        }

       
    }
}
