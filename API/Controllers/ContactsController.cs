using Core;
using Core.ViewModels;
using Infrastructure.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [TypeFilter(typeof(TrackActionPerformanceFilter))]
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
            // scopes allows us to put wrap the logs inside a scope, logs made inside this scope will have a scopeIdentifier
            using (this.logger.BeginScope("Starting operation for request: {scopeIdentifier}}", this.HttpContext.TraceIdentifier))
            {
                logger.LogInformation("Get contacts with filter: {nameFilter}", nameFilter);
                logger.LogWarning("This should only appear if the log level is warning");
                if (string.IsNullOrEmpty(nameFilter))
                {
                    return Ok(await this.service.GetAllAsync());
                }

                return Ok(await this.service.SearchContactsByNameAsync(nameFilter));
            }
        }

        [Route("UseRawSql")]
        [HttpGet()]
        public async Task<ActionResult<IEnumerable<ContactViewModel>>> UseRawSql([FromQuery(Name = "name")] string nameFilter)/*Lets use name as the query key*/
        {
            // scopes allows us to put wrap the logs inside a scope, logs made inside this scope will have a scopeIdentifier
            using (this.logger.BeginScope("Starting operation for request: {scopeIdentifier}}", this.HttpContext.TraceIdentifier))
            {
                logger.LogInformation("Get contacts with filter: {nameFilter}", nameFilter);
                logger.LogWarning("This should only appear if the log level is warning");
                if (string.IsNullOrEmpty(nameFilter))
                {
                    return Ok(await this.service.GetAllUsingSqlQuery());
                }

                return Ok(await this.service.SearchContactsByNameAsync(nameFilter));
            }
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
    }
}
