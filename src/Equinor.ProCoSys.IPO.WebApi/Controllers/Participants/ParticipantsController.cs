using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Query.GetFunctionalRoles;
using Equinor.ProCoSys.IPO.Query.GetPersons;
using Equinor.ProCoSys.IPO.Query.GetPersonsWithPrivileges;
using Equinor.ProCoSys.IPO.WebApi.Middleware;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceResult.ApiExtensions;

namespace Equinor.ProCoSys.IPO.WebApi.Controllers.Participants
{
    [ApiController]
    [Route("Participants")]
    public class ParticipantsController : ControllerBase
    {
        private const string _classification = "IPO";
        private const string _objectName = "IPO";
        private readonly List<string> _requiredSignerPrivileges = new List<string> { "SIGN", "CREATE" };
        private readonly List<string> _signerPrivileges = new List<string> { "SIGN" };

        private readonly IMediator _mediator;

        public ParticipantsController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Gets functional roles from ProCoSys library API with IPO classification 
        /// </summary>
        /// <param name="plant"></param>
        /// <returns>All ProCoSys functional roles which have classification IPO</returns>
        [Authorize(Roles = Permissions.LIBRARY_FUNCTIONAL_ROLE_READ)]
        [HttpGet("FunctionalRoles/ByClassification/IPO")]
        public async Task<ActionResult<List<ProCoSysFunctionalRoleDto>>> GetFunctionalRolesForIpo(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)] [Required]
            string plant)
        {
            var result = await _mediator.Send(new GetFunctionalRolesForIpoQuery(_classification));
            return this.FromResult(result);
        }

        /// <summary>
        /// Gets persons from ProCoSys main API
        /// </summary>
        /// <param name="plant"></param>
        /// <param name="searchString">Search string (start of first name, last name, or username)</param>
        /// <returns>All ProCoSys persons in specified plant</returns>
        [Authorize(Roles = Permissions.USER_READ)]
        [HttpGet("Persons")]
        public async Task<ActionResult<List<ProCoSysPersonDto>>> GetPersons(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)] [Required]
            string plant,
            string searchString)
        {
            var result = await _mediator.Send(new GetPersonsQuery(searchString));
            return this.FromResult(result);
        }

        /// <summary>
        /// Gets persons from Main API with the privilege IPO SIGN and CREATE
        /// </summary>
        /// <param name="plant"></param>
        /// <param name="searchString">Search string (start of first name, last name, or username)</param>
        /// <returns>All persons in ProCoSys with privilege IPO SIGN and CREATE</returns>
        [Authorize(Roles = Permissions.USER_READ)]
        [HttpGet("Persons/ByPrivileges/RequiredSigners")] //TODO: remove when not in use by UI anymore!
        public async Task<ActionResult<List<ProCoSysPersonDto>>> GetRequiredSignerPersons(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)] [Required]
            string plant,
            string searchString)
        {
            var result = await _mediator.Send(new GetPersonsWithPrivilegesQuery(searchString, _objectName, _requiredSignerPrivileges));
            return this.FromResult(result);
        }

        /// <summary>
        /// Gets persons from Main API with the privilege IPO SIGN
        /// </summary>
        /// <param name="plant"></param>
        /// <param name="searchString">Search string (start of first name, last name, or username)</param>
        /// <returns>All persons in ProCoSys with privilege IPO SIGN</returns>
        [Authorize(Roles = Permissions.USER_READ)]
        [HttpGet("Persons/ByPrivileges/AdditionalSigners")]//TODO: remove when not in use by UI anymore!
        public async Task<ActionResult<List<ProCoSysPersonDto>>> GetAdditionalSignerPersons(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)] [Required]
            string plant,
            string searchString)
        {
            var result = await _mediator.Send(new GetPersonsWithPrivilegesQuery(searchString, _objectName, _signerPrivileges));
            return this.FromResult(result);
        }

        /// <summary>
        /// Gets persons from Main API with the privilege IPO SIGN
        /// </summary>
        /// <param name="plant"></param>
        /// <param name="searchString">Search string (start of first name, last name, or username)</param>
        /// <returns>All persons in ProCoSys with privilege IPO SIGN</returns>
        [Authorize(Roles = Permissions.USER_READ)]
        [HttpGet("Persons/ByPrivileges/Signers")]
        public async Task<ActionResult<List<ProCoSysPersonDto>>> GetSignerPersons(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)] [Required]
            string plant,
            string searchString)
        {
            var result = await _mediator.Send(new GetPersonsWithPrivilegesQuery(searchString, _objectName, _signerPrivileges));
            return this.FromResult(result);
        }
    }
}
