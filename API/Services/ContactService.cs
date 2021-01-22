using Core;
using Core.ViewModels;
using Microsoft.EntityFrameworkCore;
using Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Services
{
    public class ContactService : IContactService
    {
        private readonly DataContext context;

        public ContactService(DataContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<ContactViewModel>> GetAllAsync()
        {
            return await this.context.Contacts.AsNoTracking().Select(c => new ContactViewModel() { Id = c.Id, Name = c.Name, Address = c.Address }).ToListAsync();
        }

        public async Task<ContactViewModel> GetAsync(Guid id)
        {
            var contact = await this.context.Contacts.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id);
            return new ContactViewModel() { Id = contact.Id, Name = contact.Name, Address = contact.Address };
        }

        public async Task<IEnumerable<ContactViewModel>> SearchContactsByNameAsync(string name)
        {
            var contacts = this.context.Contacts.AsNoTracking();

            if (string.IsNullOrEmpty(name))
            {
                return await contacts.Select(c => new ContactViewModel()
                {
                    Id = c.Id,
                    Name = c.Name,
                    Address = c.Address
                }).ToListAsync();
            }

            return await contacts.Where(c => c.Name.Contains(name))
                .Select(c => new ContactViewModel()
                {
                    Id = c.Id,
                    Name = c.Name,
                    Address = c.Address
                }).ToListAsync();
        }

        public async Task<IEnumerable<ContactViewModel>> GetAsync(IEnumerable<Guid> contactIds)
        {
            return await this.context.Contacts.Where(s => contactIds.Contains(s.Id))
                .Select(c => new ContactViewModel()
                {
                    Id = c.Id,
                    Name = c.Name,
                    Address = c.Address
                }).ToListAsync();
        }

        public async Task AddAsync(Contact contact)
        {
            await this.context.AddAsync(contact);
            await this.context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Contact contact)
        {
            // approach #1
            // This apporoach, updates all properties
            //this.context.Update(contact);
            //await this.context.SaveChangesAsync();

            // approach #2
            // Updates changed properties only
            //var existing = await this.context.Contacts.FindAsync(contact.Id);
            //existing.Name = contact.Name;
            //existing.Address = contact.Address;
            //await this.context.SaveChangesAsync();

            // approach #3
            // Updates changed properties only (similar to approach #2)
            var exists = await this.context.Contacts.FindAsync(contact.Id);
            this.context.Entry(exists).CurrentValues.SetValues(contact);
            await this.context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid contactId)
        {
            var contact = await this.context.Contacts.FindAsync(contactId);
            this.context.Contacts.Remove(contact);
            await this.context.SaveChangesAsync();
        }

        public async Task<bool> IsExistingAsync(Guid contactId)
        {
            var contact = await this.context.Contacts.FindAsync(contactId);
            return contact != null;
        }
    }
}
