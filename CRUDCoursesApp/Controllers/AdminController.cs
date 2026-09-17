using Entities;
using Entities.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts;
using ServiceContracts.DTO.Comunities;
using ServiceContracts.DTO.Users;
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
        public IActionResult DisplaySpecificComunity([FromRoute] Guid ComId)
        {
            return View();
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
            return View();
        }


        [HttpGet]
        [Route("admin/comunities/update/{ComId:guid}")]
        public IActionResult UpdateComunity(Guid ComId)
        {
            return View();
        }


        [HttpPost]
        [Route("admin/comunities/update/{ComId:guid}")]
        public IActionResult UpdateSpecificComunity([Bind][FromForm] UpdateComunityRequest updateComunityRequest, [FromRoute] Guid ComId)
        {
            return View();
        }

        [HttpGet]
        [Route("admin/comunities/delete/{ComId:guid}")]
        public IActionResult DeleteSpecificComunity([FromRoute] Guid ComId)
        {
            return View();
        }


        [HttpPost]
        [Route("admin/comunities/delete/{ComId:guid}")]
        public IActionResult DeleteSpecificComunityConfirm([FromRoute] Guid ComId)
        {
            return View();
        }

    }
}
