﻿using SIIR.Data;
using SIIR.DataAccess.Data.Repository;
using SIIR.DataAccess.Data.Repository.IRepository;
using SIIR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.DataAccess.Data.Repository
{
    public class ContenedorTrabajo : IContenedorTrabajo
    {
        private readonly ApplicationDbContext _db;
        public ContenedorTrabajo(ApplicationDbContext db)
        {
            _db = db;
            Admin = new AdminRepository(_db);
            User = new UserRepository(_db);
            Team = new TeamRepository(_db);
            Representative = new RepresentativeRepository(_db);
            UniformCatalog = new UniformCatalogRepository(_db);
            Coach = new CoachRepository(_db);
            Student = new StudentRepository(_db);
            DocumentCatalog = new DocumentCatalogRepository(_db);
            Document = new DocumentRepository(_db);
            RepresentativeUniformCatalog = new RepresentativeUniformCatalogRepository(_db);
            Uniform = new UniformRepository(_db);
            Notification = new NotificationRepository(_db);
        }

        public IUserRepository User { get; private set; }
        public ITeamRepository Team { get; private set; }
        public IRepresentativeRepository Representative { get; private set; }
        public IUniformCatalogRepository UniformCatalog { get; private set; }
        public ICoachRepository Coach { get; private set; }
        public IStudentRepository Student { get; private set; }
        public IDocumentCatalogRepository DocumentCatalog { get; private set; }
        public IDocumentRepository Document { get; private set; }
        public IRepresentativeUniformCatalog RepresentativeUniformCatalog { get; private set; }
        public IUniformRepository Uniform { get; private set; }

        public IAdminRepository Admin { get; private set; }
        public INotificationRepository Notification { get; private set; }
        public void Dispose()
        {
            _db.Dispose();
        }
        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
