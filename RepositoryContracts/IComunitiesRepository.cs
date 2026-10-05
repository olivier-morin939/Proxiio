using Entities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace RepositoryContracts
{
    public interface IComunitiesRepository
    {

        Task<List<Comunity>> GetAllComunities();
        Task<List<Comunity>> GetFilteredComunities(Expression<Func<Comunity, bool>> predicate);
        Task<bool> UserExistsInComunity(Guid userId, Guid comunityId);

        Task<int> GetAllComunitiesCount();
        Task<int> GetAllComunityPostsCount(Guid comId);

        Task<Comunity> GetComunityById(Guid comunityId);

        Task<Comunity> AddComunity(Comunity comunity, Guid creatorId);

        Task<Comunity> UpdateComunity(Comunity comunity);

        Task<bool> DeleteComunity(Guid comunityId);
    }
}
