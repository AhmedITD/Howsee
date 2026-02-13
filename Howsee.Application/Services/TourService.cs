using Howsee.Application.Common;
using Howsee.Application.DTOs.requests.Tour;
using Howsee.Application.DTOs.responses.Common;
using Howsee.Application.DTOs.responses.Tour;
using Howsee.Application.Interfaces;
using Howsee.Application.Interfaces.Auth;
using Howsee.Application.Interfaces.Tours;
using Howsee.Domain.Entities;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace Howsee.Application.Services;

public class TourService(
    IHowseeDbContext dbContext,
    IPasswordService passwordService,
    IMatterportApiClient matterportApiClient,
    Microsoft.Extensions.Configuration.IConfiguration configuration) : ITourService
{
    public async Task<ApiResponse<TourResponse>> CreateTour(CreateTourRequest request, int ownerId, CancellationToken cancellationToken = default)
    {
        if (matterportApiClient.IsConfigured)
        {
            try
            {
                var exists = await matterportApiClient.ValidateModelExistsAsync(request.MatterportModelId, cancellationToken);
                if (!exists)
                    return ApiResponse<TourResponse>.ErrorResponse("Matterport model not found or not accessible.", code: ErrorCodes.InvalidMatterportModel);
            }
            catch (MatterportApiException ex)
            {
                return ApiResponse<TourResponse>.ErrorResponse(ex.Message, code: ErrorCodes.InvalidMatterportModel);
            }
        }

        var tour = new Tour
        {
            OwnerId = ownerId,
            Title = request.Title,
            MatterportModelId = request.MatterportModelId,
            StartSweepId = request.StartSweepId,
            PasswordHash = string.IsNullOrEmpty(request.Password) ? null : passwordService.HashPassword(request.Password),
            ExpiresAt = request.ExpiresAt,
            IsActive = true,
            ShareToken = Guid.NewGuid().ToString("N")
        };
        dbContext.Tours.Add(tour);
        await dbContext.SaveChangesAsync(cancellationToken);

        var response = tour.Adapt<TourResponse>();
        response.HasPassword = !string.IsNullOrEmpty(tour.PasswordHash);
        return ApiResponse<TourResponse>.SuccessResponse(response);
    }

    public async Task<ApiResponse<TourResponse>> UpdateTour(int tourId, UpdateTourRequest request, int ownerId, CancellationToken cancellationToken = default)
    {
        var tour = await dbContext.Tours.FirstOrDefaultAsync(t => t.Id == tourId && t.OwnerId == ownerId, cancellationToken);
        if (tour == null)
            return ApiResponse<TourResponse>.ErrorResponse("Tour not found.", code: ErrorCodes.TourNotFound);

        if (request.MatterportModelId != null && matterportApiClient.IsConfigured)
        {
            try
            {
                var exists = await matterportApiClient.ValidateModelExistsAsync(request.MatterportModelId, cancellationToken);
                if (!exists)
                    return ApiResponse<TourResponse>.ErrorResponse("Matterport model not found or not accessible.", code: ErrorCodes.InvalidMatterportModel);
            }
            catch (MatterportApiException ex)
            {
                return ApiResponse<TourResponse>.ErrorResponse(ex.Message, code: ErrorCodes.InvalidMatterportModel);
            }
        }

        if (request.Title != null) tour.Title = request.Title;
        if (request.MatterportModelId != null) tour.MatterportModelId = request.MatterportModelId;
        if (request.StartSweepId != null) tour.StartSweepId = request.StartSweepId;
        if (request.Password != null) tour.PasswordHash = string.IsNullOrEmpty(request.Password) ? null : passwordService.HashPassword(request.Password);
        if (request.ExpiresAt.HasValue) tour.ExpiresAt = request.ExpiresAt;
        if (request.IsActive.HasValue) tour.IsActive = request.IsActive.Value;

        await dbContext.SaveChangesAsync(cancellationToken);

        var response = tour.Adapt<TourResponse>();
        response.HasPassword = !string.IsNullOrEmpty(tour.PasswordHash);
        return ApiResponse<TourResponse>.SuccessResponse(response);
    }

    public async Task<ApiResponse<TourResponse>> GetTour(int tourId, int ownerId, CancellationToken cancellationToken = default)
    {
        var tour = await dbContext.Tours.FirstOrDefaultAsync(t => t.Id == tourId && t.OwnerId == ownerId, cancellationToken);
        if (tour == null)
            return ApiResponse<TourResponse>.ErrorResponse("Tour not found.", code: ErrorCodes.TourNotFound);

        var response = tour.Adapt<TourResponse>();
        response.HasPassword = !string.IsNullOrEmpty(tour.PasswordHash);
        return ApiResponse<TourResponse>.SuccessResponse(response);
    }

    public async Task<ApiResponse<List<TourResponse>>> ListTours(int ownerId, CancellationToken cancellationToken = default)
    {
        var tours = await dbContext.Tours
            .Where(t => t.OwnerId == ownerId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

        var list = tours.Select(t =>
        {
            var r = t.Adapt<TourResponse>();
            r.HasPassword = !string.IsNullOrEmpty(t.PasswordHash);
            return r;
        }).ToList();

        return ApiResponse<List<TourResponse>>.SuccessResponse(list);
    }

    public async Task<ApiResponse<bool>> DeleteTour(int tourId, int ownerId, CancellationToken cancellationToken = default)
    {
        var tour = await dbContext.Tours.FirstOrDefaultAsync(t => t.Id == tourId && t.OwnerId == ownerId, cancellationToken);
        if (tour == null)
            return ApiResponse<bool>.ErrorResponse("Tour not found.", code: ErrorCodes.TourNotFound);

        dbContext.Tours.Remove(tour);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true);
    }

    public async Task<ApiResponse<TourViewConfigResponse>> GetViewConfigAsync(string token, string? password, CancellationToken cancellationToken = default)
    {
        var tour = await dbContext.Tours.AsNoTracking().FirstOrDefaultAsync(t => t.ShareToken == token, cancellationToken);
        if (tour == null)
            return ApiResponse<TourViewConfigResponse>.ErrorResponse("Invalid or expired link.", code: ErrorCodes.InvalidTourToken);
        if (!tour.IsActive)
            return ApiResponse<TourViewConfigResponse>.ErrorResponse("This tour is not available.", code: ErrorCodes.TourAccessDenied);

        if (tour.ExpiresAt.HasValue && tour.ExpiresAt.Value < DateTime.UtcNow)
            return ApiResponse<TourViewConfigResponse>.ErrorResponse("This tour link has expired.", code: ErrorCodes.TourExpired);

        if (!string.IsNullOrEmpty(tour.PasswordHash))
        {
            if (string.IsNullOrEmpty(password))
                return ApiResponse<TourViewConfigResponse>.ErrorResponse("Password required.", code: ErrorCodes.TourAccessDenied);
            if (!passwordService.VerifyPassword(password, tour.PasswordHash))
                return ApiResponse<TourViewConfigResponse>.ErrorResponse("Invalid password.", code: ErrorCodes.TourAccessDenied);
        }

        var applicationKey = configuration["Matterport:ApplicationKey"] ?? "";
        return ApiResponse<TourViewConfigResponse>.SuccessResponse(new TourViewConfigResponse
        {
            MatterportModelId = tour.MatterportModelId,
            ApplicationKey = applicationKey,
            StartSweepId = tour.StartSweepId,
            ExpiresAt = tour.ExpiresAt
        });
    }
}
