using FinanceSystem.API.Extensions;
using FinanceSystem.Application.DTOs.Financing;
using FinanceSystem.Application.Interfaces;
using FinanceSystem.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceSystem.API.Controllers
{
    [Route("api/financing-corrections")]
    [ApiController]
    [Authorize]
    public class FinancingCorrectionsController : ControllerBase
    {
        private readonly IFinancingCorrectionService _correctionService;
        private readonly IFinancingService _financingService;
        private readonly ICorrectionIndexService _indexService;

        public FinancingCorrectionsController(
            IFinancingCorrectionService correctionService,
            IFinancingService financingService,
            ICorrectionIndexService indexService)
        {
            _correctionService = correctionService;
            _financingService = financingService;
            _indexService = indexService;
        }

        [HttpGet("financing/{financingId}")]
        public async Task<ActionResult<IEnumerable<FinancingCorrectionDto>>> GetByFinancingId(Guid financingId)
        {
            try
            {
                var financing = await _financingService.GetByIdAsync(financingId);
                if (financing.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                var corrections = await _correctionService.GetByFinancingIdAsync(financingId);
                return Ok(corrections);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("financing/{financingId}/date-range")]
        public async Task<ActionResult<IEnumerable<FinancingCorrectionDto>>> GetByDateRange(
            Guid financingId,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            try
            {
                var financing = await _financingService.GetByIdAsync(financingId);
                if (financing.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                var corrections = await _correctionService.GetByDateRangeAsync(financingId, startDate, endDate);
                return Ok(corrections);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FinancingCorrectionDto>> GetById(Guid id)
        {
            try
            {
                var correction = await _correctionService.GetByIdAsync(id);
                var financing = await _financingService.GetByIdAsync(correction.FinancingId);

                if (financing.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                return Ok(correction);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPost("apply")]
        public async Task<ActionResult<FinancingCorrectionDto>> ApplyCorrection(ApplyCorrectionDto correctionDto)
        {
            try
            {
                var financing = await _financingService.GetByIdAsync(correctionDto.FinancingId);
                if (financing.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                var correction = await _correctionService.ApplyCorrectionAsync(correctionDto);
                return Ok(correction);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("financing/{financingId}/impact")]
        public async Task<ActionResult<decimal>> GetTotalImpact(Guid financingId)
        {
            try
            {
                var financing = await _financingService.GetByIdAsync(financingId);
                if (financing.UserId != HttpContext.GetCurrentUserId())
                    return Forbid();

                var impact = await _correctionService.GetTotalCorrectionImpactAsync(financingId);
                return Ok(impact);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpGet("indices/current/{indexType}")]
        public async Task<ActionResult<decimal>> GetCurrentIndex(CorrectionIndexType indexType)
        {
            var currentValue = await _indexService.GetCurrentIndexValueAsync(indexType);
            return Ok(currentValue);
        }

        [HttpGet("indices/historical/{indexType}")]
        public async Task<ActionResult<decimal>> GetHistoricalIndex(
            CorrectionIndexType indexType,
            [FromQuery] DateTime date)
        {
            var historicalValue = await _indexService.GetHistoricalIndexValueAsync(indexType, date);
            return Ok(historicalValue);
        }

        [HttpGet("indices/series/{indexType}")]
        public async Task<ActionResult<IEnumerable<KeyValuePair<DateTime, decimal>>>> GetHistoricalSeries(
            CorrectionIndexType indexType,
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate)
        {
            var series = await _indexService.GetHistoricalSeriesAsync(indexType, startDate, endDate);
            return Ok(series);
        }

        [HttpGet("indices/projected/{indexType}/{months}")]
        public async Task<ActionResult<decimal>> GetProjectedIndex(CorrectionIndexType indexType, int months)
        {
            var projectedValue = await _indexService.GetProjectedIndexValueAsync(indexType, months);
            return Ok(projectedValue);
        }
    }
}