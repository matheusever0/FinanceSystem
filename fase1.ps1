# filter-system-phase5-installments.ps1

$projectRoot = $PWD
$srcRoot = Join-Path $projectRoot "src"

# Create payment installment filter
$paymentInstallmentFilterDir = Join-Path $srcRoot "Equilibrium.Application\DTOs\Filters\PaymentInstallment"
New-Item -ItemType Directory -Force -Path $paymentInstallmentFilterDir

$paymentInstallmentFilterContent = @'
using System;
using Equilibrium.Domain.Enums;

namespace Equilibrium.Application.DTOs.Filters
{
    public class PaymentInstallmentFilter : BaseFilter
    {
        public int? InstallmentNumber { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public DateTime? PaymentDateFrom { get; set; }
        public DateTime? PaymentDateTo { get; set; }
        public PaymentStatus? Status { get; set; }
        public Guid? PaymentId { get; set; }
    }
}
'@

Set-Content -Path (Join-Path $paymentInstallmentFilterDir "PaymentInstallmentFilter.cs") -Value $paymentInstallmentFilterContent

# Create income installment filter
$incomeInstallmentFilterDir = Join-Path $srcRoot "Equilibrium.Application\DTOs\Filters\IncomeInstallment"
New-Item -ItemType Directory -Force -Path $incomeInstallmentFilterDir

$incomeInstallmentFilterContent = @'
using System;
using Equilibrium.Domain.Enums;

namespace Equilibrium.Application.DTOs.Filters
{
    public class IncomeInstallmentFilter : BaseFilter
    {
        public int? InstallmentNumber { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? DueDateFrom { get; set; }
        public DateTime? DueDateTo { get; set; }
        public DateTime? ReceivedDateFrom { get; set; }
        public DateTime? ReceivedDateTo { get; set; }
        public IncomeStatus? Status { get; set; }
        public Guid? IncomeId { get; set; }
    }
}
'@

Set-Content -Path (Join-Path $incomeInstallmentFilterDir "IncomeInstallmentFilter.cs") -Value $incomeInstallmentFilterContent

# Create validators
$validationDir = Join-Path $srcRoot "Equilibrium.Application\Validations\Filters"

$paymentInstallmentValidatorContent = @'
using FluentValidation;
using Equilibrium.Application.DTOs.Filters;

namespace Equilibrium.Application.Validations.Filters
{
    public class PaymentInstallmentFilterValidator : AbstractValidator<PaymentInstallmentFilter>
    {
        public PaymentInstallmentFilterValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
            
            RuleFor(x => x.MinAmount)
                .LessThanOrEqualTo(x => x.MaxAmount)
                .When(x => x.MinAmount.HasValue && x.MaxAmount.HasValue);
                
            RuleFor(x => x.DueDateFrom)
                .LessThanOrEqualTo(x => x.DueDateTo)
                .When(x => x.DueDateFrom.HasValue && x.DueDateTo.HasValue);
                
