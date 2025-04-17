using AutoMapper;
using FinanceSystem.Application.DTOs.IncomeType;
using FinanceSystem.Application.Interfaces;
using FinanceSystem.Domain.Entities;
using FinanceSystem.Domain.Interfaces.Services;

namespace FinanceSystem.Application.Services
{
    public class IncomeTypeService : IIncomeTypeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public IncomeTypeService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IncomeTypeDto> GetByIdAsync(Guid id)
        {
            var incomeType = await _unitOfWork.IncomeTypes.GetByIdAsync(id);
            if (incomeType == null)
                throw new KeyNotFoundException($"Income type with ID {id} not found");

            return _mapper.Map<IncomeTypeDto>(incomeType);
        }

        public async Task<IEnumerable<IncomeTypeDto>> GetAllSystemTypesAsync()
        {
            var incomeTypes = await _unitOfWork.IncomeTypes.GetAllSystemTypesAsync();
            return _mapper.Map<IEnumerable<IncomeTypeDto>>(incomeTypes);
        }

        public async Task<IEnumerable<IncomeTypeDto>> GetAllAvailableForUserAsync(Guid userId)
        {
            var incomeTypes = await _unitOfWork.IncomeTypes.GetAllAvailableForUserAsync(userId);
            return _mapper.Map<IEnumerable<IncomeTypeDto>>(incomeTypes);
        }

        public async Task<IEnumerable<IncomeTypeDto>> GetUserTypesAsync(Guid userId)
        {
            var incomeTypes = await _unitOfWork.IncomeTypes.GetUserTypesAsync(userId);
            return _mapper.Map<IEnumerable<IncomeTypeDto>>(incomeTypes);
        }

        public async Task<IncomeTypeDto> CreateAsync(CreateIncomeTypeDto createIncomeTypeDto, Guid userId)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with ID {userId} not found");

            var existingType = await _unitOfWork.IncomeTypes.GetByNameAsync(createIncomeTypeDto.Name);
            if (existingType != null)
                throw new InvalidOperationException($"Income type with name '{createIncomeTypeDto.Name}' already exists");

            var incomeType = new IncomeType(
                createIncomeTypeDto.Name,
                createIncomeTypeDto.Description ?? "",
                user
            );

            await _unitOfWork.IncomeTypes.AddAsync(incomeType);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<IncomeTypeDto>(incomeType);
        }

        public async Task<IncomeTypeDto> UpdateAsync(Guid id, UpdateIncomeTypeDto updateIncomeTypeDto)
        {
            var incomeType = await _unitOfWork.IncomeTypes.GetByIdAsync(id);
            if (incomeType == null)
                throw new KeyNotFoundException($"Income type with ID {id} not found");

            if (incomeType.IsSystem)
                throw new InvalidOperationException("Cannot update system income type");

            if (!string.IsNullOrEmpty(updateIncomeTypeDto.Name))
            {
                var existingType = await _unitOfWork.IncomeTypes.GetByNameAsync(updateIncomeTypeDto.Name);
                if (existingType != null && existingType.Id != id)
                    throw new InvalidOperationException($"Income type with name '{updateIncomeTypeDto.Name}' already exists");

                incomeType.UpdateName(updateIncomeTypeDto.Name);
            }

            if (!string.IsNullOrEmpty(updateIncomeTypeDto.Description))
                incomeType.UpdateDescription(updateIncomeTypeDto.Description);

            await _unitOfWork.IncomeTypes.UpdateAsync(incomeType);
            await _unitOfWork.CompleteAsync();

            return _mapper.Map<IncomeTypeDto>(incomeType);
        }

        public async Task DeleteAsync(Guid id)
        {
            var incomeType = await _unitOfWork.IncomeTypes.GetByIdAsync(id);
            if (incomeType == null)
                throw new KeyNotFoundException($"Income type with ID {id} not found");

            if (incomeType.IsSystem)
                throw new InvalidOperationException("Cannot delete system income type");

            var incomes = await _unitOfWork.Incomes.GetIncomesByTypeAsync(incomeType.UserId ?? Guid.Empty, id);
            if (incomes.Any())
                throw new InvalidOperationException("Cannot delete income type that is being used in incomes");

            await _unitOfWork.IncomeTypes.DeleteAsync(incomeType);
            await _unitOfWork.CompleteAsync();
        }
    }
}