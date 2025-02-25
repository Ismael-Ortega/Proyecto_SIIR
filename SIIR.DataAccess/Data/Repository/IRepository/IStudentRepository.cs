﻿using Microsoft.AspNetCore.Mvc.Rendering;
using SIIR.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.DataAccess.Data.Repository.IRepository
{
    public interface IStudentRepository: IRepository<Student>
    {
        void Update(Student student);
        IEnumerable<SelectListItem> GetStudentsList();
        void AssignCaptain(Student student);
        void UnassignCaptain(Student student);
    }
}
