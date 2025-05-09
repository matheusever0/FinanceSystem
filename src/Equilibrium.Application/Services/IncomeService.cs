using AutoMapper;
using Equilibrium.Application.DTOs.Income;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Entities;
using Equilibrium.Domain.Enums;
using Equilibrium.Domain.Specifications;
using Equilibrium.Application.DTOs.Common;
using Equilibrium.Domain.Interfaces.Services;
using Equilibrium.Resources;
using Equilibrium.Domain.DTOs.Filters;

namespace Equilibrium.Application.Services
{
    public class IncomeService : IIncomeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public IncomeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IncomeDto> GetByIdAsync(Guid id)
        {
            var income = await _unitOfWork.Incomes.GetIncomeWithDetailsAsync(id);
            return income == null ? throw new KeyNotFoundException(ResourceFinanceApi.Income_NotFound) : _mapper.Map<IncomeDto>(income);
        }

        public async Task<IEnumerable<IncomeDto>> GetAllByUserIdAsync(Guid userId)
        {
            var incomes = await _unitOfWork.Incomes.GetIncomesByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<IncomeDto>>(incomes);
        }

        public async Task<IEnumerable<IncomeDto>> GetByMonthAsync(Guid userId, int month, int year)
        {
            var incomes = await _unitOfWork.Incomes.GetIncomesByUserIdAndMonthAsync(userId, month, year);
            return _mapper.Map<IEnumerable<IncomeDto>>(incomes);
        }

        public async Task<IEnumerable<IncomeDto>> GetPendingAsync(Guid userId)
        {
            var incomes = await _unitOfWork.Incomes.GetPendingIncomesByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<IncomeDto>>(incomes);
        }

        public async Task<IEnumerable<IncomeDto>> GetOverdueAsync(Guid userId)
        {
            var payments = await _unitOfWork.Incomes.GetOverdueIncomesByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<IncomeDto>>(payments);
        }

        public async Task<IEnumerable<IncomeDto>> GetReceivedAsync(Guid userId)
        {
            var incomes = await _unitOfWork.Incomes.GetReceivedIncomesByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<IncomeDto>>(incomes);
        }

        public async Task<IEnumerable<IncomeDto>> GetByTypeAsync(Guid userId, Guid incomeTypeId)
        {
            var incomes = await _unitOfWork.Incomes.GetIncomesByTypeAsync(userId, incomeTypeId);
            return _mapper.Map<IEnumerable<IncomeDto>>(incomes);
        }

        public async Task<IncomeDto> CreateAsync(CreateIncomeDto createIncomeDto, Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId) ?? throw new KeyNotFoundException(ResourceFinanceApi.User_NotFound);
            var incomeType = await _unitOfWork.IncomeTypes.GetByIdAsync(createIncomeDto.IncomeTypeId) ?? throw new KeyNotFoundException(ResourceFinanceApi.Income_NotFound);
            if (!incomeType.IsSystem && incomeType.UserId != userId)
                throw new UnauthorizedAccessException(ResourceFinanceApi.Error_Unauthorized);

            var income = new Income(
                createIncomeDto.Description,
                createIncomeDto.Amount,
                createIncomeDto.DueDate,
                incomeType,
                user,
                createIncomeDto.IsRecurring,
                createIncomeDto.Notes
            );

            if (createIncomeDto.ReceivedDate.HasValue)
            {
                income.MarkAsReceived(createIncomeDto.ReceivedDate.Value);
            }

            if (createIncomeDto.NumberOfInstallments > 1)
            {
                income.AddInstallments(createIncomeDto.NumberOfInstallments);
            }

            await _unitOfWork.Incomes.AddAsync(income);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<IncomeDto>(income);
        }

        public async Task<IncomeDto> UpdateAsync(Guid id, UpdateIncomeDto updateIncomeDto)
        {
            var income = await _unitOfWork.Incomes.GetIncomeWithDetailsAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.Income_NotFound);
            if (!string.IsNullOrEmpty(updateIncomeDto.Description))
                income.UpdateDescription(updateIncomeDto.Description);

            if (updateIncomeDto.Amount.HasValue && updateIncomeDto.Amount.Value > 0)
                income.UpdateAmount(updateIncomeDto.Amount.Value);

            if (updateIncomeDto.DueDate.HasValue)
                income.UpdateDueDate(updateIncomeDto.DueDate.Value);

            if (updateIncomeDto.Notes != null)
                income.UpdateNotes(updateIncomeDto.Notes);

            if (updateIncomeDto.Status.HasValue)
            {
                switch (updateIncomeDto.Status.Value)
                {
                    case IncomeStatus.Received:
                        if (updateIncomeDto.ReceivedDate.HasValue)
                            income.MarkAsReceived(updateIncomeDto.ReceivedDate.Value);
                        else
                            income.MarkAsReceived(DateTime.Now);
                        break;
                    case IncomeStatus.Cancelled:
                        income.Cancel();
                        break;
                    case IncomeStatus.Pending:
                        income.Pending();
                        break;
                }
            }

            if (updateIncomeDto.IncomeTypeId.HasValue)
            {
                var incomeType = await _unitOfWork.IncomeTypes.GetByIdAsync(updateIncomeDto.IncomeTypeId.Value) ?? throw new KeyNotFoundException(ResourceFinanceApi.Income_NotFound);
                if (!incomeType.IsSystem && incomeType.UserId != income.UserId)
                    throw new UnauthorizedAccessException(ResourceFinanceApi.Error_Unauthorized);

                income.UpdateType(incomeType);
            }

            if (updateIncomeDto.IsRecurring.HasValue)
            {
                income.UpdateRecurring(updateIncomeDto.IsRecurring.Value);
            }

            await _unitOfWork.Incomes.UpdateAsync(income);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<IncomeDto>(income);
        }

        public async Task DeleteAsync(Guid id)
        {
            var income = await _unitOfWork.Incomes.GetByIdAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.Income_NotFound);
            if (income.Status == IncomeStatus.Received)
                throw new InvalidOperationException(ResourceFinanceApi.Income_AlreadyReceived);

            await _unitOfWork.Incomes.DeleteAsync(income);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IncomeDto> MarkAsReceivedAsync(Guid id, DateTime? receivedDate = null)
        {
            var income = await _unitOfWork.Incomes.GetIncomeWithDetailsAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.Income_NotFound);
            income.MarkAsReceived(receivedDate ?? DateTime.Now);
            await _unitOfWork.Incomes.UpdateAsync(income);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<IncomeDto>(income);
        }

        public async Task<IncomeDto> CancelAsync(Guid id)
        {
            var income = await _unitOfWork.Incomes.GetIncomeWithDetailsAsync(id) ?? throw new KeyNotFoundException(ResourceFinanceApi.Income_NotFound);
            income.Cancel();
            await _unitOfWork.Incomes.UpdateAsync(income);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<IncomeDto>(income);
        }

        public async Task<PagedResult<IncomeDto>> GetFilteredAsync(IncomeFilter filter, Guid userId)
        {
            var specification = new IncomeSpecification(filter)
            {
                UserId = userId
            };

            var (incomes, totalCount) = await _unitOfWork.Incomes.FindWithSpecificationAsync(specification);

            return new PagedResult<IncomeDto>
            {
                Items = _mapper.Map<IEnumerable<IncomeDto>>(incomes),
                TotalCount = totalCount,
                PageNumber = filter.PageNumber,
                PageSize = filter.PageSize
            };
        }
    }
}

