namespace Domain.Dtos.CreateNewUserListingDto;

public class CreateNewUserListingDto
{
    public UserRequestDto User { get; set; } = null!;
    public PropertyRequestDto Property { get; set; } = null!;
    public ListingRequestDto Listing { get; set; } = null!;
}