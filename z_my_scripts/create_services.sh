#!/bin/zsh

# List of entity names
entities=("User" "ContactInfo" "Student" "Faculty" "StudentContract" "Customer"
          "Senior" "PaymentMethod" "ServiceCategory" "Service" "StudentService"
          "StudentAvailabilitySlot" "Order" "OrderSchedule" "JobRequest"
          "ScheduleAssignment" "JobInstance" "PaymentTransaction"
          "ScheduleAssignmentReplacement" "Review" "Invoice" "InvoiceEmail"
          "City" "ServiceRegion")

# Namespace and folder path
namespace="Helpi.Application.Services"
folder="src/Helpi.Application/Services"


# Ensure the directory exists
mkdir -p $folder

# Create each entity file
for entity in "${entities[@]}"; do
    filePath="$folder/${entity}Service.cs"
    if [ ! -f "$filePath" ]; then
        echo "
        using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;

        namespace $namespace;

public class ${entity}Service
{
    
}" > "$filePath"
        echo "Created: $filePath"
    else
        echo "Skipped: $filePath (Already Exists)"
    fi
done
