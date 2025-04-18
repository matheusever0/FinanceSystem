using AutoMapper;
using FinanceSystem.Application.DTOs.Income;
using FinanceSystem.Application.Interfaces;
using FinanceSystem.Domain.Entities;
using FinanceSystem.Domain.Enums;
using FinanceSystem.Domain.Interfaces.Services;

namespace FinanceSystem.Application.Services
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
            if (income == null)
                throw new KeyNotFoundException($"Income with ID {id} not found");

            return _mapper.Map<IncomeDto>(income);
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
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found");

            var incomeType = await _unitOfWork.IncomeTypes.GetByIdAsync(createIncomeDto.IncomeTypeId);
            if (incomeType == null)
                throw new KeyNotFoundException($"Income type with ID {createIncomeDto.IncomeTypeId} not found");

            if (!incomeType.IsSystem && incomeType.UserId != userId)
                throw new UnauthorizedAccessException("User does not have access to this income type");

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
            var income = await _unitOfWork.Incomes.GetIncomeWithDetailsAsync(id);
            if (income == null)
                throw new KeyNotFoundException($"Income with ID {id} not found");

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
                            income.MarkAsReceived(DateTime.UtcNow);
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
                var incomeType = await _unitOfWork.IncomeTypes.GetByIdAsync(updateIncomeDto.IncomeTypeId.Value);
                if (incomeType == null)
                    throw new KeyNotFoundException($"Income type with ID {updateIncomeDto.IncomeTypeId.Value} not found");

                if (!incomeType.IsSystem && incomeType.UserId != income.UserId)
                    throw new UnauthorizedAccessException("User does not have access to this income type");

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
            var income = await _unitOfWork.Incomes.GetByIdAsync(id);
            if (income == null)
                throw new KeyNotFoundException($"Income with ID {id} not found");

            if (income.Status == IncomeStatus.Received)
                throw new InvalidOperationException("Cannot delete a received income");

            await _unitOfWork.Incomes.DeleteAsync(income);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<IncomeDto> MarkAsReceivedAsync(Guid id, DateTime? receivedDate = null)
        {
            var income = await _unitOfWork.Incomes.GetIncomeWithDetailsAsync(id);
            if (income == null)
                throw new KeyNotFoundException($"Income with ID {id} not found");

            income.MarkAsReceived(receivedDate ?? DateTime.UtcNow);
            await _unitOfWork.Incomes.UpdateAsync(income);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<IncomeDto>(income);
        }

        public async Task<IncomeDto> CancelAsync(Guid id)
        {
            var income = await _unitOfWork.Incomes.GetIncomeWithDetailsAsync(id);
            if (income == null)
                throw new KeyNotFoundException($"Income with ID {id} not found");

            income.Cancel();
            await _unitOfWork.Incomes.UpdateAsync(income);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<IncomeDto>(income);
        }
    }
}