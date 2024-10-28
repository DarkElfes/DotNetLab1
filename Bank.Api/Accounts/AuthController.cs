using AutoMapper;
using Bank.Api.Cards;
using Bank.Shared.Accounts;
using FluentResults.Extensions.AspNetCore;
using Microsoft.AspNetCore.Mvc;
using System.Security.Principal;

namespace Bank.Api.Accounts;

[Route("api/[controller]")]
[ApiController]
public class AuthController(
    AccountRepository accountRepo,
    CardRepository cardRepo,
    JwtTokenGenerator jwtTokenGenerator,
    IMapper mapper
    ) : Controller
{
    private readonly AccountRepository _accountRepo = accountRepo;
    private readonly JwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
    private readonly CardRepository _cardRepo = cardRepo;
    private readonly IMapper _mapper = mapper;


    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        var result = await _accountRepo.GetByEmailAsync(loginRequest.Email);

        if (result.IsFailed)
            return result.ToActionResult();

        var account = result.Value;
        if (account is null || !account.Password.Equals(loginRequest.Password))
            return BadRequest($"Incorrect email or password");

        return _jwtTokenGenerator.GenerateToken(account)
           .Map(token => new AuthResponse(_mapper.Map<AccountDto>(account), token))
           .ToActionResult();
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest registerRequest)
    {
        var result = await _accountRepo.GetByEmailAsync(registerRequest.Email);

        if (result.IsFailed)
            return result.ToActionResult();

        if (result.Value is not null)
            return BadRequest($"Account with {registerRequest.Email} email already exist");

        Account newAccount = new()
        {
            Username = registerRequest.Username,
            Email = registerRequest.Email,
            Password = registerRequest.Password,
        };
        newAccount.Card = new()
        {
            Id = await _cardRepo.GetLastId() + 1,
            Owner = newAccount,
        };


        await _accountRepo.AddAsync(newAccount);

        return _jwtTokenGenerator.GenerateToken(newAccount)
            .Map(token => new AuthResponse(_mapper.Map<AccountDto>(newAccount), token))
            .ToActionResult();
    }

}
