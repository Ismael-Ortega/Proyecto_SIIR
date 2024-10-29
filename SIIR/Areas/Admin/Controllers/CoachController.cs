﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Packaging.Signing;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using SIIR.Models.ViewModels;

namespace SIIR.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CoachController : Controller
    {
        //Se utiliza el contenedor de trabajo de Coach para almacenar los datos y utilizarlos
        private readonly IContenedorTrabajo _contenedorTrabajo;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public CoachController(IContenedorTrabajo contenedorTrabajo, IWebHostEnvironment hostingEnvironment)
        {
            _contenedorTrabajo = contenedorTrabajo;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var coach = _contenedorTrabajo.Coach.GetById(id);
            
            if (coach == null)
            {
                return NotFound();
            }

            return View(coach);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Models.Coach coach)
        {
            if (ModelState.IsValid)
            {
                string webRootPath = _hostingEnvironment.WebRootPath;
                var files = HttpContext.Request.Form.Files;
                var coachFromDb = _contenedorTrabajo.Coach.GetById(coach.Id);

                if (files.Count() > 0)
                {
                    // Editar Imagen
                    string fileName = Guid.NewGuid().ToString();
                    var uploads = Path.Combine(webRootPath, @"images\coaches");
                    var extension = Path.GetExtension(files[0].FileName);

                    var extension_new = Path.GetExtension(files[0].FileName);

                    if (!string.IsNullOrEmpty(coachFromDb.ImageUrl))
                    {
                        var imagePath = Path.Combine(webRootPath, coachFromDb.ImageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }

                    // Subimos la nueva imagen
                    using (var fileStreams = new FileStream(Path.Combine(uploads, fileName + extension_new), FileMode.Create))
                    {
                        files[0].CopyTo(fileStreams);
                    }

                    coach.ImageUrl = @"\images\coaches\" + fileName + extension_new;
                }
                else
                {
                    coach.ImageUrl = coachFromDb.ImageUrl;
                }

                _contenedorTrabajo.Coach.Update(coach);
                _contenedorTrabajo.Save();
                return RedirectToAction(nameof(Index));
            }
            return View(coach);
        }


        #region Llamadas a la API

        [HttpGet]
        public IActionResult GetAll()
        {
            return Json(new { data = _contenedorTrabajo.Coach.GetAll() });
        }

        [HttpDelete]
        public IActionResult Delete(int id)
        {
            var objFromDb = _contenedorTrabajo.Coach.GetById(id);

            var user = _contenedorTrabajo.User.GetAll(u => u.CoachId == id).FirstOrDefault();

            if (user != null)
            {
                _contenedorTrabajo.User.Remove(user);
            }

            if (objFromDb == null)
            {
                return Json(new { succes = false, message = "Error al borrar Coach"});
            }

            _contenedorTrabajo.Coach.Remove(objFromDb);
            _contenedorTrabajo.Save();
            return Json(new { succes = true, message = "Exito al borrar Coach" });
        }
        #endregion
    }
}
