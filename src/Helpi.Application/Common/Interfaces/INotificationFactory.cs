using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Common.Interfaces
{
    public interface INotificationFactory
    {
        HNotification CreateNewStudentNotification(int receiverUserId, int studentId);
        HNotification CreateNewSeniorNotification(int receiverUserId, int seniorId);
        HNotification CreatePaymentFailedNotification(int receiverUserId, int seniorId, int orderId, int jobInstanceId, string culture);
        HNotification CreatePaymentSuccessNotification(int receiverUserId, int seniorId, int orderId, int jobInstanceId, string culture);

        HNotification CreateStudentJobReminderNotification(JobInstance jobInstance, string culture);
        HNotification? ReassignmentStartNotification(
                int recieverId,
                ReassignmentRecord record,
                int seniorId,
                NotificationType type,
                string culture = "hr");

        HNotification AdminOrderScheduleCancelledNotification(
       int adminId, OrderSchedule orderSchedule, int seniorId);

        HNotification AdminOrderCancelledNotification(
            int adminId, Order order);

        HNotification NoEligibleStudentsNotification(
    int recieverUserId,
    Order order,
   OrderSchedule schedule,
   ReassignmentRecord? reassignment = null);

        HNotification AllEligibleStudentsNotified(
            int recieverUserId,
            Order order,
           OrderSchedule schedule,
           ReassignmentRecord? reassignment = null);

        HNotification JobRescheduledNotification(
            int receiverUserId,
            JobInstance originalJobInstance,
            JobInstance updatedJobInstance,
            string culture);

        HNotification JobCancelledNotification(int recieverId, JobInstance jobInstance, string culture);
        HNotification ScheduleAssignmentCancelledNotification(
        int recieverId, ScheduleAssignment scheduleAssignment, int seniorId, string culture);

        HNotification SeniorOrderCancelledNotification(
                 int receiverUserId, Order order, string culture);
        HNotification StudentContractAboutToExpire(int studentId, int contractId, string culture);
        HNotification StudentContractAdded(int studentId, int contractId, string culture);
        HNotification StudentContractUpdated(int studentId, int contractId, string culture);
        HNotification StudentContractDeleted(int studentId, int contractId, string culture);
        HNotification StudentContractExpired(int studentId, int contractId, string culture);

        HNotification ReviewRequestNotification(int recieverId, Review review, JobInstance jobInstance, string culture);

        HNotification JobRequestNotification(int recieverId,
    OrderSchedule orderSchedule,
     ReassignmentRecord? reassignmentRecord,
      string culture);

        HNotification UserDeletedNotification(int receiverUserId, int deletedUserId, string deletedUserName, NotificationType type);
        HNotification AdminNewOrderNotification(int adminId, Order order);
    }
}
