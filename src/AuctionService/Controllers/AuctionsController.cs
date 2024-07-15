

using AucionService.Entities;
using AuctionService.Data;
using AuctionService.DTOs;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NRedisStack;
using NRedisStack.RedisStackCommands;
using StackExchange.Redis;
using AucionService.Redlock;
using NetTopologySuite.Utilities;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections.Generic;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    //private readonly IAuctionRepository _repo;
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;
    private IDatabase redisDB;
    private const string resourceName = "auctionData";
    private readonly Redlock dlm;

    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;

        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
        redisDB = redis.GetDatabase();

        dlm = new Redlock(ConnectionMultiplexer.Connect("127.0.0.1:6379"));
    }

    [HttpGet]
    public async Task<ActionResult<List<AuctionDto>>> GetAllAuctions()
    {
        Lock lockObject;
        var locked = dlm.Lock(resourceName, new TimeSpan(0, 0, 5), out lockObject);
        if (locked)
        {
            var aunctions = await _context.Auctions.Include(x => x.Item)
                .OrderBy(x => x.Item.Make).ToListAsync();
            return _mapper.Map<List<AuctionDto>>(aunctions);
        } else
        {
            List<AuctionDto> empty = []; 
            return _mapper.Map<List<AuctionDto>>(empty);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
    {
        var aunction = await _context.Auctions.Include(x => x.Item).FirstOrDefaultAsync(x => x.Id == id);
        return aunction == null ? NotFound() : _mapper.Map<AuctionDto>(aunction);
    }

    //[Authorize]
    //[HttpPost]
    //public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto auctionDto)
    //{
    //    var auction = _mapper.Map<Auction>(auctionDto);

    //    auction.Seller = User.Identity.Name;

    //    _repo.AddAuction(auction);

    //    var newAuction = _mapper.Map<AuctionDto>(auction);

    //    await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

    //    var result = await _repo.SaveChangesAsync();

    //    if (!result) return BadRequest("Could not save changes to the DB");

    //    return CreatedAtAction(nameof(GetAuctionById),
    //        new { auction.Id }, newAuction);
    //}

}
