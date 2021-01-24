﻿using Core;
using Core.ViewModels;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : ControllerBase
    {
        private readonly IContactService service;
        private readonly ILogger<ContactsController> logger;

        public ContactsController(IContactService service, ILogger<ContactsController> logger)
        {
            this.service = service;
            this.logger = logger;
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<ContactViewModel>>> GetContacts([FromQuery(Name = "name")] string nameFilter)/*Lets use name as the query key*/
        {
            logger.LogInformation("Get contacts with filter: {nameFilter}", nameFilter);
            logger.LogWarning("This should only appear if the log level is warning");
            if (string.IsNullOrEmpty(nameFilter))
            {
                return Ok(await this.service.GetAllAsync());
            }

            return Ok(await this.service.SearchContactsByNameAsync(nameFilter));
        }

        [Route("UseRawSql")]
        [HttpGet()]
        public async Task<ActionResult<IEnumerable<ContactViewModel>>> UseRawSql([FromQuery(Name = "name")] string nameFilter)/*Lets use name as the query key*/
        {
            logger.LogInformation("Get contacts with filter: {nameFilter}", nameFilter);
            logger.LogWarning("This should only appear if the log level is warning");
            if (string.IsNullOrEmpty(nameFilter))
            {
                return Ok(await this.service.GetAllUsingSqlQuery());
            }

            return Ok(await this.service.SearchContactsByNameAsync(nameFilter));
        }

        // api/contacts/broken1
        [Route("Broken1")]
        [HttpGet]
        public IActionResult BrokenAction1()
        {
            try
            {
                throw new Exception("Oops something went wrong");
            }
            catch (Exception ex)
            {

                this.logger.LogError(ex.Message);
            }
            
            return Ok("We handled the error ");
        }

        // api/contacts/broken2
        [Route("Broken2")]
        [HttpGet]
        public IActionResult BrokenAction2()
        {
            // this will be logged
            throw new Exception("Unhandled exception");
        }

        // api/contacts/broken2
        [Route("SqlException")]
        [HttpGet]
        public IActionResult SqlException()
        {
            // this will be logged
            this.service.ThrowSqlError();
            return Ok();
        }

        [Route(nameof(ContactsController.GetContactsLongProcess))]
        [HttpGet()]
        public async Task<ActionResult<IEnumerable<ContactViewModel>>> GetContactsLongProcess([FromQuery(Name = "name")] string nameFilter)/*Lets use name as the query key*/
        {
            await Task.Delay(32000);
            if (string.IsNullOrEmpty(nameFilter))
            {
                return Ok(await this.service.GetAllAsync());
            }

            return Ok(await this.service.SearchContactsByNameAsync(nameFilter));
        }

        //[HttpGet("{contactId}", Name = "[controller]." + nameof(ContactsController.GetContactById))]
        [HttpGet("{contactId}", Name = nameof(ContactsController.GetContactById))]
        public async Task<ActionResult<ContactViewModel>> GetContactById([FromRoute] Guid contactId)
        {
            if (!await this.service.IsExistingAsync(contactId))
            {
                return NotFound();
            }

            return Ok(await this.service.GetAsync(contactId));
        }

        [HttpGet("({contactIds})", Name = nameof(ContactsController.GetContactsById))]
        public async Task<ActionResult<ContactViewModel>> GetContactsById(
           [FromRoute][ModelBinder(BinderType = typeof(ModelBinders.ArrayModelBinder))] IEnumerable<Guid> contactIds)
        {

            return Ok(await this.service.GetAsync(contactIds));
        }

        [HttpPost()]
        public async Task<ActionResult<ContactViewModel>> CreateContact(Contact model)
        {
            var contact = new Contact() { Name = model.Name, Address = model.Address };
            await this.service.AddAsync(contact);
            return CreatedAtRoute(nameof(ContactsController.GetContactById), new { contactId = contact.Id },
                new ContactViewModel()
                {
                    Id = contact.Id,
                    Name = contact.Name,
                    Address = contact.Address
                });
        }

        [HttpPut("{contactId}")]
        public async Task<ActionResult<Contact>> UpdateOrInsertContact([FromRoute] Guid contactId,
           UpdateContactViewModel model)
        {
            // If we do not want to Upsert functionality then return NotFound
            //if (!await this.service.IsExistingAsync(contactId))
            //{
            //    return NotFound();
            //}

            if (!await this.service.IsExistingAsync(contactId))
            {
                // Create
                var newContact = new Contact()
                {
                    Id = contactId,
                    Address = model.Address,
                    Name = model.Name
                };

                await this.service.AddAsync(newContact);
                return CreatedAtRoute(nameof(ContactsController.GetContactById), new { contactId = newContact.Id },
                    new ContactViewModel()
                    {
                        Id = newContact.Id,
                        Name = newContact.Name,
                        Address = newContact.Address
                    });
            }
            else
            {
                // Update
                var contact = new Contact() { Id = contactId, Name = model.Name, Address = model.Address };
                await this.service.UpdateAsync(contact);
                return NoContent();
            }
        }

        [HttpPatch("{contactId}")]
        public async Task<ActionResult<ContactViewModel>> PatchOrInsertContact([FromRoute] Guid contactId,
       JsonPatchDocument<UpdateContactViewModel> patchDocument)
        {
            // If we do not want to Upsert functionality then return NotFound
            //if (!await this.service.IsExistingAsync(contactId))
            //{
            //    return NotFound();
            //}

            if (!await this.service.IsExistingAsync(contactId))
            {
                // insert
                var updateViewModel = new UpdateContactViewModel();
                patchDocument.ApplyTo(updateViewModel, ModelState); // to get the correct ModelState, install Microsoft.AspNetCore.Mvc.NewtonsoftJson

                if (!TryValidateModel(updateViewModel))
                {
                    return ValidationProblem(ModelState);
                }

                var newContact = new Contact()
                {
                    Id = contactId,
                    Name = updateViewModel.Name,
                    Address = updateViewModel.Address
                };
                await this.service.AddAsync(newContact);

                var viewModel = new ContactViewModel()
                {
                    Id = newContact.Id,
                    Name = newContact.Name,
                    Address = newContact.Address
                };

                return CreatedAtRoute(nameof(ContactsController.GetContactById),
                    new { contactId = viewModel.Id }, viewModel);
            }
            else
            {
                var contact = await this.service.GetAsync(contactId);
                var updateViewModel = new UpdateContactViewModel()
                {
                    Name = contact.Name,
                    Address = contact.Address
                };

                patchDocument.ApplyTo(updateViewModel, ModelState);

                if (!TryValidateModel(updateViewModel))
                {
                    return ValidationProblem(ModelState);
                }

                var updatedContact = new Contact()
                {
                    Id = contactId,
                    Name = updateViewModel.Name,
                    Address = updateViewModel.Address
                };

                await this.service.UpdateAsync(updatedContact);

                return NoContent();
            }

        }

        [HttpDelete("{contactId}", Name = nameof(ContactsController.DeleteContact))]
        public async Task<IActionResult> DeleteContact(Guid contactId)
        {
            //check if contact exists
            if (!await this.service.IsExistingAsync(contactId))
            {
                return NotFound();
            }

            await this.service.DeleteAsync(contactId);
            return NoContent();
        }
    }
}
