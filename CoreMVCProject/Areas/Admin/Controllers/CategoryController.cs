using CoreMVCProject.CommonHelper;
using CoreMVCProject.DataAccessLayer;
using CoreMVCProject.DataAccessLayer.Infrastructure.IRepository;
using CoreMVCProject.Models;
using CoreMVCProject.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoreMVCProjectWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =WebsiteRole.Role_Admin)]
    public class CategoryController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            CategoryVM vm = new(); ;
            vm.Categories = _unitOfWork.Category.GetAll();
            return View(vm);
        }
        //public IActionResult Create()
        //{
        //    return View();
        //}
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public IActionResult Create(Category category)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _unitOfWork.Category.Add(category);
        //        _unitOfWork.Save();
        //        TempData["successMsg"] = "Category created successfully.";
        //        return RedirectToAction("Index");
        //    }
        //    return View(category);
        //}
        public IActionResult CreateUpdate(int? id)
        {
            CategoryVM vm = new();
            if (id == null || id == 0)
            {
                return View(vm);
            }
            else
            {
                vm.Category = _unitOfWork.Category.GetT(x => x.Id == id);
                if (vm.Category == null)
                {
                    return NotFound();
                }
                return View(vm);
            }

        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateUpdate(CategoryVM categoryVM)
        {
            if (ModelState.IsValid)
            {
                if (categoryVM.Category.Id == 0)
                {
                    _unitOfWork.Category.Add(categoryVM.Category);
                    TempData["successMsg"] = "Category created successfully.";
                }
                else
                {
                    _unitOfWork.Category.Update(categoryVM.Category);
                    TempData["successMsg"] = "Category updated successfully.";
                    
                }
                _unitOfWork.Save();
                return RedirectToAction("Index");
            }
            return View(categoryVM);
        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var category = _unitOfWork.Category.GetT(x => x.Id == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteData(int? id)
        {
            if (ModelState.IsValid)
            {
                if (id == null || id == 0)
                {
                    return NotFound();
                }
                var category = _unitOfWork.Category.GetT(x => x.Id == id);
                if (category == null)
                {
                    return NotFound();
                }
                _unitOfWork.Category.Delete(category);
                _unitOfWork.Save();
                TempData["errorMsg"] = "Category deleted successfully.";
                return RedirectToAction("Index");
            }
            return View();
        }
    }
}
