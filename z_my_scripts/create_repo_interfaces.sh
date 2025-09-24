#!/bin/zsh

# List of entity names
entities=("User" "ContactInfo" "Student" "Faculty" "StudentContract" "Customer"
          "Senior" "PaymentMethod" "ServiceCategory" "Service" "StudentService"
          "StudentAvailabilitySlot" "Order" "OrderSchedule" "JobRequest"
          "ScheduleAssignment" "JobInstance" "PaymentTransaction"
          "ScheduleAssignmentReplacement" "Review" "Invoice" "InvoiceEmail"
          "City" "ServiceRegion")

# Namespace and folder path
namespace="Helpi.Application.Interfaces"
folder="src/Helpi.Application/Interfaces"

# Ensure the directory exists
mkdir -p $folder

# Create each entity file
for entity in "${entities[@]}"; do
    filePath="$folder/I${entity}Repository.cs"
    if [ ! -f "$filePath" ]; then
        echo "namespace $namespace;

public class I${entity}Repository
{
    
}" > "$filePath"
        echo "Created: $filePath"
    else
        echo "Skipped: $filePath (Already Exists)"
    fi
done
