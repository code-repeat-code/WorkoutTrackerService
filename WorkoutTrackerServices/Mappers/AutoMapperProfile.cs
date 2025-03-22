using AutoMapper;
using WorkoutTrackerServices.Entities;
using WorkoutTrackerServices.Models;
using WorkoutTrackerServices.Models.ReportModels;

namespace WorkoutTrackerServices.Mappers;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Exercise, ExerciseResponseDto>()
            .ReverseMap();
        CreateMap<WorkoutSchedule, WorkoutScheduleRequestDto>().ReverseMap();
        CreateMap<WorkoutSchedule, WorkoutScheduleDetailedResponseDto>().ReverseMap();
        // Nested object mapping
        CreateMap<Workout, WorkoutResponseDto>();
        CreateMap<WorkoutExercise, WorkoutExerciseScheduleResponseDto>();
        CreateMap<Exercise, ExerciseResponseDto>();
        
        CreateMap<Workout, WorkoutReportResponseDto>()
            .ForMember(dest => dest.Exercises, opt => opt.MapFrom(src => src.WorkoutExercises));

        CreateMap<WorkoutExercise, WorkoutExerciseReportDto>()
            .ForMember(dest => dest.ExerciseName, opt => opt.MapFrom(src => src.Exercise.ExerciseName))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Exercise.Category));
    }
}