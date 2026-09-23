using Entities;
using Entities.Enums;
using ServiceContracts.DTO.Comunities;
using ServiceContracts.DTO.Posts;
using System;

namespace ServiceContracts
{
    /// <summary>
    /// This is the represenation of the data layer of the ComunitiesService.
    /// </summary>
    public interface IComunitiesService
    {


        /// <summary>
        /// Add a new comunity to the ComunitiesService
        /// </summary>
        /// <param name="addComunityRequest">A DTO object of type AddComunityRequest which contains the adding informations of the Comunity object</param>
        /// <returns>Returns an DTO object of type ComunityResponse that contains all the informations of the comunity</returns>
        ComunityResponse AddComunity(AddComunityRequest? addComunityRequest);

        /// <summary>
        /// Get all comunities contained inside the ComunitiesService
        /// </summary>
        /// <returns>Returns a list of DTO object of type ComunityResponse that contains all the informations of the comunities</returns>
        List<ComunityResponse> GetAllComunities();


        /// <summary>
        /// Get the counts of all the comunities contained inside the ComunitiesService
        /// </summary>
        /// <returns>An integer representing the number of comunities in the system</returns>
        public int GetAllComunitiesCount();


        /// <summary>
        /// Get the counts of all posts contained inside a specific comunity
        /// </summary>
        /// <returns>An integer representing the number of posts in desired comunity</returns>
        public int GetComunityPostsCount(Guid comunityId);


        /// <summary>
        /// Get the desired comunity from the ComunitiesService
        /// </summary>
        /// <param name="ComId">The desired Comunity Id to get from the ComunitiesService</param>
        /// <returns>Returns an DTO object of type ComunityResponse that contains all the informations of the comunity</returns>
        ComunityResponse GetComunityByComId(Guid ComId);

        /// <summary>
        /// Update the desired comunity from the ComunitiesService
        /// </summary>
        /// <param name="updateComunityRequest">A DTO object of type UpdateComunityRequest which contains the updating informations of the Comunity object</param>
        /// <returns>Returns an DTO object of type ComunityResponse that contains all the informations of the comunity</returns>
        ComunityResponse UpdateComunity(UpdateComunityRequest updateComunityRequest);

        /// <summary>
        /// Delete the desired comunity from the ComunitiesService
        /// </summary>
        /// <param name="ComId">The desired Comunity Id to delete from the ComunitiesService</param>
        /// <returns>True if the operation is successful, otherwise False</returns>
        bool DeleteComunityByComId(Guid ComId);

        /// <summary>
        /// Get all the comunities contained inside the ComunitiesService filtered by the property name and the search string
        /// </summary>
        /// <param name="searchBy">The property name to filter in</param>
        /// <param name="searchString">The search string to apply filter on</param>
        /// <returns>Returns a list of filtered DTO object of type ComunityResponse that contains all the informations of the comunity</returns>
        List<ComunityResponse> GetFilteredComunities(string searchBy, string searchString);

        /// <summary>
        /// Get all the comunities contained inside the ComunitiesService sorted by the property name and the sort order
        /// </summary>
        /// <param name="allComResponses">The current state of the comunities responses list</param>
        /// <param name="sortBy">The property name to sort by</param>
        /// <param name="sortOrder">The sort order, ASC or DESC</param>
        /// <returns>Returns a list of sorted DTO object of type ComunityResponse that contains all the informations of the comunity</returns>
        public List<ComunityResponse> GetSortedComunities(List<ComunityResponse> allComResponses, string sortBy, SortOption sortOrder);




    }
}
