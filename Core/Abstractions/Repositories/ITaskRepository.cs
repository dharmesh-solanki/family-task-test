using Domain.DataModels;
using System;
using System.Collections.Generic;
using System.Text;
using Core.Abstractions.Repositories;
using Domain.Commands;
using System.Threading.Tasks;

namespace Core.Abstractions.Repositories
{
    public interface ITaskRepository : IBaseRepository<Guid, Domain.DataModels.Task, ITaskRepository>
    {
    }
}
