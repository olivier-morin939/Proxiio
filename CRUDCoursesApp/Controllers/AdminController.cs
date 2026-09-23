using Entities;
using Entities.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts;
using ServiceContracts.DTO.Comunities;
using ServiceContracts.DTO.Users;
using ServiceContracts.DTO.Posts;
using ServiceContracts.DTO.Reports;
using ServiceContracts.DTO.Signals;
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
        private readonly IPostsService _postsService;

        private readonly IComunityMembersService _comunityMembersService;
        private readonly IComunitiesService _comunitiesService;
        private readonly IReportsService _reportsService;
        private readonly ISignalsService _signalsService;

        public AdminController(
            IUsersService usersService, 
            IComunitiesService comunitiesService,
            IComunityMembersService communityMembersService,
            IPostsService postsService,
            IReportsService reportsService,
            ISignalsService signalsService
            
            )
        {
            _usersService = usersService;
            _comunitiesService = comunitiesService;
            _postsService = postsService;
            _comunityMembersService = communityMembersService;
            _reportsService = reportsService;
            _signalsService = signalsService;

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
            ViewBag.TotalPostsCounts = _postsService.GetAllPostsCount();
            ViewBag.TotalSignalsCounts = _signalsService.GetAllSignals().Count;
            ViewBag.TotalReportsCounts = _reportsService.GetAllReports().Count;
            return View();
        }


        #region RoutesToUsersCRUD

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
                    List<string> errorsList = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage)).ToList();
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
                    List<string> errorsList = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage)).ToList();
                    string errors = string.Join("\n", errorsList);
                    TempData["ErrorMessage"] = errors;
                    return RedirectToAction(nameof(UpdateSpecificUser), new { UserId });
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
            return RedirectToAction(nameof(UpdateSpecificUser), new { UserId });
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
                    return RedirectToAction(nameof(DeleteSpecificUser), new { UserId });
                }
            }
            catch (Exception)
            {
                TempData["InvalidErrorMessage"] = "User deletion failed !";
            }
            return RedirectToAction(nameof(DeleteSpecificUser), new { UserId });
        }

        #endregion

        #region RoutesToComunitiesCRUD
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
                var allPosts = _postsService.GetAllFilteredPostsByComunitiy(ComId, string.Empty, string.Empty);
                int postsTotal = allPosts.Count;
                postsPage = Math.Max(1, postsPage);
                postsPageSize = Math.Clamp(postsPageSize, 1, 50);
                membersPage = Math.Max(1, membersPage);
                membersPageSize = Math.Clamp(membersPageSize, 1, 50);
                var pagedPosts = allPosts.Skip((postsPage - 1) * postsPageSize).Take(postsPageSize).ToList();

                // prepare paged members
                var allMembers = _comunityMembersService.GetComunityMembers(ComId) ?? new List<ComunityMemberResponse>();
                int membersTotal = allMembers.Count;
                var pagedMembers = allMembers.Skip((membersPage - 1) * membersPageSize).Take(membersPageSize).ToList();

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
                    List<string> errorsList = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage)).ToList();
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
                    var errorsList = ModelState.Values.SelectMany(value => value.Errors.Select(error => error.ErrorMessage)).ToList();
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

        #endregion

        #region RoutesForUsersInComunity
        [HttpGet]
        [Route("admin/comunities/view/{ComId:guid}/users/view")]
        public IActionResult DisplayUserByComunity([FromRoute] Guid ComId)
        {
            try
            {
                var comResponse = _comunitiesService.GetComunityByComId(ComId);
                ViewBag.ComId = ComId;
                return View(comResponse.Users ?? new List<UserResponse>());
            }
            catch (ArgumentNullException) { return NotFound(); }
        }


        [HttpGet]
        [Route("admin/comunities/view/{ComId:guid}/users/add")]
        public IActionResult AddUserByComunity([FromRoute] Guid ComId)
        {
            if (!_comunitiesService.GetAllComunities().Any(c => c.Id == ComId)) return NotFound();
            ViewBag.ComId = ComId;
            return View();
        }

        [HttpPost]
        [Route("admin/comunities/view/{ComId:guid}/users/add")]
        public IActionResult AddUserByComunity([FromRoute] Guid ComId, [Bind][FromForm] AddUserRequest addUserRequest)
        {
            if (!ModelState.IsValid) { TempData["ErrorMessage"] = "Vérifie les renseignements du compte."; return RedirectToAction(nameof(AddUserByComunity), new { ComId }); }
            try
            {
                var user = _usersService.AddUser(addUserRequest);
                _comunityMembersService.AddComunityMember(new AddComunityMemberRequest { ComunityId = ComId, UserId = user.UserId, Role = ComunityRole.Member });
                TempData["SuccessMessage"] = "Le membre a été ajouté à la communauté.";
                return RedirectToAction(nameof(DisplayUserByComunity), new { ComId });
            }
            catch (Exception ex) { TempData["ErrorMessage"] = ex.Message; return RedirectToAction(nameof(AddUserByComunity), new { ComId }); }
        }


        [HttpGet]
        [Route("admin/comunities/view/{ComId:guid}/users/delete/{UserId:guid}")]
        public IActionResult ConfirmDeleteUserByComunity([FromRoute] Guid ComId, [FromRoute] Guid UserId)
        {
            try { ViewBag.ComId = ComId; return View(_usersService.GetUserById(UserId)); }
            catch (ArgumentNullException) { return NotFound(); }
        }


        [HttpPost]
        [Route("admin/comunities/view/{ComId:guid}/users/delete/{UserId:guid}")]
        public IActionResult DeleteUserByComunity([FromRoute] Guid ComId, [FromRoute] Guid UserId)
        {
            if (!_comunityMembersService.RemoveComunityMember(ComId, UserId)) TempData["ErrorMessage"] = "Ce membre n'a pas été trouvé dans cette communauté.";
            else TempData["SuccessMessage"] = "Le membre a été retiré de la communauté.";
            return RedirectToAction(nameof(DisplayUserByComunity), new { ComId });
        }
        #endregion

        #region RoutesForPostsInComunity
        [HttpGet]
        [Route("admin/comunities/view/{ComId:guid}/posts/view")]
        public IActionResult DisplayPostsByComunity([FromRoute] Guid ComId, [FromQuery] string searchBy, [FromQuery] string searchString, [FromQuery] string sortBy = nameof(PostResponse.Title), [FromQuery] SortOption sortOrder = SortOption.ASC)
        {
            ViewBag.SearchFields = new Dictionary<string, string>()
            {
                {nameof(PostResponse.UserId), "Owner Id"},
                {nameof(PostResponse.Title), "Title"},
                {nameof(PostResponse.Body), "Body"},
                {nameof(PostResponse.CreatedAt), "Created At"}
            };


            try
            {
                ComunityResponse comResponse = _comunitiesService.GetComunityByComId(ComId);

                // Filter the posts
                List<PostResponse> comunityFilteredPosts = _postsService.GetAllFilteredPostsByComunitiy(ComId, searchBy, searchString);
                ViewBag.CurrentSearchBy = searchBy;
                ViewBag.CurrentSearchString = searchString;

                // Sort the posts
                List<PostResponse> comunitySortedPosts = _postsService.GetAllSortedPostsByComunity(ComId, comunityFilteredPosts, sortBy, sortOrder);
                ViewBag.CurrentSortBy = sortBy;
                ViewBag.CurrentSortOrder = sortOrder.ToString();
                ViewBag.ComId = ComId;
                ViewBag.PageTitle = $"Publications · {comResponse.Name}";

                return View(comunitySortedPosts);
            }
            catch (ArgumentNullException)
            {
                return LocalRedirect("/not-found");
            }
        }


        [HttpGet]
        [Route("admin/comunities/view/{ComId:guid}/posts/add")]
        public IActionResult AddPostByComunity([FromRoute] Guid ComId)
        {
            try { _comunitiesService.GetComunityByComId(ComId); }
            catch (ArgumentNullException) { return NotFound(); }
            ViewBag.ComId = ComId;
            ViewBag.Users = _comunitiesService.GetComunityByComId(ComId).Users ?? new List<UserResponse>();
            return View();
        }

        [HttpPost]
        [Route("admin/comunities/view/{ComId:guid}/posts/add")]
        public IActionResult AddPostByComunity([FromRoute] Guid ComId, [Bind][FromForm] AddPostRequest addPostRequest)
        {
            if (!ModelState.IsValid) { TempData["ErrorMessage"] = "Vérifie le titre, le message et l'auteur."; return RedirectToAction(nameof(AddPostByComunity), new { ComId }); }
            try
            {
                addPostRequest.ComunityId = ComId;
                _postsService.AddPost(addPostRequest);
                TempData["SuccessMessage"] = "La publication a été créée.";
                return RedirectToAction(nameof(DisplayPostsByComunity), new { ComId });
            }
            catch (Exception ex) { TempData["ErrorMessage"] = ex.Message; return RedirectToAction(nameof(AddPostByComunity), new { ComId }); }
        }


        [HttpPost]
        [Route("admin/comunities/view/{ComId:guid}/posts/delete/{UserId:guid}")]
        public IActionResult DeletePostsByComunity([FromRoute] Guid ComId, [FromRoute] Guid UserId)
        {
            if (!_postsService.DeletePostByPostId(UserId)) TempData["ErrorMessage"] = "La publication est introuvable.";
            else TempData["SuccessMessage"] = "La publication a été supprimée.";
            return RedirectToAction(nameof(DisplayPostsByComunity), new { ComId });
        }

        [HttpGet]
        [Route("admin/comunities/view/{ComId:guid}/posts/update/{UserId:guid}")]
        public IActionResult UpdatePostByComunity(Guid ComId, Guid UserId)
        {
            try
            {
                var post = _postsService.GetPostByPostId(UserId);
                ViewBag.ComId = ComId;
                return View(new UpdatePostRequest { Id = post.Id, ComunityId = ComId, UserId = post.UserId, Title = post.Title ?? "", Body = post.Body ?? "", ImagesPath = post.ImagesPath ?? new(), AdditionalsPath = post.AdditionalsPath ?? new() });
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPost]
        [Route("admin/comunities/view/{ComId:guid}/posts/update/{UserId:guid}")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdatePostByComunity(Guid ComId, Guid UserId, UpdatePostRequest request)
        {
            if (!ModelState.IsValid) return RedirectToAction(nameof(UpdatePostByComunity), new { ComId, UserId });
            try
            {
                request.Id = UserId;
                request.ComunityId = ComId;
                _postsService.UpdatePost(request);
                TempData["SuccessMessage"] = "La publication a été mise à jour.";
            }
            catch (Exception ex) { TempData["ErrorMessage"] = ex.Message; }
            return RedirectToAction(nameof(DisplayPostsByComunity), new { ComId });
        }
        #endregion

        [HttpGet("/admin/posts")]
        public IActionResult DisplayPosts()
        {
            ViewBag.PageTitle = "Toutes les publications";
            ViewBag.ComId = Guid.Empty;
            return View("DisplayPostsByComunity", _postsService.GetAllPosts());
        }

        [HttpGet("/admin/signals")]
        public IActionResult DisplaySignals() => View(_signalsService.GetAllSignals());

        [HttpGet("/admin/signals/add")]
        public IActionResult AddSignal() => View(new AddSignalRequest());

        [HttpPost("/admin/signals/add")]
        [ValidateAntiForgeryToken]
        public IActionResult AddSignal(AddSignalRequest request)
        {
            if (!ModelState.IsValid) return View(request);
            _signalsService.AddSignal(request);
            TempData["SuccessMessage"] = "Le signalement technique a été créé.";
            return RedirectToAction(nameof(DisplaySignals));
        }

        [HttpGet("/admin/signals/update/{id:guid}")]
        public IActionResult UpdateSignal(Guid id)
        {
            try
            {
                var signal = _signalsService.GetSignalById(id);
                return View(new UpdateSignalRequest { Id = signal.Id, ProblemName = signal.ProblemName, ProblemDescription = signal.ProblemDescription, Level = signal.Level, Status = signal.Status, IsConfirmed = signal.IsConfirmed });
            }
            catch (KeyNotFoundException) { return NotFound(); }
        }

        [HttpPost("/admin/signals/update/{id:guid}")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateSignal(Guid id, UpdateSignalRequest request)
        {
            if (!ModelState.IsValid) return View(request);
            request.Id = id;
            _signalsService.UpdateSignal(request);
            TempData["SuccessMessage"] = "Le signal technique a été mis à jour.";
            return RedirectToAction(nameof(DisplaySignals));
        }

        [HttpPost("/admin/signals/delete/{id:guid}")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSignal(Guid id)
        {
            var deleted = _signalsService.DeleteSignal(id);
            TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted ? "Le signal technique a été supprimé." : "Ce signal technique est introuvable.";
            return RedirectToAction(nameof(DisplaySignals));
        }

        [HttpGet("/admin/reports")]
        public IActionResult DisplayReports(Guid? postId = null)
        {
            var reports = _reportsService.GetAllReports();
            if (postId.HasValue) reports = reports.Where(r => r.PostId == postId.Value).ToList();
            ViewBag.PostId = postId;
            ViewBag.PostTitles = _postsService.GetAllPosts().ToDictionary(p => p.Id, p => p.Title ?? "Publication");
            return View(reports);
        }

        [HttpGet("/admin/reports/add")]
        public IActionResult AddReport()
        {
            ViewBag.Posts = _postsService.GetAllPosts();
            return View(new AddReportRequest());
        }

        [HttpPost("/admin/reports/add")]
        [ValidateAntiForgeryToken]
        public IActionResult AddReport(AddReportRequest request)
        {
            if (!ModelState.IsValid) { ViewBag.Posts = _postsService.GetAllPosts(); return View(request); }
            try
            {
                request.StatusOfReport = ReportStatus.Received;
                _reportsService.AddReport(request);
                TempData["SuccessMessage"] = "Le signalement de publication a été créé.";
                return RedirectToAction(nameof(DisplayReports));
            }
            catch (Exception ex) { ModelState.AddModelError(string.Empty, ex.Message); ViewBag.Posts = _postsService.GetAllPosts(); return View(request); }
        }

        [HttpPost("/admin/reports/status/{id:guid}")]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateReportStatus(Guid id, ReportStatus status)
        {
            try
            {
                var report = _reportsService.GetReportByReportId(id);
                _reportsService.UpdateReport(new UpdateReportRequest { Id = report.Id, PostId = report.PostId, TypeOfReport = report.TypeOfReport, MessageOfReport = report.MessageOfReport, StatusOfReport = status });
                TempData["SuccessMessage"] = "Le statut du signalement a été mis à jour.";
            }
            catch (Exception ex) { TempData["ErrorMessage"] = ex.Message; }
            return RedirectToAction(nameof(DisplayReports));
        }

        [HttpPost("/admin/reports/delete/{id:guid}")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteReport(Guid id)
        {
            var deleted = _reportsService.DeleteReportByReportId(id);
            TempData[deleted ? "SuccessMessage" : "ErrorMessage"] = deleted ? "Le signalement a été supprimé." : "Ce signalement est introuvable.";
            return RedirectToAction(nameof(DisplayReports));
        }

    }
}
