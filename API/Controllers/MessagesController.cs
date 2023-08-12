using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IOneDriveService _oneDriveService;
        public MessagesController(IMapper mapper, IUnitOfWork uow, IOneDriveService oneDriveService)
        {
            _uow = uow;
            _oneDriveService = oneDriveService;
            _mapper = mapper;
        }

        private string DetermineMediaType(IFormFile file)
        {
            if(FileExtensions.IsImage(file)) return "Image";

            if(FileExtensions.IsVideo(file)) return "Video";

            return "File";
        }

        [HttpPost("location/{recipientUsername}")]
        public async Task<ActionResult> CreateLocationMessage(string recipientUsername)
        {
            var username = User.GetUsername();

            if (username == recipientUsername.ToLower()) return BadRequest("You cannot send messages to yourself");

            var sender = await _uow.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await _uow.UserRepository.GetUserByUsernameAsync(recipientUsername);

            if (recipient == null || sender == null) return NotFound();

            if (recipient.IsBlocked || recipient.IsDeleted) return NotFound();

            if (await _uow.LikesRepository.GetUserLike(sender.Id, recipient.Id) == null || await _uow.LikesRepository.GetUserLike(recipient.Id, sender.Id) == null)
                return BadRequest("You cannot send messages to this user");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = sender.Latitude + "," + sender.Longitude,
                MessageType = "Location"
            };

            _uow.MessageRepository.AddMessage(message);

            if (await _uow.Complete()) return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");
        }

        [HttpPost("file")]
        [DisableRequestSizeLimit]
        //[RequestFormLimits(MultipartBodyLengthLimit = 500 * 1024 * 1024)]
        //[RequestSizeLimit(500 * 1024 * 1024)]
        public async Task<ActionResult<MessageDto>> CreateFileMessage([FromForm]CreateFileMessageDto createFileMessageDto)
        {
            var username = User.GetUsername();

            if (username == createFileMessageDto.RecipientUsername.ToLower()) return BadRequest("You cannot send messages to yourself");

            var sender = await _uow.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await _uow.UserRepository.GetUserByUsernameAsync(createFileMessageDto.RecipientUsername);

            if (recipient == null || sender == null) return NotFound();

            if (recipient.IsBlocked || recipient.IsDeleted) return NotFound();

            if (await _uow.LikesRepository.GetUserLike(sender.Id, recipient.Id) == null || await _uow.LikesRepository.GetUserLike(recipient.Id, sender.Id) == null)
                return BadRequest("You cannot send messages to this user");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName
            };

            if (createFileMessageDto.File != null)
            {
                if (!FileExtensions.IsValidFileName(createFileMessageDto.File.FileName)) return BadRequest("Invalid file name");
                var mediaType = DetermineMediaType(createFileMessageDto.File);
                if (mediaType == "File" && createFileMessageDto.File.Length > 500 * 1024 * 1024) return BadRequest("File size cannot exceed 500 MB");
                var fileUrl = await _oneDriveService.UploadToOneDriveAsync(createFileMessageDto.File);
                message.Content = fileUrl;
                message.MessageType = mediaType;
            }
            else return BadRequest("You must provide a file to send a file message");

            _uow.MessageRepository.AddMessage(message);

            if (await _uow.Complete()) return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();

            if (username == createMessageDto.RecipientUsername.ToLower()) return BadRequest("You cannot send messages to yourself");

            var sender = await _uow.UserRepository.GetUserByUsernameAsync(username);
            var recipient = await _uow.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if (recipient == null || sender == null) return NotFound();

            if (recipient.IsBlocked || recipient.IsDeleted) return NotFound();

            if (await _uow.LikesRepository.GetUserLike(sender.Id, recipient.Id) == null || await _uow.LikesRepository.GetUserLike(recipient.Id, sender.Id) == null)
                return BadRequest("You cannot send messages to this user");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content,
                MessageType = "Text"
            };

            _uow.MessageRepository.AddMessage(message);

            if (await _uow.Complete()) return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send message");
        }

        [HttpGet]
        public async Task<ActionResult<PagedList<MessageDto>>> GetMessagesForUser([FromQuery] MessageParams messageParams)
        {
            messageParams.Username = User.GetUsername();

            var messages = await _uow.MessageRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages));

            return messages;
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();

            var message = await _uow.MessageRepository.GetMessage(id);

            if (message.SenderUsername != username && message.RecipientUsername != username)
                return BadRequest("You cannot delete this message");

            if (message.SenderUsername == username) message.SenderDeleted = true;
            if (message.RecipientUsername == username) message.RecipientDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted)
            {
                _uow.MessageRepository.DeleteMessage(message);
            }

            if (await _uow.Complete()) return Ok();

            return BadRequest("Problem deleting the message");

        }
    }
}