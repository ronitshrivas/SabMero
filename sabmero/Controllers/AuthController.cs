using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using sabmero.DTOs.Auth;
using sabmero.Services;
using System.Security.Claims;

namespace sabmero.Controllers;

// ── ALL AUTH ENDPOINTS ────────────────────────────────────────────────────────
//
//  POST   /api/auth/register       → Register new user
//  POST   /api/auth/login          → Login with phone + password
//  POST   /api/auth/send-otp       → Send OTP to phone
//  POST   /api/auth/verify-otp     → Verify OTP, get token
//  GET    /api/auth/me             → Get my own profile (requires token)
//
// ─────────────────────────────────────────────────────────────────────────────

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    // ── POST /api/auth/register ───────────────────────────────────────────────
    // Register a new Customer, Vendor, Technician, or Rider.
    // Request Body (JSON):
    // {
    //   "fullName": "Ram Bahadur",
    //   "phone": "9812345678",
    //   "email": "ram@gmail.com",       ← optional
    //   "password": "MyPass123",
    //   "address": "Kathmandu, Nepal",
    //   "role": "Customer"              ← optional, defaults to "Customer"
    // }
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (success, message, data) = await _auth.RegisterAsync(dto);

        if (!success)
            return BadRequest(new { success = false, message });

        return Ok(new { success = true, message, data });
    }

    // ── POST /api/auth/login ──────────────────────────────────────────────────
    // Login with phone number + password.
    // Request Body (JSON):
    // {
    //   "phone": "9812345678",
    //   "password": "MyPass123"
    // }
    // Response: JWT token + user profile
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (success, message, data) = await _auth.LoginAsync(dto);

        if (!success)
            return Unauthorized(new { success = false, message });

        return Ok(new { success = true, message, data });
    }

    // ── POST /api/auth/send-otp ───────────────────────────────────────────────
    // Request a 6-digit OTP sent to the given phone number.
    // In development: the OTP is printed in server logs (check Railway logs).
    // In production: plug in your Bulk SMS API here.
    // Request Body (JSON):
    // {
    //   "phone": "9812345678"
    // }
    [HttpPost("send-otp")]
    public async Task<IActionResult> SendOtp([FromBody] SendOtpDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (success, message) = await _auth.SendOtpAsync(dto);

        if (!success)
            return BadRequest(new { success = false, message });

        return Ok(new { success = true, message });
    }

    // ── POST /api/auth/verify-otp ─────────────────────────────────────────────
    // Verify the OTP code. If correct, returns a JWT token.
    // If phone is new (not yet registered), creates a Customer account automatically.
    // Request Body (JSON):
    // {
    //   "phone": "9812345678",
    //   "code": "847291"
    // }
    [HttpPost("verify-otp")]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (success, message, data) = await _auth.VerifyOtpAsync(dto);

        if (!success)
            return BadRequest(new { success = false, message });

        return Ok(new { success = true, message, data });
    }

    // ── GET /api/auth/me ──────────────────────────────────────────────────────
    // Returns the currently logged-in user's profile.
    // Flutter must send: Authorization: Bearer <token>  in the request header.
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        // Read user info from the JWT token claims
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var phone = User.FindFirst(ClaimTypes.MobilePhone)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;
        var fullName = User.FindFirst(ClaimTypes.Name)?.Value;

        return Ok(new
        {
            success = true,
            data = new
            {
                id = userId,
                fullName,
                phone,
                role
            }
        });
    }
}