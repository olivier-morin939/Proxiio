using Entities;
using Entities.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts;
using ServiceContracts.DTO.Comunities;
using ServiceContracts.DTO.Users;
using ServiceContracts.DTO.Posts;
using Services;
using System.Collections.Immutable;
using System.Data;
using System.Globalization;
using System.Net;

namespace CRUDCoursesApp.Controllers
{
    public class AdminController : Controller
    {

        private readonly IUsersService _usersService;
        private readonly IComunitiesService _comunitiesService;

        public AdminController(IUsersService usersService, IComunitiesService comunitiesService)
        {
            _usersService = usersService;
            _comunitiesService = comunitiesService;


            if (_usersService.GetAllUsersCount() == 0)
                _usersService.SeedMockUsers();

            if (_comunitiesService.GetAllComunitiesCount() == 0)
            {
                _comunitiesService.SeedMockComunities();
                _comunitiesService.SeedMockPosts();
            }

        }

        // safe setter for TempData to avoid NullReference during unit tests
        private void SafeSetTempData(string key, string? value)
        {
            if (TempData != null && value != null)
            {
                TempData[key] = value;
            }
        }


        [HttpGet]
        [Route("admin")]
        public IActionResult Index()
        {
            // Fill the quick stats informations
            ViewBag.TotalUserCounts = _usersService.GetAllUsersCount();
            ViewBag.TotalComunitiesCounts = _comunitiesService.GetAllComunitiesCount();
            ViewBag.TotalPostsCounts = _comunitiesService.GetAllPostsCount();
            //Not implemented yet
            ViewBag.TotalLikesCounts = 0;
            //Not implemented yet
            ViewBag.TotalSignalsCounts = 0;
            return View();
        }


        [HttpGet]
        [Route("admin/users/view")]
        public IActionResult DisplayUsers([FromQuery] string searchBy, [FromQuery] string searchString, [FromQuery] string sortBy = nameof(UserResponse.Name), [FromQuery] SortOption sortOption = SortOption.ASC)
        {

            ViewBag.SearchFields = new Dictionary<string, string>()
            {
                {nameof(UserResponse.Name), "User Name"},
                {nameof(UserResponse.Email), "User Email"},
                {nameof(UserResponse.Role), "User Role"},
                 {nameof(UserResponse.UserState), "User State"},
                {nameof(UserResponse.DateOfBirth), "Date of Birth"}
            };

            // Search and filter
            List<UserResponse> allFilteredResponses = _usersService.GetFilteredUsers(searchBy, searchString);
            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentSearchString = searchString;


            // Sort
            List<UserResponse> sortedUserResponses = _usersService.GetSortedUsers(allFilteredResponses, sortBy, sortOption);
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOption.ToString();


            return View(sortedUserResponses);
        }


        [HttpGet]
        [Route("admin/users/view/{UserId:guid}")]
        public IActionResult DisplaySpecificUser([FromRoute] Guid UserId)
        {
            try
            {
                UserResponse matchingUserResponse = _usersService.GetUserById(UserId);
                return View(matchingUserResponse);
            }
            catch (ArgumentNullException ex) { 
                
                ViewBag.ErrorMessage = ex.Message;
            }

            return View();
        }


        [HttpGet]
        [Route("admin/users/add")]
        public IActionResult AddUser()
        {

            return View();
        }


        [HttpPost]
        [Route("admin/users/add")]
        public IActionResult AddUser([FromForm] AddUserRequest newUserAddRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    List<string> errorsList = ModelState.SelectMany(e => e.Value.Errors).Select(e => e.ErrorMessage).ToList();
                    string errors = string.Join("\n", errorsList);
                    TempData["ErrorMessage"] = errors;
                    return RedirectToAction("AddUser", "Admin");
                }

                UserResponse newUserResponse = _usersService.AddUser(newUserAddRequest);
                TempData["SuccessMessage"] = "User added with success !";
                return RedirectToAction("DisplayUsers", "Admin");
            }
            catch (DuplicateNameException dne)
            {
                TempData["DuplicateErrorMessage"] = dne.Message;
            }
            catch (Exception ex)
            {
                TempData["InvalidErrorMessage"] = ex.Message;
            }

