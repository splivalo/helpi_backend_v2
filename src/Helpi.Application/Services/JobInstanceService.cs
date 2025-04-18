
using System.Runtime.ConstrainedExecution;
using AutoMapper;
using Helpi.Application.DTOs;
using Helpi.Application.Interfaces;
using Helpi.Domain.Entities;
using Helpi.Domain.Enums;

namespace Helpi.Application.Services;

public class JobInstanceService
{
        private readonly IJobInstanceRepository _repository;
        private readonly IMapper _mapper;

        public JobInstanceService(IJobInstanceRepository repository, IMapper mapper)
        {
                _repository = repository;
                _mapper = mapper;
        }


        public async Task<List<JobInstanceDto>> GetJobInstancesByAssignmentAsync(int assignmentId)
        {

                return _mapper.Map<List<JobInstanceDto>>(await _repository.GetByAssignmentAsync(assignmentId));
        }

        public async Task<List<JobInstanceDto>> GetJobInstancesByStudentAsync(int studentId)
        {
                var jobInstances = await _repository.GetJobInstancesByStudentAsync(studentId);
                return _mapper.Map<List<JobInstanceDto>>(jobInstances);
        }

        public async Task<List<JobInstanceDto>> GetJobInstances()
        {
                var jobInstances = await _repository.GetJobInstances();
                return _mapper.Map<List<JobInstanceDto>>(jobInstances);
        }

        public async Task<List<JobInstanceDto>> GetSeniorCompletedJobInstances(int seniorId)
        {
                var jobInstances = await _repository.GetSeniorCompletedJobInstances(seniorId);
                return _mapper.Map<List<JobInstanceDto>>(jobInstances);
        }
        public async Task<List<JobInstanceDto>> GetStudentCompletedJobInstances(int studentId)
        {
                var jobInstances = await _repository.GetStudentCompletedJobInstances(studentId);
                return _mapper.Map<List<JobInstanceDto>>(jobInstances);
        }
        public async Task<List<JobInstanceDto>> GetStudentUpComingJobInstances(int studentId)
        {
                var jobInstances = await _repository.GetStudentUpComingJobInstances(studentId);
                return _mapper.Map<List<JobInstanceDto>>(jobInstances);
        }


        //     public async Task<JobInstanceDto> CreateJobInstanceAsync(JobInstanceCreateDto dto)
        //     {
        //         var instance = _mapper.Map<JobInstance>(dto);
        //         await _repository.AddAsync(instance);
        //         return _mapper.Map<JobInstanceDto>(instance);
        //     }


}