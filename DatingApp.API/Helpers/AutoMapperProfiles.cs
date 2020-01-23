using System;
using System.Linq;
using AutoMapper;
using DatingApp.API.Dtos;
using DatingApp.API.Models;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        // Automapper uses profiles to understand the source and destinations of what it's mapping.
        public AutoMapperProfiles()
        {
            CreateMap<User, UserForListDto>()
                // ForMember allows us to customize the configuration for an individual memeber of the class.
                // Frist expression is the destination, we'll be setting the PhotoUrl
                .ForMember(dest => dest.PhotoUrl, opt => {
                    // this is the expression on what we want to map to the PhotoUrl, we'll use LINQ to grab the first photo with 
                    opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url);
                })
                .ForMember(dest => dest.Age, opt => {
                    opt.MapFrom(u => u.DateOfBirth.CalculateAge());
                });
            CreateMap<User, UserForDetailsDto>()
                .ForMember(dest => dest.PhotoUrl, opt => {
                    opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url);
                })
                .ForMember(dest => dest.Age, opt => {
                    opt.MapFrom(u => u.DateOfBirth.CalculateAge());
                });
            CreateMap<Photo, PhotosForDetailsDto>();
            CreateMap<UserForUpdateDto, User>();
            CreateMap<PhotoForCreationDto, Photo>();
            CreateMap<Photo, PhotoForReturnDto>();
        }
    }
}