            return RedirectToAction("AddUser", "Admin");
        }


        [HttpGet]
        [Route("admin/users/update/{UserId:guid}")]
        public IActionResult UpdateSpecificUser([FromRoute] Guid UserId)
        {
            try
            {
                UserResponse userResponse = _usersService.GetUserById(UserId);
                UpdateUserRequest updateRequest = userResponse.ToUpdateUserRequest();
                return View(updateRequest);
            }
            catch (Exception)
            {
                return LocalRedirect("/not-found");
            }
        }


        [HttpPost]
        [Route("admin/users/update/{UserId:guid}")]
        public IActionResult UpdateSpecificUser([FromForm] UpdateUserRequest updateUserRequest,[FromRoute] Guid UserId)
        {

            try
            {
                if (!ModelState.IsValid)
                {
                    List<string> errorsList = ModelState.SelectMany(e => e.Value.Errors).Select(e => e.ErrorMessage).ToList();
                    string errors = string.Join("\n", errorsList);
                    TempData["ErrorMessage"] = errors;
                    return RedirectToAction("UpdateSpecificUser", "Admin");
                }

                updateUserRequest.UserId = UserId;
                UserResponse updatedUser = _usersService.UpdateUser(updateUserRequest);
                TempData["SuccessMessage"] = "User updated with success !";
                return RedirectToAction("DisplayUsers");
            }
            catch (DuplicateNameException dne)
            {
                TempData["DuplicateErrorMessage"] = dne.Message;
            }
            catch (Exception ex)
            {
                TempData["InvalidErrorMessage"] = ex.Message;
            }
            return RedirectToAction("UpdateSpecificUser", "Admin");
        }


        [HttpGet]
        [Route("admin/users/delete/{UserId:guid}")]
        public IActionResult DeleteSpecificUser([FromRoute] Guid UserId)
        {
            try
            {
                UserResponse userResponse = _usersService.GetUserById(UserId);
                return View(userResponse);
            }
            catch (Exception)
            {
                return LocalRedirect("/not-found");
            }
            
        }


        [HttpPost]
        [Route("admin/users/delete/{UserId:guid}")]
        public IActionResult DeleteSpecificUserConfirm([FromRoute] Guid UserId)
        {
            try
            {
                bool isDeleted = _usersService.DeleteUser(UserId);
                if (isDeleted)
                {
                    TempData["SuccessMessage"] = "User deleted with success !";
                    return RedirectToAction("DisplayUsers", "Admin");
                }
                else
                {
                    TempData["InvalidErrorMessage"] = "User deletion failed !";
                    return RedirectToAction("DeleteSpecificUser", "Admin");
                }
            }
            catch (Exception)
            {
                TempData["InvalidErrorMessage"] = "User deletion failed !";
            }
            return RedirectToAction("DeleteSpecificUser", "Admin");
        }


        [HttpGet]
        [Route("admin/comunities/view")]
        public IActionResult DisplayComunities([FromQuery] string searchBy, [FromQuery] string searchString, [FromQuery] string sortBy = nameof(ComunityResponse.Name), [FromQuery] SortOption sortOption = SortOption.ASC)
        {
            ViewBag.SearchFields = new Dictionary<string, string>()
            {
                {nameof(ComunityResponse.Name), "Comunity Name"},
                {nameof(ComunityResponse.Description), "Comunity Description"},
                {nameof(ComunityResponse.TeacherId), "Teacher Id"},
                {nameof(ComunityResponse.UsersCount), "User Counts"},
                {nameof(ComunityResponse.PostsCount), "Posts Counts"}
            };


            // Search and filter
            List<ComunityResponse> allFilteredResponses = _comunitiesService.GetFilteredComunities(searchBy, searchString);
            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentSearchString = searchString;


            // Sort
            List<ComunityResponse> sortedComunityResponses = _comunitiesService.GetSortedComunities(allFilteredResponses, sortBy, sortOption);
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOption.ToString();

            return View(sortedComunityResponses);
        }

        [HttpGet]
        [Route("admin/comunities/view/{ComId:guid}")]
        public IActionResult DisplaySpecificComunity([FromRoute] Guid ComId, [FromQuery] int postsPage = 1, [FromQuery] int postsPageSize = 8, [FromQuery] int membersPage = 1, [FromQuery] int membersPageSize = 8)
        {
            try
            {
                ComunityResponse comResponse = _comunitiesService.GetComunityByComId(ComId);

                // prepare paged posts
                var allPosts = comResponse.Posts ?? new List<ServiceContracts.DTO.Posts.PostResponse>();
                int postsTotal = allPosts.Count;
                var pagedPosts = allPosts.Skip((Math.Max(1, postsPage) - 1) * postsPageSize).Take(postsPageSize).ToList();

                // prepare paged members
                var allMembers = _comunitiesService.GetComunityMembers(ComId) ?? new List<ComunityMemberResponse>();
                int membersTotal = allMembers.Count;
                var pagedMembers = allMembers.Skip((Math.Max(1, membersPage) - 1) * membersPageSize).Take(membersPageSize).ToList();

                var vm = new CRUDCoursesApp.ViewModels.ComunityDetailsViewModel()
                {
                    Comunity = comResponse,
                    PagedPosts = pagedPosts,
                    PostsPage = postsPage,
                    PostsPageSize = postsPageSize,
                    PostsTotal = postsTotal,
                    PagedMembers = pagedMembers,
                    MembersPage = membersPage,
                    MembersPageSize = membersPageSize,
                    MembersTotal = membersTotal
                };

                return View(vm);
            }
            catch (Exception)
            {
                return LocalRedirect("/not-found");
            }
        }

        [HttpGet]
        [Route("admin/comunities/add")]
        public IActionResult AddComunity()
        {
            return View();
        }

        [HttpPost]
        [Route("admin/comunities/add")]
        public IActionResult AddComunity([Bind][FromForm] AddComunityRequest addComunityRequest)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    List<string> errorsList = ModelState.SelectMany(e => e.Value.Errors).Select(e => e.ErrorMessage).ToList();
                    string errors = string.Join("\n", errorsList);
                    TempData["ErrorMessage"] = errors;
                    return RedirectToAction("AddComunity", "Admin");
                }

                var created = _comunitiesService.AddComunity(addComunityRequest);
                TempData["SuccessMessage"] = "Community added successfully!";
                return RedirectToAction("DisplayComunities", "Admin");
            }
            catch (DuplicateNameException dne)
            {
                TempData["DuplicateErrorMessage"] = dne.Message;
            }
            catch (Exception ex)
            {
                TempData["InvalidErrorMessage"] = ex.Message;
            }

            return RedirectToAction("AddComunity", "Admin");
        }


        [HttpGet]
        [Route("admin/comunities/update/{ComId:guid}")]
        public IActionResult UpdateComunity(Guid ComId)
        {
            try
            {
                ComunityResponse comResponse = _comunitiesService.GetComunityByComId(ComId);
                return View(comResponse);
            }
            catch (Exception)
            {
                return LocalRedirect("/not-found");
            }
        }


        [HttpPost]
        [Route("admin/comunities/update/{ComId:guid}")]
        public IActionResult UpdateSpecificComunity([Bind][FromForm] UpdateComunityRequest updateComunityRequest, [FromRoute] Guid ComId)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errorsList = ModelState.SelectMany(e => e.Value.Errors).Select(e => e.ErrorMessage).ToList();
                    SafeSetTempData("ErrorMessage", string.Join("\n", errorsList));
                    return RedirectToAction("UpdateComunity", new { ComId });
                }

                // ensure Id and TeacherId are set
                updateComunityRequest.Id = ComId;
                if (updateComunityRequest.TeacherId == Guid.Empty)
                {
                    var existing = _comunitiesService.GetComunityByComId(ComId);
                    updateComunityRequest.TeacherId = existing.TeacherId;
                }

                var updated = _comunitiesService.UpdateComunity(updateComunityRequest);
                SafeSetTempData("SuccessMessage", "Community updated successfully!");
                return RedirectToAction("DisplaySpecificComunity", new { ComId });
            }
            catch (DuplicateNameException dne)
            {
                SafeSetTempData("DuplicateErrorMessage", dne.Message);
            }
            catch (Exception ex)
            {
                SafeSetTempData("InvalidErrorMessage", ex.Message);
            }

            return RedirectToAction("UpdateComunity", new { ComId });
        }

        [HttpGet]
        [Route("admin/comunities/delete/{ComId:guid}")]
        public IActionResult DeleteSpecificComunity([FromRoute] Guid ComId)
        {
            try
            {
                ComunityResponse comResponse = _comunitiesService.GetComunityByComId(ComId);
                return View(comResponse);
            }
            catch (Exception)
            {
                return LocalRedirect("/not-found");
            }
        }


        [HttpPost]
        [Route("admin/comunities/delete/{ComId:guid}")]
        public IActionResult DeleteSpecificComunityConfirm([FromRoute] Guid ComId)
        {
            try
            {
                bool deleted = _comunitiesService.DeleteComunityByComId(ComId);
                if (deleted)
                {
                    SafeSetTempData("SuccessMessage", "Community deleted successfully.");
                    return RedirectToAction("DisplayComunities");
                }
                else
                {
                    SafeSetTempData("InvalidErrorMessage", "Community could not be deleted.");
                    return RedirectToAction("DisplaySpecificComunity", new { ComId });
                }
            }
            catch (Exception ex)
            {
                SafeSetTempData("InvalidErrorMessage", ex.Message);
                return RedirectToAction("DisplaySpecificComunity", new { ComId });
            }
        }

    }
}
