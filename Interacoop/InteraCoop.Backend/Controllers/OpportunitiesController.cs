﻿using InteraCoop.Backend.UnitsOfWork.Implementations;
using InteraCoop.Backend.UnitsOfWork.Interfaces;
using InteraCoop.Shared.Dtos;
using InteraCoop.Shared.Entities;
using Microsoft.AspNetCore.Mvc;

namespace InteraCoop.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OpportunitiesController : GenericController<Opportunity>
    {
        private readonly IOpportunitiesUnitOfWork _opportunitiesUnitOfWork;

        public OpportunitiesController(IGenericUnitOfWork<Opportunity> unitOfWork, IOpportunitiesUnitOfWork opportunitiesUnitOfWork) : base(unitOfWork)
        {
            _opportunitiesUnitOfWork = opportunitiesUnitOfWork;
        }

        [HttpDelete("{id}")]
        public override async Task<IActionResult> DeleteAsync(int id)
        {
            var action = await _opportunitiesUnitOfWork.DeleteAsync(id);
            if (!action.WasSuccess)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpGet]
        public override async Task<IActionResult> GetAsync([FromQuery] PaginationDTO pagination)
        {
            var response = await _opportunitiesUnitOfWork.GetAsync(pagination);
            if (response.WasSuccess)
            {
                return Ok(response.Result);
            }
            return BadRequest();
        }

        [HttpGet("totalPages")]
        public override async Task<IActionResult> GetPagesAsync([FromQuery] PaginationDTO pagination){
            var action = await _opportunitiesUnitOfWork.GetTotalPagesAsync(pagination);
            if (action.WasSuccess)
            {
                return Ok(action.Result);
            }
            return BadRequest();
        }

        [HttpGet("{id}")]
        public override async Task<IActionResult> GetAsync(int id)
        {
            var action = await _opportunitiesUnitOfWork.GetAsync(id);
            if (action.WasSuccess)
            {
                return Ok(action.Result);
            }
            return NotFound(action.Message);
        }

        [HttpPost("new")]
        public async Task<IActionResult> PostAsync(OpportunityDto opportunityDto)
        {
            var action = await _opportunitiesUnitOfWork.AddAsync(opportunityDto); 
            if (action.WasSuccess)
            {
                return Ok(action.Result);
            }
            return NotFound(action.Message);
        }

        [HttpPut("Update")]
        public async Task<IActionResult> UpdateAsync(OpportunityDto opportunityDto)
        {
            var action = await _opportunitiesUnitOfWork.UpdateAsync(opportunityDto);
            if (action.WasSuccess)
            {
                return Ok(action.Result);
            }
            return NotFound(action.Message);
        }
    }
}