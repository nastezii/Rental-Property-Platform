using System.ComponentModel.DataAnnotations;
using Application.BookingService;
using Application.ListingService;
using Application.PropertyService;
using AutoMapper;
using Domain.Dtos.CreateNewUserListingDto;
using Domain.Dtos.General;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace WebApplication1.Controllers;

[ApiController]
[Route("api")]
public class ApiController(IPropertyService propertyService, IListingService listingService, IBookingService bookingService,
    IMapper mapper, PasswordHasher<User> hasher) : ControllerBase
{
    // 1. Комплексні сценарії створення сутностей
    [HttpPost]
    public async Task<IActionResult> CreateUserWithProperty(CreateNewUserListingDto dto)
    {
        var user = mapper.Map<User>(dto.User);
        var hash = hasher.HashPassword(user, dto.User.Password);
        user.PasswordHash = hash;
        
        var property = mapper.Map<Property>(dto.Property);
        var listing = mapper.Map<Listing>(dto.Listing);
        
        var amenities = dto.Property.Amenities?
            .Select(id => new PropertyAmenity { AmenityId = id })
            .ToList() ?? [];
        
        var addedListing = await listingService.CreateNewUserListingAsync(user, property, amenities, listing);
        var response = mapper.Map<ListingResponseDto>(addedListing);
        return Ok(response);
    }
    
    // 2. Сценарії оновлення сутностей
    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateProperty([Required] int id, PropertyRequestDto propertyDto)
    {
        var amenities = propertyDto.Amenities?
            .Select(x => new PropertyAmenity { AmenityId = x })
            .ToList() ?? [];
        
        var property = mapper.Map<Property>(propertyDto);
        var updated = await propertyService.UpdatePropertyAsync(id, property, amenities);
        var response = mapper.Map<PropertyResponseDto>(updated);
        return Ok(response);
    }

    // 3. Сценарій видалення сутностей
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProperty(int id)
    {
        var response = await propertyService.DeleteAsync(id);
        return Ok(response);
    }
    
    // 4. Прості SELECT-запити
    [HttpGet("properties")]
    public async Task<IActionResult> GetTopRateProperties()
    {
        var properties = await propertyService.GetTopRatePropertiesAsync();
        var response = mapper.Map<List<PropertyResponseDto>>(properties);
        return Ok(response);
    }
    
    [HttpGet("listings")]
    public async Task<IActionResult> GetActiveListing([Required] int page, [Required] int pageSize)
    {
        var listings = await listingService.GetActiveListingAsync(page, pageSize);
        var response = mapper.Map<List<ListingResponseDto>>(listings);
        return Ok(response);
    }
    
    // 5. Складні аналітичні запити
    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics()
    {
        var response = await bookingService.GetAnalyticsAsync();
        return Ok(response);
    }
}