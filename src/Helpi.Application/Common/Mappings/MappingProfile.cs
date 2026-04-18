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
        CreateMap<Admin, AdminDto>();
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
        CreateMap<StudentContract, StudentContractDto>()
            .ForMember(dest => dest.Sessions, opt => opt.MapFrom(src => src.JobInstances));
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
             .ForMember(dest => dest.SeniorName, opt => opt.MapFrom(src => src.Senior != null && src.Senior.Contact != null ? src.Senior.Contact.FullName : null))
             .ForMember(dest => dest.SeniorEmail, opt => opt.MapFrom(src => src.Senior != null && src.Senior.Contact != null ? src.Senior.Contact.Email : null))
             .ForMember(dest => dest.SeniorPhone, opt => opt.MapFrom(src => src.Senior != null && src.Senior.Contact != null ? src.Senior.Contact.Phone : null))
             .ForMember(dest => dest.SeniorAddress, opt => opt.MapFrom(src => src.Senior != null && src.Senior.Contact != null ? src.Senior.Contact.FullAddress : null))
             .ForMember(dest => dest.SeniorLatitude, opt => opt.MapFrom(src => src.Senior != null && src.Senior.Contact != null ? (decimal?)src.Senior.Contact.Latitude : null))
             .ForMember(dest => dest.SeniorLongitude, opt => opt.MapFrom(src => src.Senior != null && src.Senior.Contact != null ? (decimal?)src.Senior.Contact.Longitude : null))
             .ForMember(dest => dest.Services, opt => opt.MapFrom(src => src.OrderServices.Select(os => os.Service)))
             .ForMember(dest => dest.Schedules, opt => opt.MapFrom(src => src.Schedules))
             .ForMember(dest => dest.AssignedStudentName, opt => opt.MapFrom(src =>
                 src.Schedules.SelectMany(s => s.Assignments)
                     .Where(a => a.Status == Helpi.Domain.Enums.AssignmentStatus.Accepted || a.Status == Helpi.Domain.Enums.AssignmentStatus.Completed)
                     .Select(a => a.Student != null && a.Student.Contact != null ? a.Student.Contact.FullName : null)
                     .FirstOrDefault()))
             .ForMember(dest => dest.AssignedStudentId, opt => opt.MapFrom(src =>
                 src.Schedules.SelectMany(s => s.Assignments)
                     .Where(a => a.Status == Helpi.Domain.Enums.AssignmentStatus.Accepted || a.Status == Helpi.Domain.Enums.AssignmentStatus.Completed)
                     .Select(a => (int?)a.StudentId)
                     .FirstOrDefault()))
             .ForMember(dest => dest.AssignedStudentEmail, opt => opt.MapFrom(src =>
                 src.Schedules.SelectMany(s => s.Assignments)
                     .Where(a => a.Status == Helpi.Domain.Enums.AssignmentStatus.Accepted || a.Status == Helpi.Domain.Enums.AssignmentStatus.Completed)
                     .Select(a => a.Student != null && a.Student.Contact != null ? a.Student.Contact.Email : null)
                     .FirstOrDefault()))
             .ForMember(dest => dest.AssignedStudentPhone, opt => opt.MapFrom(src =>
                 src.Schedules.SelectMany(s => s.Assignments)
                     .Where(a => a.Status == Helpi.Domain.Enums.AssignmentStatus.Accepted || a.Status == Helpi.Domain.Enums.AssignmentStatus.Completed)
                     .Select(a => a.Student != null && a.Student.Contact != null ? a.Student.Contact.Phone : null)
                     .FirstOrDefault()))
             .ForMember(dest => dest.AssignedStudentAddress, opt => opt.MapFrom(src =>
                 src.Schedules.SelectMany(s => s.Assignments)
                     .Where(a => a.Status == Helpi.Domain.Enums.AssignmentStatus.Accepted || a.Status == Helpi.Domain.Enums.AssignmentStatus.Completed)
                     .Select(a => a.Student != null && a.Student.Contact != null ? a.Student.Contact.FullAddress : null)
                     .FirstOrDefault()))
             .ForMember(dest => dest.AssignedStudentFaculty, opt => opt.MapFrom(src =>
                 src.Schedules.SelectMany(s => s.Assignments)
                     .Where(a => a.Status == Helpi.Domain.Enums.AssignmentStatus.Accepted || a.Status == Helpi.Domain.Enums.AssignmentStatus.Completed)
                     .Select(a => a.Student != null && a.Student.Faculty != null ? a.Student.FacultyId : (int?)null)
                     .FirstOrDefault()))
             .ForMember(dest => dest.AssignedStudentDateOfBirth, opt => opt.MapFrom(src =>
                 src.Schedules.SelectMany(s => s.Assignments)
                     .Where(a => a.Status == Helpi.Domain.Enums.AssignmentStatus.Accepted || a.Status == Helpi.Domain.Enums.AssignmentStatus.Completed)
                     .Select(a => a.Student != null && a.Student.Contact != null ? (DateOnly?)a.Student.Contact.DateOfBirth : null)
                     .FirstOrDefault()))
             .ForMember(dest => dest.AssignedStudentGender, opt => opt.MapFrom(src =>
                 src.Schedules.SelectMany(s => s.Assignments)
                     .Where(a => a.Status == Helpi.Domain.Enums.AssignmentStatus.Accepted || a.Status == Helpi.Domain.Enums.AssignmentStatus.Completed)
                     .Select(a => a.Student != null ? (int?)a.Student.Contact.Gender : null)
                     .FirstOrDefault()))
             .ForMember(dest => dest.AssignedStudentCity, opt => opt.MapFrom(src =>
                 src.Schedules.SelectMany(s => s.Assignments)
                     .Where(a => a.Status == Helpi.Domain.Enums.AssignmentStatus.Accepted || a.Status == Helpi.Domain.Enums.AssignmentStatus.Completed)
                     .Select(a => a.Student != null && a.Student.Contact != null ? a.Student.Contact.CityName : null)
                     .FirstOrDefault()))
             .ForMember(dest => dest.AssignedStudentStatus, opt => opt.MapFrom(src =>
                 src.Schedules.SelectMany(s => s.Assignments)
                     .Where(a => a.Status == Helpi.Domain.Enums.AssignmentStatus.Accepted || a.Status == Helpi.Domain.Enums.AssignmentStatus.Completed)
                     .Select(a => a.Student != null ? (int?)a.Student.Status : null)
                     .FirstOrDefault()))
             .ForMember(dest => dest.AssignedStudentAverageRating, opt => opt.MapFrom(src =>
                 src.Schedules.SelectMany(s => s.Assignments)
                     .Where(a => a.Status == Helpi.Domain.Enums.AssignmentStatus.Accepted || a.Status == Helpi.Domain.Enums.AssignmentStatus.Completed)
                     .Select(a => a.Student != null ? (decimal?)a.Student.AverageRating : null)
                     .FirstOrDefault()))
             .ForMember(dest => dest.AssignedStudentTotalReviews, opt => opt.MapFrom(src =>
                 src.Schedules.SelectMany(s => s.Assignments)
                     .Where(a => a.Status == Helpi.Domain.Enums.AssignmentStatus.Accepted || a.Status == Helpi.Domain.Enums.AssignmentStatus.Completed)
                     .Select(a => a.Student != null ? (int?)a.Student.TotalReviews : null)
                     .FirstOrDefault()))
             .ForMember(dest => dest.AssignedStudentDaysToContractExpire, opt => opt.MapFrom(src =>
                 src.Schedules.SelectMany(s => s.Assignments)
                     .Where(a => a.Status == Helpi.Domain.Enums.AssignmentStatus.Accepted || a.Status == Helpi.Domain.Enums.AssignmentStatus.Completed)
                     .Select(a => a.Student != null ? a.Student.DaysToContractExpire : null)
                     .FirstOrDefault()))
             .ForMember(dest => dest.CouponCode, opt => opt.MapFrom(src => src.Coupon != null ? src.Coupon.Code : null));



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
        CreateMap<JobInstance, SessionDto>()
            .ForMember(dest => dest.Services,
                opt => opt.MapFrom(src => src.Order != null
                    ? src.Order.OrderServices.Select(os => os.Service).ToList()
                    : new List<Service>()))
            .ForMember(dest => dest.SeniorReview,
                opt => opt.MapFrom(src => src.Reviews
                    .Where(r => !r.IsPending && r.Type == ReviewType.SeniorToStudent)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefault()))
            .ForMember(dest => dest.StudentReview,
                opt => opt.MapFrom(src => src.Reviews
                    .Where(r => !r.IsPending && r.Type == ReviewType.StudentToSenior)
                    .OrderByDescending(r => r.CreatedAt)
                    .FirstOrDefault()));
        CreateMap<SessionUpdateDto, JobInstance>();

        // PaymentTransaction Mappings
        CreateMap<PaymentTransaction, PaymentTransactionDto>();
        CreateMap<PaymentTransactionUpdateDto, PaymentTransaction>();

        // Review Mappings
        CreateMap<Review, ReviewDto>();
        // CreateMap<MakeReviewUpdateDto, Review>();
        CreateMap<ReviewUpdateDto, Review>();

        // Coupon Mappings
        CreateMap<Coupon, CouponDto>();
        CreateMap<CouponCreateDto, Coupon>();

        // Sponsor Mappings
        CreateMap<Sponsor, SponsorDto>();
        CreateMap<SponsorCreateDto, Sponsor>();

        // SuspensionLog Mappings
        CreateMap<SuspensionLog, SuspensionLogDto>();

        // Invoice Mappings
        CreateMap<Invoice, InvoiceDto>();
        CreateMap<InvoiceUpdateDto, Invoice>();

        // InvoiceEmail Mappings
        CreateMap<HEmail, HEmailDto>();
        CreateMap<HEmailUpdateDto, HEmail>();

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


        // Map JobInstance → CompletedSessionDto
        CreateMap<JobInstance, CompletedSessionDto>();

        // Chat Mappings
        CreateMap<ChatRoom, ChatRoomDto>();
        CreateMap<ChatMessage, ChatMessageDto>();
    }


}