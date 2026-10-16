using ServiceContracts.DTO.Comunities;
using ServiceContracts.DTO.Posts;
using System.Collections.Generic;

namespace CRUDCoursesApp.ViewModels
{
    public class ComunityDetailsViewModel
    {
        public ComunityResponse Comunity { get; set; } = new ComunityResponse();

        public List<PostResponse> PagedPosts { get; set; } = new List<PostResponse>();
        public int PostsPage { get; set; }
        public int PostsPageSize { get; set; }
        public int PostsTotal { get; set; }

        public List<ServiceContracts.DTO.Comunities.ComunityMemberResponse> PagedMembers { get; set; } = new List<ServiceContracts.DTO.Comunities.ComunityMemberResponse>();
        public int MembersPage { get; set; }
        public int MembersPageSize { get; set; }
        public int MembersTotal { get; set; }
    }
}
