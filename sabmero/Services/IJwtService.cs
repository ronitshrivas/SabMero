using sabmero.Models;

namespace sabmero.Services;

// Interface = a contract.
// It says "any class that implements me MUST have these methods".
// This makes the code easier to test and swap out later.
public interface IJwtService
{
    // Takes a User object and returns a JWT token string
    string GenerateToken(User user);
}