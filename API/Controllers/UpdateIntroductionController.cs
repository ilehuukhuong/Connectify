using API.DTOs;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class UpdateIntroductionController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IContentModeratorService _contentModeratorService;
        public UpdateIntroductionController(IUnitOfWork uow, IMapper mapper, IContentModeratorService contentModeratorService)
        {
            _contentModeratorService = contentModeratorService;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUserIntroduction(IntroductionUpdateDto introductionUpdateDto)
        {
            var user = await _uow.UserRepository.GetUserByUsernameAsync(User.GetUsername());

            if (user == null) return NotFound();

            bool isInappropriate = await _contentModeratorService.IsInappropriateText(introductionUpdateDto.Introduction);
            if (isInappropriate)
            {
                return BadRequest("The introduction contains inappropriate content.");
            }

            _mapper.Map(introductionUpdateDto, user);

            if (await _uow.Complete()) return NoContent();

            return BadRequest("Failed to update user");
        }
    }
}