            RuleFor(x => x.PaymentDateFrom)
                .LessThanOrEqualTo(x => x.PaymentDateTo)
                .When(x => x.PaymentDateFrom.HasValue && x.PaymentDateTo.HasValue);
        }
    }
}
'@

Set-Content -Path (Join-Path $validationDir "PaymentInstallmentFilterValidator.cs") -Value $paymentInstallmentValidatorContent

$incomeInstallmentValidatorContent = @'
using FluentValidation;
using Equilibrium.Application.DTOs.Filters;

namespace Equilibrium.Application.Validations.Filters
{
    public class IncomeInstallmentFilterValidator : AbstractValidator<IncomeInstallmentFilter>
    {
        public IncomeInstallmentFilterValidator()
        {
            RuleFor(x => x.PageNumber).GreaterThan(0);
            RuleFor(x => x.PageSize).InclusiveBetween(1, 100);
            
            RuleFor(x => x.MinAmount)
                .LessThanOrEqualTo(x => x.MaxAmount)
                .When(x => x.MinAmount.HasValue && x.MaxAmount.HasValue);
                
            RuleFor(x => x.DueDateFrom)
                .LessThanOrEqualTo(x => x.DueDateTo)
                .When(x => x.DueDateFrom.HasValue && x.DueDateTo.HasValue);
                
            RuleFor(x => x.ReceivedDateFrom)
                .LessThanOrEqualTo(x => x.ReceivedDateTo)
                .When(x => x.ReceivedDateFrom.HasValue && x.ReceivedDateTo.HasValue);
        }
    }
}
'@

Set-Content -Path (Join-Path $validationDir "IncomeInstallmentFilterValidator.cs") -Value $incomeInstallmentValidatorContent

# Create specifications
$specDir = Join-Path $srcRoot "Equilibrium.Domain\Specifications"

# Payment Installment Specification
$paymentInstallmentSpecDir = Join-Path $specDir "PaymentInstallment"
New-Item -ItemType Directory -Force -Path $paymentInstallmentSpecDir

$paymentInstallmentSpecContent = @'
using System;
using System.Linq.Expressions;
using Equilibrium.Application.DTOs.Filters;

namespace Equilibrium.Domain.Specifications
{
    public class PaymentInstallmentSpecification : BaseSpecification<PaymentInstallment>
    {
        public PaymentInstallmentSpecification(PaymentInstallmentFilter filter)
        {
            AddInclude(pi => pi.Payment);
            
            if (filter.InstallmentNumber.HasValue)
                ApplyCriteria(pi => pi.InstallmentNumber == filter.InstallmentNumber.Value);
                
            if (filter.MinAmount.HasValue)
                ApplyCriteria(pi => pi.Amount >= filter.MinAmount.Value);
                
            if (filter.MaxAmount.HasValue)
                ApplyCriteria(pi => pi.Amount <= filter.MaxAmount.Value);
                
            if (filter.DueDateFrom.HasValue)
                ApplyCriteria(pi => pi.DueDate >= filter.DueDateFrom.Value);
                
            if (filter.DueDateTo.HasValue)
                ApplyCriteria(pi => pi.DueDate <= filter.DueDateTo.Value);
                
            if (filter.PaymentDateFrom.HasValue)
                ApplyCriteria(pi => pi.PaymentDate.HasValue && pi.PaymentDate >= filter.PaymentDateFrom.Value);
                
            if (filter.PaymentDateTo.HasValue)
                ApplyCriteria(pi => pi.PaymentDate.HasValue && pi.PaymentDate <= filter.PaymentDateTo.Value);
                
            if (filter.Status.HasValue)
                ApplyCriteria(pi => pi.Status == filter.Status.Value);
                
            if (filter.PaymentId.HasValue)
                ApplyCriteria(pi => pi.PaymentId == filter.PaymentId.Value);
                
            int skip = (filter.PageNumber - 1) * filter.PageSize;
            ApplyPaging(skip, filter.PageSize);
            
            if (filter.Ascending)
                ApplyOrderBy(GetSortProperty(filter.OrderBy));
            else
                ApplyOrderByDescending(GetSortProperty(filter.OrderBy));
        }
        
        private Expression<Func<PaymentInstallment, object>> GetSortProperty(string propertyName)
        {
            return propertyName?.ToLower() switch
            {
                "installmentnumber" => pi => pi.InstallmentNumber,
                "amount" => pi => pi.Amount,
                "duedate" => pi => pi.DueDate,
                "paymentdate" => pi => pi.PaymentDate,
                "status" => pi => pi.Status,
                _ => pi => pi.DueDate
            };
        }
    }
}
'@

Set-Content -Path (Join-Path $paymentInstallmentSpecDir "PaymentInstallmentSpecification.cs") -Value $paymentInstallmentSpecContent

# Income Installment Specification
$incomeInstallmentSpecDir = Join-Path $specDir "IncomeInstallment"
New-Item -ItemType Directory -Force -Path $incomeInstallmentSpecDir

$incomeInstallmentSpecContent = @'
using System;
using System.Linq.Expressions;
using Equilibrium.Application.DTOs.Filters;
using Equilibrium.Domain.Entities;

namespace Equilibrium.Domain.Specifications
{
    public class IncomeInstallmentSpecification : BaseSpecification<IncomeInstallment>
    {
        public IncomeInstallmentSpecification(IncomeInstallmentFilter filter)
        {
            AddInclude(ii => ii.Income);
            
            if (filter.InstallmentNumber.HasValue)
                ApplyCriteria(ii => ii.InstallmentNumber == filter.InstallmentNumber.Value);
                
            if (filter.MinAmount.HasValue)
                ApplyCriteria(ii => ii.Amount >= filter.MinAmount.Value);
                
            if (filter.MaxAmount.HasValue)
                ApplyCriteria(ii => ii.Amount <= filter.MaxAmount.Value);
                
            if (filter.DueDateFrom.HasValue)
                ApplyCriteria(ii => ii.DueDate >= filter.DueDateFrom.Value);
                
            if (filter.DueDateTo.HasValue)
                ApplyCriteria(ii => ii.DueDate <= filter.DueDateTo.Value);
                
            if (filter.ReceivedDateFrom.HasValue)
                ApplyCriteria(ii => ii.ReceivedDate.HasValue && ii.ReceivedDate >= filter.ReceivedDateFrom.Value);
                
            if (filter.ReceivedDateTo.HasValue)
                ApplyCriteria(ii => ii.ReceivedDate.HasValue && ii.ReceivedDate <= filter.ReceivedDateTo.Value);
                
            if (filter.Status.HasValue)
                ApplyCriteria(ii => ii.Status == filter.Status.Value);
                
            if (filter.IncomeId.HasValue)
                ApplyCriteria(ii => ii.IncomeId == filter.IncomeId.Value);
                
            int skip = (filter.PageNumber - 1) * filter.PageSize;
            ApplyPaging(skip, filter.PageSize);
            
            if (filter.Ascending)
                ApplyOrderBy(GetSortProperty(filter.OrderBy));
            else
                ApplyOrderByDescending(GetSortProperty(filter.OrderBy));
        }
        
