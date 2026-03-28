using e360_clone.BusinessObjects.DTOs;

namespace e360_clone.Repositories
{
    public interface IExamRoomAllocationRepository
    {
        Task<ExamAllocationSummaryDto?> GetSummaryAsync(int examId);
        Task<ExamAllocationSummaryDto?> AutoAllocateAsync(int examId, List<int> roomIds);
        Task<ExamAllocationSummaryDto?> SaveAllocationsAsync(int examId, List<ExamAllocationItemDto> allocations);
    }
}
