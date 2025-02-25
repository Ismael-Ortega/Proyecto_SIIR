﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using SIIR.Models.ViewModels;
using System.IO;
using System.Security.Claims;

namespace SIIR.Areas.Coach.Controllers
{
    [Area("Coach")]
    [Authorize(Roles = "Coach")]
    public class TeamController : Controller
    {
        private readonly IContenedorTrabajo _contenedorTrabajo;

        public TeamController(IContenedorTrabajo contenedorTrabajo)
        {
            _contenedorTrabajo = contenedorTrabajo;
        }

        [HttpGet]
        public IActionResult Roster(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var team = _contenedorTrabajo.Team.GetById(id);
            if (team == null)
            {
                return NotFound();
            }

            // Get the userRole and put it in the ViewBag
            ViewBag.UserRole = User.IsInRole("Admin") ? "Admin" : User.IsInRole("Coach") ? "Coach" : "Student";

            var users = _contenedorTrabajo.User.GetAll(u => u.LockoutEnd == null && u.StudentId != null).Select(u => u.StudentId).ToList();
            var students = _contenedorTrabajo.Student.GetAll(s => s.TeamId == id.Value && users.Contains(s.Id)).ToList();
            var captain = students.FirstOrDefault(s => s.IsCaptain);

            team.Representative = _contenedorTrabajo.Representative.GetById(team.RepresentativeId);
            TeamVM teamVM = new()
            {
                StudentList = students.Select(s => new SelectListItem
                {
                    Text = $"{s.Name} {s.LastName} {s.SecondLastName}",
                    Value = s.Id.ToString()
                }),
                Team = team,
                Captain = captain
            };

            team.Coach = _contenedorTrabajo.Coach.GetById(team.CoachId);
            return View(teamVM);
        }
    }
}