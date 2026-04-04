
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

namespace Helpi.Application.Services;


public class UserService
{
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
                _userRepository = userRepository;
                _mapper = mapper;
        }

        public async Task<List<UserDto>> GetAllUsersAsync() =>
            _mapper.Map<List<UserDto>>(await _userRepository.GetAllAsync());

        public async Task<UserDto> GetUserByIdAsync(int id) =>
                        _mapper.Map<UserDto>(await _userRepository.GetByIdAsync(id)
                                ?? throw new KeyNotFoundException($"User with ID {id} was not found."));

        public async Task<UserDto> CreateUserAsync(UserCreateDto dto)
        {
                var user = _mapper.Map<User>(dto);
                await _userRepository.AddAsync(user);
                return _mapper.Map<UserDto>(user);
        }

        public async Task UpdateUserAsync(int id, UserUpdateDto dto)
        {
                var user = await _userRepository.GetByIdAsync(id)
                        ?? throw new KeyNotFoundException($"User with ID {id} was not found.");
                _mapper.Map(dto, user);
                await _userRepository.UpdateAsync(user);
        }

        public async Task DeleteUserAsync(int id)
        {
                var user = await _userRepository.GetByIdAsync(id)
                        ?? throw new KeyNotFoundException($"User with ID {id} was not found.");
                await _userRepository.DeleteAsync(user);
        }
}