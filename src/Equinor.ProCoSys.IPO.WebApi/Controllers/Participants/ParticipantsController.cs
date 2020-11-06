using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Equinor.ProCoSys.IPO.Query.GetFunctionalRoles;
using Equinor.ProCoSys.IPO.Query.GetPersons;
using Equinor.ProCoSys.IPO.Query.GetPersonsInUserGroup;
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
        private const string _contractorUserGroup = "CONTRACTOR_PKG_ENG";
        private const string _constructionUserGroup = "CONTRACTOR_PKG_ENG";

        private readonly IMediator _mediator;

        public ParticipantsController(IMediator mediator) => _mediator = mediator;

        /// <summary>
        /// Gets functional roles from ProCoSys library API with IPO classification 
        /// </summary>
        /// <param name="plant"></param>
        /// <returns>All ProCoSys functional roles which have classification IPO</returns>
        [Authorize(Roles = Permissions.LIBRARY_FUNCTIONAL_ROLE_READ)]
        [HttpGet("/FunctionalRoles/ByClassification/IPO")]
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
        [HttpGet("/Persons")]
        public async Task<ActionResult<List<ProCoSysPersonDto>>> GetPersons(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)] [Required]
            string plant,
            string searchString)
        {
            var result = await _mediator.Send(new GetPersonsQuery(searchString));
            return this.FromResult(result);
        }

        /// <summary>
        /// Gets persons from Main API with the user group MC_CONTRACTOR_MLA
        /// </summary>
        /// <param name="plant"></param>
        /// <param name="searchString">Search string (start of first name, last name, or username)</param>
        /// <returns>All persons in ProCoSys whom have access to the user group MC Contr. MCCR/PL/Preserv MLA</returns>
        [Authorize(Roles = Permissions.USER_READ)]
        [HttpGet("/Persons/ByUserGroup/Contractor")]
        public async Task<ActionResult<List<ProCoSysPersonDto>>> GetContractorPersons(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)] [Required]
            string plant,
            string searchString)
        {
            var result = await _mediator.Send(new GetPersonsInUserGroupQuery(searchString, _contractorUserGroup));
            return this.FromResult(result);
        }

        /// <summary>
        /// Gets persons from Main API with the user group MC_LEAD_DISCIPLINE
        /// </summary>
        /// <param name="plant"></param>
        /// <param name="searchString">Search string (start of first name, last name, or username)</param>
        /// <returns>All persons in ProCoSys whom have access to the user group MC Lead & Discipline</returns>
        [Authorize(Roles = Permissions.USER_READ)]
        [HttpGet("/Persons/ByUserGroup/Construction")]
        public async Task<ActionResult<List<ProCoSysPersonDto>>> GetConstructionPersons(
            [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)] [Required]
            string plant,
            string searchString)
        {
            var result = await _mediator.Send(new GetPersonsInUserGroupQuery(searchString, _constructionUserGroup));
            return this.FromResult(result);
        }
    }
}
