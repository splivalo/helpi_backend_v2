#!/bin/zsh

# List of entity names
entities=("User" "ContactInfo" "Student" "Faculty" "StudentContract" "Customer"
          "Senior" "PaymentMethod" "ServiceCategory" "Service" "StudentService"
          "StudentAvailabilitySlot" "Order" "OrderSchedule" "JobRequest"
          "ScheduleAssignment" "JobInstance" "PaymentTransaction"
          "ScheduleAssignmentReplacement" "Review" "Invoice" "InvoiceEmail"
          "City" "ServiceRegion")

# Namespace and folder path
namespace="Helpi.Infrastructure.Repositories"
folder="src/Helpi.Infrastructure/Repositories"

# Ensure the directory exists
mkdir -p $folder

# Create each entity file
for entity in "${entities[@]}"; do
    filePath="$folder/${entity}Repository.cs"
    if [ ! -f "$filePath" ]; then
        echo "namespace $namespace;

using Helpi.Application.Interfaces;
        using Helpi.Domain.Entities;
using Helpi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public class ${entity}Repository
{
    
}" > "$filePath"
        echo "Created: $filePath"
    else
        echo "Skipped: $filePath (Already Exists)"
    fi
done
