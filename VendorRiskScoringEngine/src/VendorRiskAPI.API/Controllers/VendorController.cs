using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using VendorRiskAPI.Application.DTOs.Request;
using VendorRiskAPI.Application.DTOs.Response;
using VendorRiskAPI.Application.Interfaces;
using VendorRiskAPI.Domain.Entities;

namespace VendorRiskAPI.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VendorController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRiskScoringService _riskScoringService;
    private readonly IMapper _mapper;
    private readonly ILogger<VendorController> _logger;

    public VendorController(
        IUnitOfWork unitOfWork,
        IRiskScoringService riskScoringService,
        IMapper mapper,
        ILogger<VendorController> logger)
    {
        _unitOfWork = unitOfWork;
        _riskScoringService = riskScoringService;
        _mapper = mapper;
        _logger = logger;
    }

    /// <summary>
    /// Create a new vendor
    /// </summary>
    /// <param name="request">Vendor creation request</param>
    /// <returns>Created vendor</returns>
    /// <response code="201">Vendor created successfully</response>
    /// <response code="400">Invalid request data</response>
    [HttpPost]
    [ProducesResponseType(typeof(VendorResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VendorResponse>> CreateVendor([FromBody] CreateVendorRequest request)
    {
        _logger.LogInformation("Creating new vendor: {VendorName}", request.Name);

        var vendor = _mapper.Map<VendorProfile>(request);
        await _unitOfWork.VendorProfiles.AddAsync(vendor);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Vendor created successfully with ID: {VendorId}", vendor.Id);

        var response = _mapper.Map<VendorResponse>(vendor);
        return CreatedAtAction(nameof(GetVendorById), new { id = vendor.Id }, response);
    }

    /// <summary>
    /// Get vendor by ID
    /// </summary>
    /// <param name="id">Vendor ID</param>
    /// <returns>Vendor details</returns>
    /// <response code="200">Vendor found</response>
    /// <response code="404">Vendor not found</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(VendorResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VendorResponse>> GetVendorById(int id)
    {
        _logger.LogInformation("Retrieving vendor with ID: {VendorId}", id);

        var vendor = await _unitOfWork.VendorProfiles.GetByIdAsync(id);
        if (vendor == null)
        {
            _logger.LogWarning("Vendor not found with ID: {VendorId}", id);
            return NotFound(new { Message = $"Vendor with ID {id} not found" });
        }

        var response = _mapper.Map<VendorResponse>(vendor);
        return Ok(response);
    }

    /// <summary>
    /// Get all vendors with pagination
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 10, max: 100)</param>
    /// <returns>Paginated list of vendors</returns>
    /// <response code="200">Vendors retrieved successfully</response>
    /// <remarks>
    /// Response headers:
    /// - X-Total-Count: Total number of vendors
    /// - X-Page: Current page number
    /// - X-Page-Size: Items per page
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VendorResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VendorResponse>>> GetAllVendors(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Retrieving vendors - Page: {Page}, PageSize: {PageSize}", page, pageSize);

        var allVendors = await _unitOfWork.VendorProfiles.GetAllAsync();
        var paginatedVendors = allVendors
            .OrderByDescending(v => v.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var response = _mapper.Map<IEnumerable<VendorResponse>>(paginatedVendors);

        Response.Headers.Add("X-Total-Count", allVendors.Count().ToString());
        Response.Headers.Add("X-Page", page.ToString());
        Response.Headers.Add("X-Page-Size", pageSize.ToString());

        return Ok(response);
    }

    /// <summary>
    /// Calculate and get risk assessment for a vendor
    /// </summary>
    /// <param name="id">Vendor ID</param>
    /// <returns>Risk assessment with scores and explanation</returns>
    /// <response code="200">Risk assessment calculated successfully</response>
    /// <response code="404">Vendor not found</response>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /api/vendor/1/risk
    ///
    /// Risk calculation formula:
    /// - Financial Risk: Based on financial health (0-100 scale)
    /// - Operational Risk: Based on SLA uptime and incidents
    /// - Security/Compliance Risk: Based on certifications and documents
    /// - Final Score: (Financial × 0.4) + (Operational × 0.3) + (Security × 0.3)
    /// </remarks>
    [HttpGet("{id}/risk")]
    [ProducesResponseType(typeof(RiskAssessmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RiskAssessmentResponse>> GetVendorRiskAssessment(int id)
    {
        _logger.LogInformation("Calculating risk assessment for vendor ID: {VendorId}", id);

        var vendor = await _unitOfWork.VendorProfiles.GetByIdAsync(id);
        if (vendor == null)
        {
            _logger.LogWarning("Vendor not found with ID: {VendorId}", id);
            return NotFound(new { Message = $"Vendor with ID {id} not found" });
        }

        // Calculate risk assessment
        var assessment = await _riskScoringService.CalculateRiskScoreAsync(vendor);
        
        // Save the assessment
        await _unitOfWork.RiskAssessments.AddAsync(assessment);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Risk assessment completed for vendor ID: {VendorId}, Risk Level: {RiskLevel}, Score: {Score}",
            id, assessment.RiskLevel, assessment.FinalRiskScore);

        var response = _mapper.Map<RiskAssessmentResponse>(assessment);
        return Ok(response);
    }

    /// <summary>
    /// Delete a vendor
    /// </summary>
    /// <param name="id">Vendor ID to delete</param>
    /// <returns>No content</returns>
    /// <response code="204">Vendor deleted successfully</response>
    /// <response code="404">Vendor not found</response>
    /// <remarks>
    /// This will also delete all associated risk assessments due to cascade delete.
    /// </remarks>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVendor(int id)
    {
        _logger.LogInformation("Deleting vendor with ID: {VendorId}", id);

        var vendor = await _unitOfWork.VendorProfiles.GetByIdAsync(id);
        if (vendor == null)
        {
            _logger.LogWarning("Vendor not found with ID: {VendorId}", id);
            return NotFound(new { Message = $"Vendor with ID {id} not found" });
        }

        _unitOfWork.VendorProfiles.Remove(vendor);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Vendor deleted successfully with ID: {VendorId}", id);

        return NoContent();
    }
}
