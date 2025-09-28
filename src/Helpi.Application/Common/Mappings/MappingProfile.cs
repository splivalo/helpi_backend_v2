using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.DTOs.Auth;
using Helpi.Application.DTOs.JobRequest;
using Helpi.Application.DTOs.Order;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {

        CreateMap<HNotification, HNotificationDto>();
        CreateMap<CreateHNotificationDto, HNotification>();
        CreateMap<UpdateHNotificationDto, HNotification>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        CreateMap<PricingChangeHistory, PricingChangeHistoryDto>().ReverseMap();
        CreateMap<PricingConfiguration, PricingConfigurationDto>().ReverseMap();

        // User Mappings
        CreateMap<User, UserDto>();
        CreateMap<UserCreateDto, User>();
        CreateMap<UserUpdateDto, User>();

        CreateMap<PaymentProfile, PaymentProfileDto>();
        CreateMap<CreatePaymentProfileDto, PaymentProfile>();
        CreateMap<UpdatePaymentProfileDto, PaymentProfile>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));



        // ContactInfo Mappings
        CreateMap<ContactInfo, ContactInfoDto>();
        CreateMap<ContactInfoCreateDto, ContactInfo>();
        CreateMap<ContactInfoUpdateDto, ContactInfo>();

        // 
        CreateMap<ContactInfoDto, ContactInfo>();

        CreateMap<StudentRegisterDto, Student>()
              .ForMember(dest => dest.Contact, opt => opt.MapFrom(src => src.ContactInfo));

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
            .ForMember(dest => dest.Contact, opt => opt.MapFrom(src => src.Contact))
            .ForMember(dest => dest.Seniors, opt => opt.MapFrom(src => src.Seniors));
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
        CreateMap<ServiceCategory, ServiceCategoryDto>()
           .ForMember(dest => dest.Services, opt => opt.MapFrom(src => src.Services));
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
        CreateMap<OrderCreateDto, Order>()
              .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => OrderStatus.Pending))
              .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow))
              .ForMember(dest => dest.OrderServices, opt => opt.Ignore())
              .ForMember(dest => dest.Schedules, opt => opt.Ignore());

        CreateMap<Order, OrderDto>()
             .ForMember(dest => dest.Services, opt => opt.MapFrom(src => src.OrderServices.Select(os => os.Service)))
            .ForMember(dest => dest.Schedules, opt => opt.MapFrom(src => src.Schedules));



        // OrderService Mappings
        CreateMap<OrderServiceCreateDto, OrderService>()
            .ForMember(dest => dest.OrderId, opt => opt.Ignore());

        CreateMap<OrderService, OrderServiceDto>()
            .ForMember(dest => dest.Service, opt => opt.MapFrom(src => src.Service));


        // OrderSchedule Mappings
        CreateMap<OrderSchedule, OrderScheduleDto>();
        CreateMap<OrderScheduleCreateDto, OrderSchedule>();
        CreateMap<OrderScheduleUpdateDto, OrderSchedule>();



        // JobRequest Mappings
        CreateMap<JobRequest, JobRequestDto>();

        CreateMap<JobRequestCreateDto, JobRequest>();
        CreateMap<JobRequestUpdateDto, JobRequest>();

        CreateMap<RespondToJobRequestDto, JobRequest>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.JobRequestId))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src =>
                src.isAccepted ? JobRequestStatus.Accepted : JobRequestStatus.Declined));

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
        // CreateMap<ScheduleAssignmentReplacement, ScheduleAssignmentReplacementDto>();
        // CreateMap<ScheduleAssignmentReplacementCreateDto, ScheduleAssignmentReplacement>();

        // Review Mappings
        CreateMap<Review, ReviewDto>();
        // CreateMap<MakeReviewUpdateDto, Review>();
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


        // Map StudentContract → CompletedStudentContractDto
        CreateMap<StudentContract, CompletedStudentContractDto>()
            .ForMember(dest => dest.CompletedJobs,
                opt => opt.MapFrom(src => src.JobInstances))
            .ForMember(dest => dest.TotalJobs,
                opt => opt.MapFrom(src => src.JobInstances.Count()))
            .ForMember(dest => dest.DurationHours,
                opt => opt.MapFrom(src => src.JobInstances
                    .Sum(j => j.DurationHours)))
            .ForMember(dest => dest.TotalCompanyEarnings,
                opt => opt.MapFrom(src => src.JobInstances
                    .Sum(j => j.CompanyAmount)))
            .ForMember(dest => dest.TotalStudentEarnings,
                opt => opt.MapFrom(src => src.JobInstances
                    .Sum(j => j.ServiceProviderAmount)));


        // Map JobInstance → CompletedJobInstanceDto
        CreateMap<JobInstance, CompletedJobInstanceDto>();
    }


}