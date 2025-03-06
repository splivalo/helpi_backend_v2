#!/bin/zsh

# List of entity names
entities=("User" "ContactInfo" "Student" "Faculty" "StudentContract" "Customer"
          "Senior" "PaymentMethod" "ServiceCategory" "Service" "StudentService"
          "StudentAvailabilitySlot" "Order" "OrderSchedule" "JobRequest"
          "ScheduleAssignment" "JobInstance" "PaymentTransaction"
          "ScheduleAssignmentReplacement" "Review" "Invoice" "InvoiceEmail"
          "City" "ServiceRegion")

# Namespace and folder path
namespace="Helpi.Application.DTOs"
folder="src/Helpi.Application/DTOs"

# Ensure the directory exists
mkdir -p $folder

# Create each entity file
for entity in "${entities[@]}"; do
    filePath="$folder/${entity}Dtos.cs"
    if [ ! -f "$filePath" ]; then
        echo "namespace $namespace;

public class ${entity}Dto
{
    public int Id { get; set; }
}" > "$filePath"
        echo "Created: $filePath"
    else
        echo "Skipped: $filePath (Already Exists)"
    fi
done
