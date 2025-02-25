﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Logging;
using SIIR.DataAccess.Data.Repository;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using SIIR.Models.ViewModels;
using SIIR.Utilities;
using static QuestPDF.Helpers.Colors;

namespace SIIR.Areas.Identity.Pages.Account
{
    [Authorize(Roles = "Admin, Coach")]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ITeamRepository _teamRepository;
		private readonly IContenedorTrabajo _contenedorTrabajo;


		public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender,
            RoleManager<IdentityRole> roleManager,
            ITeamRepository teamRepository,
            IContenedorTrabajo contenedorTrabajo)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            _roleManager = roleManager;
            _teamRepository = teamRepository;
            _contenedorTrabajo = contenedorTrabajo;
		}

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }
        public IEnumerable<SelectListItem> TeamList { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            [Required(ErrorMessage = "El campo correo es obligatorio")]
            [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
            [RegularExpression(@"^[^@]+@queretaro\.tecnm\.mx$",
            ErrorMessage = "Solo se permiten correos con el dominio @queretaro.tecnm.mx")]
            [Display(Name = "Correo")]
            public string Email { get; set; }

			[Required(ErrorMessage = "El nombre es obligatorio.")]
			[StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
            [Display(Name = "Nombre")]
            public string Name { get; set; }

			[Required(ErrorMessage = "El apellido paterno es obligatorio.")]
			[StringLength(50, ErrorMessage = "El apellido paterno no puede exceder los 50 caracteres.")]
            [Display(Name = "Apellido Paterno")]
            public string LastName { get; set; }

			[Required(ErrorMessage = "El apellido materno es obligatorio.")]
			[StringLength(50, ErrorMessage = "El apellido materno no puede exceder los 50 caracteres.")]
            [Display(Name = "Apellido Materno")]
            public string SecondLastName { get; set; }
            [Display(Name = "Equipo")]
            public int? TeamId { get; set; }

        }
		private string GenerateSecurePassword()
		{
			const string uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			const string lowercaseChars = "abcdefghijklmnopqrstuvwxyz";
			const string numericChars = "0123456789";
			const string specialChars = "!@#$%^&*";

			var random = new Random();
			var password = new StringBuilder();

			password.Append(uppercaseChars[random.Next(uppercaseChars.Length)]);
			password.Append(lowercaseChars[random.Next(lowercaseChars.Length)]);
			password.Append(numericChars[random.Next(numericChars.Length)]);
			password.Append(specialChars[random.Next(specialChars.Length)]);

			const string allChars = uppercaseChars + lowercaseChars + numericChars + specialChars;
			while (password.Length < 12)
			{
				password.Append(allChars[random.Next(allChars.Length)]);
			}

			return new string(password.ToString().ToCharArray().OrderBy(x => random.Next()).ToArray());
		}


		public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (User.IsInRole("Coach"))
            {
                var userCoachId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                ApplicationUser userCoach = await _userManager.FindByIdAsync(userCoachId);

                TeamList = _teamRepository.GetListaTeams(userCoach.CoachId);
            }
            else
            {
                TeamList = _teamRepository.GetListaTeams();
            }
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

            var selectedRole = Request.Form["rdUserRole"].ToString();

            if (ModelState.IsValid && !string.IsNullOrEmpty(selectedRole))
            {
                var user = CreateUser();
                var password = GenerateSecurePassword();

				await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
                await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

                // Ensure roles exist
                await EnsureRoleExists(CNT.AdminRole);
                await EnsureRoleExists(CNT.StudentRole);
                await EnsureRoleExists(CNT.CoachRole);

                switch (selectedRole)
                {
                    case CNT.AdminRole:
                        var admin = new Models.Admin()
                        {
                            Name = Input.Name,
                            LastName = Input.LastName,
                            SecondLastName = Input.SecondLastName
                        };
                        user.Admin = admin;
                        break;
                    case CNT.CoachRole:
                        var coach = new Models.Coach()
                        {
                            Name = Input.Name,
                            LastName = Input.LastName,
                            SecondLastName = Input.SecondLastName,
                        };
                        user.Coach = coach;
                        break;
                    case CNT.StudentRole:
                        if (!Input.TeamId.HasValue)
                        {
                            ModelState.AddModelError(string.Empty, "Debe seleccionar un equipo para el estudiante.");
                            if (User.IsInRole("Coach"))
                            {
                                var userCoachId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                                ApplicationUser userCoach = await _userManager.FindByIdAsync(userCoachId);

                                TeamList = _teamRepository.GetListaTeams(userCoach.CoachId);
                            }
                            else
                            {
                                TeamList = _teamRepository.GetListaTeams();
                            }

                            return Page();
                        }

                        var team = _teamRepository.GetFirstOrDefault(t => t.Id == Input.TeamId.Value);
                        if (team == null || team.CoachId == null)
                        {
                            ModelState.AddModelError(string.Empty, "El equipo seleccionado no tiene un entrenador asignado.");
                            TeamList = _teamRepository.GetListaTeams();
                            return Page();
                        }

                        var student = new Models.Student
                        {
                            Name = Input.Name,
                            LastName = Input.LastName,
                            SecondLastName = Input.SecondLastName,
                            Email = Input.Email,
                            TeamId = Input.TeamId.Value,
                            CoachId = team.CoachId,
						};

						user.Student = student;

						break;
                    default:
                        ModelState.AddModelError(string.Empty, "Seleccion de Rol invalida");
                        return Page();
                }

                var result = await _userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, selectedRole);
                    _logger.LogInformation("Nueva cuenta creada");

                    TempData["Message"] = $"Usuario {Input.Email} registrado exitosamente como {selectedRole}";
					TempData["Type"] = "success";

                    if (user.StudentId.HasValue)
                    {
                        CreateUniformStudent(user.Student.Id, user.Student.Team.RepresentativeId);
                    }

					// Configura el token para forzar el cambio de contraseña en el primer inicio de sesión
					var code = await _userManager.GeneratePasswordResetTokenAsync(user);
					var callbackUrl = Url.Page(
						"/Account/ResetPassword",
						pageHandler: null,
						values: new { area = "Identity", code },
						protocol: Request.Scheme);

					var emailTemplate = $@"
                        <!DOCTYPE html>
                        <html>
                        <head>
                            <meta charset='UTF-8'>
                            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                            <title>Bienvenido al Sistema</title>
                            <style>
                                body {{
                                    font-family: Arial, sans-serif;
                                    line-height: 1.6;
                                    color: #333333;
                                    margin: 0;
                                    padding: 0;
                                }}
                                .container {{
                                    max-width: 600px;
                                    margin: 0 auto;
                                    padding: 20px;
                                }}
                                .header {{
                                    background-color: #1a73e8;
                                    color: white;
                                    padding: 20px;
                                    text-align: center;
                                    border-radius: 5px 5px 0 0;
                                }}
                                .content {{
                                    background-color: #ffffff;
                                    padding: 20px;
                                    border: 1px solid #dddddd;
                                    border-radius: 0 0 5px 5px;
                                }}
                                .button {{
                                    display: inline-block;
                                    padding: 12px 24px;
                                    background-color: #1a73e8;
                                    color: white !important;
                                    text-decoration: none;
                                    border-radius: 5px;
                                    margin: 20px 0;
                                }}
                                .footer {{
                                    text-align: center;
                                    margin-top: 20px;
                                    color: #666666;
                                    font-size: 12px;
                                }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='header'>
                                    <h1>Bienvenido al SIIR</h1>
                                </div>
                                <div class='content'>
                                    <p>Hola {Input.Name},</p>
                                    <p>Tu cuenta ha sido creada exitosamente. Aquí están tus credenciales de acceso:</p>
                                    <p><strong>Correo electrónico:</strong> {Input.Email}</p>
                                    <p><strong>Contraseña:</strong> {password}</p>
                                    <div style='text-align: center;'>
                                        <p>Por favor, cambia tu contraseña lo antes posible después de tu primer inicio de sesión.</p>
                                        <p>De no hacerlo, estas comprometiendo la integridad de tu información.</p>
                                        <a href='{Url.Page("/Account/Login", pageHandler: null, values: null, protocol: Request.Scheme)}' class='button'>Iniciar Sesión</a>
                                    </div>
                                    <p>Si no has solicitado esta cuenta, por favor contacta con el Departamento de Extraescolares.</p>
                                </div>
                                <div class='footer'>
                                    <p>Este es un correo electrónico automático, por favor no responda a este mensaje.</p>
                                    <p>© {DateTime.Now.Year} SIIR. Todos los derechos reservados.</p>
                                </div>
                            </div>
                        </body>
                        </html>";

                    await _emailSender.SendEmailAsync(
                        Input.Email,
                        "Bienvenido al Sistema",
                        emailTemplate);

                    // Redirigir al listado de usuarios
                    if (User.IsInRole("Coach"))
                        return RedirectToAction("Index", "Home", new { area = "Coach" });
                    return RedirectToAction("Index", "Users", new { area = "Admin" });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            if (User.IsInRole("Coach"))
            {
                var userCoachId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                ApplicationUser userCoach = await _userManager.FindByIdAsync(userCoachId);

                TeamList = _teamRepository.GetListaTeams(userCoach.CoachId);
            }
            else
            {
                TeamList = _teamRepository.GetListaTeams();
            }
            return Page();
        }

        private void CreateUniformStudent(int studentId, int representativeId) 
        {
			var representativeUniforms = _contenedorTrabajo.RepresentativeUniformCatalog
				.GetAll()
		        .Where(u => u.RepresentativeId == representativeId)
		        .ToList();

            foreach(var representativeUniform in representativeUniforms)
            {
                var uniform = new Uniform();
                uniform.StudentId = studentId;
                uniform.RepresentativeId = representativeId;
                uniform.UniformCatalogId = representativeUniform.UniformCatalogId;
				_contenedorTrabajo.Uniform.Add(uniform);
                _contenedorTrabajo.Save();
			}
		}

        private async Task EnsureRoleExists(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a parameterless constructor, or alternatively " +
                    $"override the register page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }

        private string GetDashboardUrl(string role)
        {
            switch (role)
            {
                case CNT.AdminRole:
                    return "/Admin/Home";
                case CNT.CoachRole:
                    return "/Coach/Home";
                case CNT.StudentRole:
                    return "/Student/Home";
                default:
                    return "/";
            }
        }
    }
}
