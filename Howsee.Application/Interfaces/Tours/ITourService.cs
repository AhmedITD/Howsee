using Howsee.Application.DTOs.requests.Tour;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Tour;

namespace Howsee.Application.Interfaces.Tours;

public interface ITourService
{
    Task<ApiResponse<TourResponse>> CreateTour(CreateTourRequest request, int ownerId, CancellationToken cancellationToken = default);
    Task<ApiResponse<TourResponse>> UpdateTour(int tourId, UpdateTourRequest request, int ownerId, CancellationToken cancellationToken = default);
    Task<ApiResponse<TourResponse>> GetTour(int tourId, int ownerId, CancellationToken cancellationToken = default);
    Task<ApiResponse<List<TourResponse>>> ListTours(int ownerId, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> DeleteTour(int tourId, int ownerId, CancellationToken cancellationToken = default);
    Task<ApiResponse<TourViewConfigResponse>> GetViewConfigAsync(string token, string? password, CancellationToken cancellationToken = default);
}
