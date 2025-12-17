
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Exceptions;
using Helpi.Application.Interfaces;
using Helpi.Application.Interfaces.Services;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Helpi.Application.Services;

public class StudentContractService
{
        private readonly IStudentContractRepository _repository;
        private readonly IStudentRepository _studentRepository;
        private readonly IContractNumberService _contractNumberService;
        private readonly IGoogleDriveService _googleDriveService;
        private readonly IMapper _mapper;
        private readonly StudentStatusService _studentStatusService;
        private readonly ILogger<StudentContractService> _logger;

        public StudentContractService(
            IStudentContractRepository repository,
            IStudentRepository studentRepository,
            IContractNumberService contractNumberService,
            IGoogleDriveService googleDriveService,
           StudentStatusService studentStatusService,
            IMapper mapper,
            ILogger<StudentContractService> logger)
        {
                _repository = repository;
                _studentRepository = studentRepository;
                _contractNumberService = contractNumberService;
                _googleDriveService = googleDriveService;
                _studentStatusService = studentStatusService;
                _mapper = mapper;
                _logger = logger;
        }

        public async Task<List<StudentContractDto>> GetContractsByStudentAsync(int studentId) =>
            _mapper.Map<List<StudentContractDto>>(await _repository.GetByStudentIdAsync(studentId));

        public async Task<StudentContractDto> CreateContractAsync(StudentContractCreateDto dto)
        {
                var student = await GetAndValidateStudentAsync(dto.StudentId);

                // Generate contract number
                var contractNumber = await _contractNumberService.GetNextContractNumberAsync();

                // Build file metadata
                var (folderName, fileName) = BuildFileMetadata(student, contractNumber);

                // Process and upload file
                var contractFile = await ReadFileContent(dto.ContractFile[0]);
                var cloudPath = await _googleDriveService.UploadContractAsync(
                    folderName,
                    contractFile,
                    fileName
                );

                // var today = DateOnly.FromDateTime(DateTime.UtcNow);
                // var isValid = dto.ExpirationDate > today;
                // var status = isValid ? ContractStatus.valid : ContractStatus.expired;

                var contract = new StudentContract
                {
                        StudentId = dto.StudentId,
                        CloudPath = cloudPath,
                        EffectiveDate = dto.EffectiveDate,
                        ExpirationDate = dto.ExpirationDate,
                        ContractNumber = contractNumber.ToString()
                };

                await _repository.AddAsync(contract);
                _logger.LogInformation("Created contract {ContractNumber} for student {StudentId}", contractNumber, dto.StudentId);

                student.Contracts.Add(contract);

                await _studentStatusService.ProcessStudentStatus(student);

                return _mapper.Map<StudentContractDto>(contract);
        }

        public async Task<StudentContractDto> UpdateContractAsync(int contractId, StudentContractUpdateDto dto)
        {
                var contract = await GetAndValidateContractAsync(contractId);

                var students = await _studentRepository.LoadStudentsWithIncludes(contract.StudentId, new StudentIncludeOptions
                {
                        ContactInfo = true,
                        Contracts = true,
                });

                var student = students.First();

                // If there's a new contract file, upload it
                if (dto.NewContractFile != null)
                {

                        var contractNumber = int.Parse(contract.ContractNumber);

                        // Build file metadata
                        var (folderName, fileName) = BuildFileMetadata(student, contractNumber);


                        // Delete old file and upload new one
                        var cloudPath = await ReplaceContractFileAsync(contract.CloudPath, dto.NewContractFile[0], folderName, fileName);

                        // Update cloud path in contract
                        contract.CloudPath = cloudPath;
                }



                if (dto.EffectiveDate.HasValue)
                {
                        contract.EffectiveDate = dto.EffectiveDate.Value;
                }

                if (dto.ExpirationDate.HasValue)
                {
                        contract.ExpirationDate = dto.ExpirationDate.Value;
                }






                await _repository.UpdateAsync(contract);
                _logger.LogInformation("Updated contract {ContractId} for student {StudentId}", contractId, contract.StudentId);



                await _studentStatusService.ProcessStudentStatus(student);

                return _mapper.Map<StudentContractDto>(contract);
        }

