using AutoMapper;
using Equilibrium.Application.DTOs.Income;
using Equilibrium.Application.Interfaces;
using Equilibrium.Domain.Entities;
using Equilibrium.Domain.Enums;
using Equilibrium.Domain.Interfaces.Services;
using Equilibrium.Resources;

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

        public async Task<IEnumerable<IncomeDto>> GetFilteredAsync(Guid userId, IncomeFilter filter)
        {
            var query = await _unitOfWork.Incomes.FindAsync(
                        p => p.UserId == userId,
                        p => p.User,
                        p => p.IncomeType
                    );

            if (!string.IsNullOrEmpty(filter.Description))
            {
                query = query.Where(i => i.Description.Contains(filter.Description, StringComparison.OrdinalIgnoreCase));
            }

            if (filter.MinAmount.HasValue)
            {
                query = query.Where(i => i.Amount >= filter.MinAmount.Value);
            }

            if (filter.MaxAmount.HasValue)
            {
                query = query.Where(i => i.Amount <= filter.MaxAmount.Value);
            }

            if (filter.Status.HasValue)
            {
                query = query.Where(i => i.Status == filter.Status.Value);
            }

            if (filter.IncomeTypeId.HasValue)
            {
                query = query.Where(i => i.IncomeTypeId == filter.IncomeTypeId.Value);
            }

            if (filter.IsRecurring.HasValue)
            {
                query = query.Where(i => i.IsRecurring == filter.IsRecurring.Value);
            }

            if (filter.Month.HasValue && filter.Year.HasValue)
            {
                query = query.Where(i => i.DueDate.Month == filter.Month.Value && i.DueDate.Year == filter.Year.Value);
            }
            else
            {
                if (filter.StartDate.HasValue)
                {
                    query = query.Where(i => i.DueDate >= filter.StartDate.Value);
                }

                if (filter.EndDate.HasValue)
                {
                    query = query.Where(i => i.DueDate <= filter.EndDate.Value);
                }
            }

            if (filter.ReceivedStartDate.HasValue)
            {
                query = query.Where(i => i.ReceivedDate >= filter.ReceivedStartDate.Value);
            }

            if (filter.ReceivedEndDate.HasValue)
            {
                query = query.Where(i => i.ReceivedDate <= filter.ReceivedEndDate.Value);
            }

            if (filter.HasInstallments.HasValue)
            {
                if (filter.HasInstallments.Value)
                {
                    query = query.Where(i => i.Installments.Any());
                }
                else
                {
                    query = query.Where(i => !i.Installments.Any());
                }
            }

            query = filter.OrderBy?.ToLower() switch
            {
                "description" => filter.Ascending ? query.OrderBy(i => i.Description) : query.OrderByDescending(i => i.Description),
                "amount" => filter.Ascending ? query.OrderBy(i => i.Amount) : query.OrderByDescending(i => i.Amount),
                "duedate" => filter.Ascending ? query.OrderBy(i => i.DueDate) : query.OrderByDescending(i => i.DueDate),
                "receiveddate" => filter.Ascending ? query.OrderBy(i => i.ReceivedDate) : query.OrderByDescending(i => i.ReceivedDate),
                "status" => filter.Ascending ? query.OrderBy(i => i.Status) : query.OrderByDescending(i => i.Status),
                "createdat" => filter.Ascending ? query.OrderBy(i => i.CreatedAt) : query.OrderByDescending(i => i.CreatedAt),
                _ => filter.Ascending ? query.OrderBy(i => i.DueDate) : query.OrderByDescending(i => i.DueDate)
            };

            var items = query.ToList();
            return _mapper.Map<IEnumerable<IncomeDto>>(items);
        }

        public async Task<IncomeDto> GetByIdAsync(Guid id)
        {
            var income = await _unitOfWork.Incomes.GetIncomeWithDetailsAsync(id);
            return income == null ? throw new KeyNotFoundException(ResourceFinanceApi.Income_NotFound) : _mapper.Map<IncomeDto>(income);
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
    }
}

