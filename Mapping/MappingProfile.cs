using Mapster;
using Sonic.Models;

namespace Sonic.API.Mapping
{
    public static class MappingProfile
    {
        public static void Register()
        {
            // Example mapping configuration
            TypeAdapterConfig<User, UserReadDto>.NewConfig();
            TypeAdapterConfig<UserRegisterDto, User>.NewConfig();
            TypeAdapterConfig<User, UserCreatedDto>.NewConfig();
            TypeAdapterConfig<User, UserUpdateDto>.NewConfig();
            TypeAdapterConfig<UserCreatedDto, User>.NewConfig();
        }
    }
}