        public async Task DeleteContractAsync(int id)
        {
                var contract = await GetAndValidateContractAsync(id);

                var studentId = contract.StudentId;

                // Delete the file from Google Drive
                try
                {
                        await _googleDriveService.DeleteFileAsync(contract.CloudPath);
                }
                catch (Exception ex)
                {
                        // Log but continue - we still want to delete the database record
                        _logger.LogWarning(ex, "Failed to delete contract file from cloud storage: {CloudPath}", contract.CloudPath);
                }



                // Delete the contract record
                contract.DeletedOn = DateTime.Now;
                await _repository.UpdateAsync(contract);
                _logger.LogInformation("Deleted contract {ContractId}", id);

                await ProcessStudentStatus(studentId);
        }

        private async Task ProcessStudentStatus(int studentId)
        {
                var students = await _studentRepository.LoadStudentsWithIncludes(studentId, new StudentIncludeOptions
                {
                        Contracts = true,
                        ContactInfo = true
                },
                asNoTracking: false);

                var student = students.First();

                await _studentStatusService.ProcessStudentStatus(student);
        }

        public async Task<StudentContractDto> GetContractByIdAsync(int id)
        {
                var contract = await GetAndValidateContractAsync(id);
                return _mapper.Map<StudentContractDto>(contract);
        }

        public async Task<IEnumerable<StudentContractDto>> GetContractsByStudentIdAsync(int studentId)
        {
                var contracts = await _repository.GetByStudentIdAsync(studentId);
                return _mapper.Map<IEnumerable<StudentContractDto>>(contracts);
        }

        // public async Task<byte[]> DownloadContractAsync(int id)
        // {
        // var contract = await GetAndValidateContractAsync(id);
        // return await _googleDriveService.DownloadFileAsync(contract.CloudPath);
        // }



        private async Task<Student> GetAndValidateStudentAsync(int studentId)
        {
                var student = await _studentRepository.GetByIdAsync(studentId);

                if (student == null)
                {
                        throw new NotFoundException("Student", studentId);
                }

                return student;
        }

        private async Task<StudentContract> GetAndValidateContractAsync(int contractId)
        {
                var contract = await _repository.GetByIdAsync(contractId);

                if (contract == null)
                {
                        throw new NotFoundException("Contract", contractId);
                }

                return contract;
        }

        private (string folderName, string fileName) BuildFileMetadata(Student student, int contractNumber)
        {
                var currentYear = DateTime.UtcNow.Year;
                var folderName = $"{student.Contact.FullName}-{student.StudentNumber}";
                var fileName = $"{contractNumber}-{student.StudentNumber}-{currentYear}.pdf";

                return (folderName, fileName);
        }

        // private async Task<string> GetContractCloudPathAsync(string folderName, string fileName)
        // {
        //         _googleDriveService.
        //         return $"{folderName}/{fileName}"; // todo
        // }

        private async Task<string> ReplaceContractFileAsync(string oldCloudPath, IFormFile newFile, string folderName, string fileName)
        {
                try
                {
                        // Delete the old contract file
                        await _googleDriveService.DeleteFileAsync(oldCloudPath);
                }
                catch (Exception ex)
                {
                        _logger.LogWarning(ex, "Failed to delete old contract file: {CloudPath}", oldCloudPath);

                }

                // Upload the new file
                var contractFile = await ReadFileContent(newFile);
                return await _googleDriveService.UploadContractAsync(
                      folderName,
                      contractFile,
                      fileName
                  );
        }


        private async Task<byte[]> ReadFileContent(IFormFile file)
        {
                await using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
        }

        public async Task<List<CompletedStudentContractDto>> GetStudentCompletedContractsAsync(int studentId)
        {
                var contracts = await _repository.GetCompletedContractsForStudentAsync(studentId);
                return _mapper.Map<List<CompletedStudentContractDto>>(contracts);
        }
}