using ServiceContracts.DTO.Comunities;
using System;
using System.Collections.Generic;

namespace ServiceContracts
{
    /// <summary>
    /// This is the represenation of the data layer of the ComunityMembersService.
    /// </summary>
    public interface IComunityMembersService
    {

        /// <summary>
        /// Add a new member into the desired comunity
        /// </summary>
        /// <param name="addComunityMemberRequest">The DTO object containing the adding informations of the member of the desired community</param>
        /// <returns>Returns an DTO object of type CommunityMemberResponse that contains all the informations about the newly created member</returns>
        Task<ComunityMemberResponse> AddComunityMember(AddComunityMemberRequest? addComunityMemberRequest);

        /// <summary>
        /// Get all the community members inside a specific community
        /// </summary>
        /// <param name="comunityId">The desired community id to target</param>
        /// <returns>Returns a list of DTO object of type CommunityMemberResponse that contains all the informations about the members</returns>
        Task<List<ComunityMemberResponse>> GetComunityMembers(Guid comunityId);

        Task<bool> IsComunityMember(Guid comunityId, Guid userId);

        /// <summary>
        /// Get the members count of a specific community
        /// </summary>
        /// <param name="comunityId">The desired community id to target</param>
        /// <returns>The members count(int) of the desired community</returns>
        Task<int> GetComunityMembersCount(Guid comunityId);

        /// <summary>
        /// Remove a currently registered member from a community
        /// </summary>
        /// <param name="comunityId">The desired community id to target</param>
        /// <param name="userId">The desired user id to remove</param>
        /// <returns>True if the operation was successful, otherwise False</returns>
        Task<bool> RemoveComunityMember(Guid comunityId, Guid userId);

        /// <summary>
        /// Update a registered member from a community
        /// </summary>
        /// <param name="updateComunityMemberRequest">The DTO object containing the updating informations of the member of the desired community</param>
        /// <returns>Returns an DTO object of type CommunityMemberResponse that contains all the information about the updated member</returns>
        Task<ComunityMemberResponse> UpdateComunityMember(UpdateComunityMemberRequest updateComunityMemberRequest);
    }
}