        private Expression<Func<IncomeInstallment, object>> GetSortProperty(string propertyName)
        {
            return propertyName?.ToLower() switch
            {
                "installmentnumber" => ii => ii.InstallmentNumber,
                "amount" => ii => ii.Amount,
                "duedate" => ii => ii.DueDate,
                "receiveddate" => ii => ii.ReceivedDate,
                "status" => ii => ii.Status,
                _ => ii => ii.DueDate
            };
        }
    }
}
'@

Set-Content -Path (Join-Path $incomeInstallmentSpecDir "IncomeInstallmentSpecification.cs") -Value $incomeInstallmentSpecContent

# Update services and controllers
function Update-InstallmentService {
    param(
        [string]$entityName,
        [string]$servicePath,
        [string]$serviceImplPath
    )
    
    if (Test-Path $servicePath) {
        $serviceContent = Get-Content $servicePath -Raw
        
        if (-not ($serviceContent -match "GetFilteredAsync")) {
            # Add using statements
            $namespaceMatch = "namespace Equilibrium.Application.Interfaces"
            $newUsings = "using Equilibrium.Application.DTOs.Common;`r`nusing Equilibrium.Application.DTOs.Filters;`r`n`r`n$namespaceMatch"
            $updatedContent = $serviceContent -replace [regex]::Escape($namespaceMatch), $newUsings
            
            # Add new method
            $lastMethodPattern = "Task CancelAsync\(Guid id\);"
            $newMethod = "Task CancelAsync(Guid id);`r`n    Task<PagedResult<${entityName}Dto>> GetFilteredAsync(${entityName}Filter filter, Guid userId);"
            $updatedContent = $updatedContent -replace [regex]::Escape($lastMethodPattern), $newMethod
            
            Set-Content -Path $servicePath -Value $updatedContent
        }
    }
    
    if (Test-Path $serviceImplPath) {
        $implContent = Get-Content $serviceImplPath -Raw
        
        if (-not ($implContent -match "GetFilteredAsync")) {
            # Add using statements
            $usingMatch = "using Equilibrium.Resources;"
            $newUsings = "using Equilibrium.Resources;`r`nusing Equilibrium.Domain.Specifications;`r`nusing Equilibrium.Application.DTOs.Common;`r`nusing Equilibrium.Application.DTOs.Filters;"
            $updatedContent = $implContent -replace [regex]::Escape($usingMatch), $newUsings
            
            # Add implementation method
            $implementationMethod = @"
    public async Task<PagedResult<${entityName}Dto>> GetFilteredAsync(${entityName}Filter filter, Guid userId)
    {
        var specification = new ${entityName}Specification(filter);
        
        // Apply custom criteria for user ID based on linked entities
        specification.UserId = userId;
        
        var (items, totalCount) = await _unitOfWork.${entityName}s.FindWithSpecificationAsync(specification);
        
        return new PagedResult<${entityName}Dto>
        {
            Items = _mapper.Map<IEnumerable<${entityName}Dto>>(items),
            TotalCount = totalCount,
            PageNumber = filter.PageNumber,
            PageSize = filter.PageSize
        };
    }
"@
            
            # Add the implementation before the last closing brace
            $lastBraceIndex = $updatedContent.LastIndexOf("}")
            $updatedContent = $updatedContent.Substring(0, $lastBraceIndex) + $implementationMethod + "`r`n}" + $updatedContent.Substring($lastBraceIndex + 1)
            
            Set-Content -Path $serviceImplPath -Value $updatedContent
        }
    }
}

function Update-InstallmentController {
    param(
        [string]$entityName,
        [string]$controllerPath
    )
    
    if (Test-Path $controllerPath) {
        $controllerContent = Get-Content $controllerPath -Raw
        
        if (-not ($controllerContent -match "GetFiltered")) {
            # Add using statements
            $usingMatch = "using Equilibrium.Domain.Interfaces.Services;"
            $newUsings = "using Equilibrium.Domain.Interfaces.Services;`r`nusing Equilibrium.Application.DTOs.Common;`r`nusing Equilibrium.Application.DTOs.Filters;`r`nusing Equilibrium.Application.Validations.Filters;"
            $updatedContent = $controllerContent -replace [regex]::Escape($usingMatch), $newUsings
            
            $newEndpoint = @"
        [HttpGet("filter")]
        public async Task<ActionResult<PagedResult<${entityName}Dto>>> GetFiltered([FromQuery] ${entityName}Filter filter)
        {
            if (filter == null)
                filter = new ${entityName}Filter();
                
            var validator = new ${entityName}FilterValidator();
            var validationResult = await validator.ValidateAsync(filter);
            
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Errors);
                
            var userId = HttpContext.GetCurrentUserId();
            var pagedResult = await _service.GetFilteredAsync(filter, userId);
            
            // Add pagination headers
            Response.Headers.Add("X-Pagination-Total", pagedResult.TotalCount.ToString());
            Response.Headers.Add("X-Pagination-Pages", pagedResult.TotalPages.ToString());
            Response.Headers.Add("X-Pagination-Page", pagedResult.PageNumber.ToString());
            Response.Headers.Add("X-Pagination-Size", pagedResult.PageSize.ToString());
            
            return Ok(pagedResult);
        }
"@
            
            # Find position for the new endpoint
            $firstGetPosition = $updatedContent.IndexOf("[HttpGet(")
            $updatedContent = $updatedContent.Substring(0, $firstGetPosition) + $newEndpoint + "`r`n        " + $updatedContent.Substring($firstGetPosition)
            
            Set-Content -Path $controllerPath -Value $updatedContent
        }
    }
}

