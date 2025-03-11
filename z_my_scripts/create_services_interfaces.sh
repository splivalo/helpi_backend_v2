#!/bin/zsh

# List of entity names
entities=("User" "ContactInfo" "Student" "Faculty" "StudentContract" "Customer"
          "Senior" "PaymentMethod" "ServiceCategory" "Service" "StudentService"
          "StudentAvailabilitySlot" "Order" "OrderSchedule" "JobRequest"
          "ScheduleAssignment" "JobInstance" "PaymentTransaction"
          "ScheduleAssignmentReplacement" "Review" "Invoice" "InvoiceEmail"
          "City" "ServiceRegion")

# Namespace and folder path
namespace="Helpi.Application.Services.Interfaces"
folder="src/Helpi.Application/Services/Interfaces"


# Ensure the directory exists
mkdir -p $folder

# Create each entity file
for entity in "${entities[@]}"; do
    filePath="$folder/I${entity}Service.cs"
    if [ ! -f "$filePath" ]; then
        echo "
    


using Helpi.Application.DTOs;

        namespace $namespace;

public class I${entity}Service
{
    
}" > "$filePath"
        echo "Created: $filePath"
    else
        echo "Skipped: $filePath (Already Exists)"
    fi
done
