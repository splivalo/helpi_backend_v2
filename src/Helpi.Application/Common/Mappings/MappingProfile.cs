using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Domain.Entities;

namespace Helpi.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // User Mappings
        CreateMap<User, UserDto>();
        CreateMap<UserCreateDto, User>();
        CreateMap<UserUpdateDto, User>();

        // ContactInfo Mappings
        CreateMap<ContactInfo, ContactInfoDto>();
        CreateMap<ContactInfoCreateDto, ContactInfo>();
        CreateMap<ContactInfoUpdateDto, ContactInfo>();

        // Student Mappings
        CreateMap<Student, StudentDto>()
            .ForMember(dest => dest.Contact, opt => opt.MapFrom(src => src.Contact));
        CreateMap<StudentCreateDto, Student>();
        CreateMap<StudentUpdateDto, Student>();

        // Faculty Mappings
        CreateMap<Faculty, FacultyDto>();
        CreateMap<FacultyCreateDto, Faculty>();
        CreateMap<FacultyUpdateDto, Faculty>();

        // StudentContract Mappings
        CreateMap<StudentContract, StudentContractDto>();
        CreateMap<StudentContractCreateDto, StudentContract>();
        CreateMap<StudentContractUpdateDto, StudentContract>();

        // Customer Mappings
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.Contact, opt => opt.MapFrom(src => src.Contact));
        CreateMap<CustomerCreateDto, Customer>();
        CreateMap<CustomerUpdateDto, Customer>();

        // Senior Mappings
        CreateMap<Senior, SeniorDto>()
            .ForMember(dest => dest.Contact, opt => opt.MapFrom(src => src.Contact));
        CreateMap<SeniorCreateDto, Senior>();
        CreateMap<SeniorUpdateDto, Senior>();

        // PaymentMethod Mappings
        CreateMap<PaymentMethod, PaymentMethodDto>();
        CreateMap<PaymentMethodCreateDto, PaymentMethod>();
        CreateMap<PaymentMethodUpdateDto, PaymentMethod>();

        // ServiceCategory Mappings
        CreateMap<ServiceCategory, ServiceCategoryDto>();
        CreateMap<ServiceCategoryCreateDto, ServiceCategory>();
        CreateMap<ServiceCategoryUpdateDto, ServiceCategory>();

        // Service Mappings
        CreateMap<Service, ServiceDto>();
        CreateMap<ServiceCreateDto, Service>();
        CreateMap<ServiceUpdateDto, Service>();

        // StudentService Mappings
        CreateMap<StudentService, StudentServiceDto>();
        CreateMap<StudentServiceCreateDto, StudentService>();
        CreateMap<StudentServiceUpdateDto, StudentService>();

        // StudentAvailabilitySlot Mappings
        CreateMap<StudentAvailabilitySlot, StudentAvailabilitySlotDto>();
        CreateMap<StudentAvailabilitySlotCreateDto, StudentAvailabilitySlot>();
        CreateMap<StudentAvailabilitySlotUpdateDto, StudentAvailabilitySlot>();

        // Order Mappings
        CreateMap<Order, OrderDto>()
            .ForMember(dest => dest.Service, opt => opt.MapFrom(src => src.Service));
        CreateMap<OrderCreateDto, Order>();
        CreateMap<OrderUpdateDto, Order>();

        // OrderSchedule Mappings
        CreateMap<OrderSchedule, OrderScheduleDto>();
        CreateMap<OrderScheduleCreateDto, OrderSchedule>();
        CreateMap<OrderScheduleUpdateDto, OrderSchedule>();

        // JobRequest Mappings
        CreateMap<JobRequest, JobRequestDto>();
        CreateMap<JobRequestCreateDto, JobRequest>();
        CreateMap<JobRequestUpdateDto, JobRequest>();

        // ScheduleAssignment Mappings
        CreateMap<ScheduleAssignment, ScheduleAssignmentDto>();
        CreateMap<ScheduleAssignmentCreateDto, ScheduleAssignment>();
        CreateMap<ScheduleAssignmentUpdateDto, ScheduleAssignment>();

        // JobInstance Mappings
        CreateMap<JobInstance, JobInstanceDto>();
        CreateMap<JobInstanceUpdateDto, JobInstance>();

        // PaymentTransaction Mappings
        CreateMap<PaymentTransaction, PaymentTransactionDto>();
        CreateMap<PaymentTransactionUpdateDto, PaymentTransaction>();

        // ScheduleAssignmentReplacement Mappings
        CreateMap<ScheduleAssignmentReplacement, ScheduleAssignmentReplacementDto>();
        CreateMap<ScheduleAssignmentReplacementCreateDto, ScheduleAssignmentReplacement>();

        // Review Mappings
        CreateMap<Review, ReviewDto>();
        CreateMap<ReviewCreateDto, Review>();
        CreateMap<ReviewUpdateDto, Review>();

        // Invoice Mappings
        CreateMap<Invoice, InvoiceDto>();
        CreateMap<InvoiceUpdateDto, Invoice>();

        // InvoiceEmail Mappings
        CreateMap<InvoiceEmail, InvoiceEmailDto>();
        CreateMap<InvoiceEmailUpdateDto, InvoiceEmail>();

        // City Mappings
        CreateMap<City, CityDto>();
        CreateMap<CityCreateDto, City>();
        CreateMap<CityUpdateDto, City>();

        // ServiceRegion Mappings
        CreateMap<ServiceRegion, ServiceRegionDto>();
        CreateMap<ServiceRegionCreateDto, ServiceRegion>();
        CreateMap<ServiceRegionUpdateDto, ServiceRegion>();

        // Additional custom mappings for complex scenarios
        CreateMap<DateTime, DateOnly>().ConvertUsing(dt => DateOnly.FromDateTime(dt));
        CreateMap<DateOnly, DateTime>().ConvertUsing(d => d.ToDateTime(TimeOnly.MinValue));
        CreateMap<TimeOnly, TimeSpan>().ConvertUsing(t => t.ToTimeSpan());
        CreateMap<TimeSpan, TimeOnly>().ConvertUsing(t => TimeOnly.FromTimeSpan(t));
    }
}