# Update Payment Installment
$paymentInstallmentServicePath = Join-Path $srcRoot "Equilibrium.Application\Interfaces\IPaymentInstallmentService.cs"
$paymentInstallmentServiceImplPath = Join-Path $srcRoot "Equilibrium.Application\Services\PaymentInstallmentService.cs"
$paymentInstallmentControllerPath = Join-Path $srcRoot "Equilibrium.API\Controllers\PaymentInstallmentsController.cs"

Update-InstallmentService -entityName "PaymentInstallment" -servicePath $paymentInstallmentServicePath -serviceImplPath $paymentInstallmentServiceImplPath
Update-InstallmentController -entityName "PaymentInstallment" -controllerPath $paymentInstallmentControllerPath

# Update Income Installment
$incomeInstallmentServicePath = Join-Path $srcRoot "Equilibrium.Application\Interfaces\IIncomeInstallmentService.cs"
$incomeInstallmentServiceImplPath = Join-Path $srcRoot "Equilibrium.Application\Services\IncomeInstallmentService.cs"
$incomeInstallmentControllerPath = Join-Path $srcRoot "Equilibrium.API\Controllers\IncomeInstallmentsController.cs"

Update-InstallmentService -entityName "IncomeInstallment" -servicePath $incomeInstallmentServicePath -serviceImplPath $incomeInstallmentServiceImplPath
Update-InstallmentController -entityName "IncomeInstallment" -controllerPath $incomeInstallmentControllerPath

Write-Host "Installments filtering implementation completed"