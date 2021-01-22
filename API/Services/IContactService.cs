using Core;
using Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Services
{
    public interface IContactService
    {
        Task<IEnumerable<ContactViewModel>> GetAllAsync();

        Task<ContactViewModel> GetAsync(Guid id);

        Task<IEnumerable<ContactViewModel>> SearchContactsByNameAsync(string name);

        Task<IEnumerable<ContactViewModel>> GetAsync(IEnumerable<Guid> contactIds);

        Task AddAsync(Contact contact);

        Task UpdateAsync(Contact contact);

        Task DeleteAsync(Guid contactId);

        Task<bool> IsExistingAsync(Guid contactId);
    }
}