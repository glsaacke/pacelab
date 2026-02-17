# JWT Authentication Explained

## What is JWT?

JWT (JSON Web Token) is a compact, URL-safe means of representing claims to be transferred between two parties. In PaceLab, we use JWT for stateless authentication between the frontend and backend API.

## How JWT Works in PaceLab

### 1. User Registration/Login
- User provides email and password
- Backend validates credentials
- Backend generates a JWT token
- Token is sent back to the frontend

### 2. Token Structure
A JWT consists of three parts separated by dots (.):
```
header.payload.signature
```

**Example:**
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1laWRlbnRpZmllciI6IjEyMyIsImVtYWlsIjoidXNlckBleGFtcGxlLmNvbSIsImV4cCI6MTY3MDAwMDAwMH0.signature
```

#### Header
Contains the algorithm used to sign the token (HS256 in our case):
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

#### Payload (Claims)
Contains the user data and token metadata:
```json
{
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": "123",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress": "user@example.com",
  "jti": "unique-token-id",
  "exp": 1670000000,
  "iss": "issuer-name",
  "aud": "audience-name"
}
```

**Claims we use:**
- `NameIdentifier`: User ID (used for authorization)
- `Email`: User's email address
- `jti`: Unique token ID (prevents replay attacks)
- `exp`: Expiration time (Unix timestamp)
- `iss`: Issuer (who created the token - our API)
- `aud`: Audience (who the token is for - our frontend)

#### Signature
Created by taking the encoded header and payload, and signing them with the secret key using HMAC-SHA256:
```
HMACSHA256(
  base64UrlEncode(header) + "." + base64UrlEncode(payload),
  secret
)
```

### 3. Frontend Stores Token
- Token is typically stored in browser (localStorage, sessionStorage, or httpOnly cookie)
- Frontend includes token in `Authorization` header for all protected API requests:
  ```
  Authorization: Bearer <token>
  ```

### 4. Backend Validates Token
For each protected endpoint:
1. Extract token from `Authorization` header
2. Verify signature using the secret key
3. Check expiration time
4. Validate issuer and audience
5. Extract user claims (user ID, email)
6. Allow or deny the request

### 5. Token Expiration
- Tokens expire after 60 minutes (configurable in `.env`)
- When expired, user must log in again to get a new token
- Future: Implement refresh tokens for better UX

## Security Considerations

### Why JWT is Secure
1. **Signed, not encrypted**: The payload is Base64-encoded (readable), but the signature prevents tampering
2. **Stateless**: Server doesn't need to store sessions - it just validates the signature
3. **Tamper-proof**: Any change to the payload invalidates the signature

### Best Practices We Follow
1. **Strong Secret Key**: Minimum 256 bits (32 characters)
2. **HTTPS Only**: Tokens should only be transmitted over HTTPS in production
3. **Short Expiration**: 60 minutes reduces risk if token is stolen
4. **Never expose secret**: Secret key stored in environment variables, never committed to Git
5. **Validate everything**: Check signature, expiration, issuer, and audience on every request

### What JWT Does NOT Protect Against
- **XSS (Cross-Site Scripting)**: If attacker can run JavaScript in your app, they can steal tokens
  - Mitigation: Sanitize all user input, use Content Security Policy
- **Token theft**: If someone steals the token, they can impersonate the user until it expires
  - Mitigation: Short expiration times, refresh tokens, secure storage

## Configuration in PaceLab

### Environment Variables (.env)

### Production Recommendations
1. **Generate a strong secret**: Use a cryptographically secure random string (256+ bits)
   ```bash
   # Generate a secure secret (PowerShell)
   [Convert]::ToBase64String((1..64 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
   ```
2. **Store secret in hosting platform**: Use Azure Key Vault, AWS Secrets Manager, or environment variables
3. **Use HTTPS**: Never send tokens over unencrypted connections
4. **Rotate secrets periodically**: Change the secret key every 90 days (requires all users to re-login)

## Code Flow in PaceLab

### 1. Register/Login (AuthService.cs)
```csharp
public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
{
    // 1. Hash password with BCrypt
    var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, 12);
    
    // 2. Save user to database
    var user = new User { Email = request.Email, PasswordHash = passwordHash };
    _context.Users.Add(user);
    await _context.SaveChangesAsync();
    
    // 3. Generate JWT token
    var token = GenerateJwtToken(user);
    
    // 4. Return user data + token
    return new AuthResponse { User = MapToDto(user), Token = token };
}
```

### 2. Generate Token (AuthService.cs)
```csharp
private string GenerateJwtToken(User user)
{
    var claims = new[]
    {
        new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };
    
    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
    var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
    
    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddMinutes(60),
        signingCredentials: credentials
    );
    
    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

### 3. Validate Token (Program.cs)
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
        };
    });
```

### 4. Protect Endpoints (Controllers)
```csharp
[Authorize]  // Requires valid JWT token
[HttpGet("protected")]
public IActionResult GetProtectedData()
{
    // Extract user ID from token
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    
    // Use userId to fetch user-specific data
    return Ok(new { message = $"Hello user {userId}" });
}
```

## Testing JWT

### 1. Register a User
```bash
POST /api/auth/register
Content-Type: application/json

{
  "email": "test@example.com",
  "password": "securePassword123"
}
```

**Response:**
```json
{
  "user": {
    "userId": 1,
    "email": "test@example.com",
    "createdAt": "2026-02-16T12:00:00Z",
    "lastLoggedIn": "2026-02-16T12:00:00Z"
  },
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### 2. Use Token for Protected Requests
```bash
GET /api/activities
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 3. Decode Token (for debugging)
Visit [jwt.io](https://jwt.io) and paste your token to see the decoded payload.

## Common Issues & Solutions

### Issue: "Unauthorized" error
**Cause**: Token is missing, invalid, or expired  
**Solution**: Check that token is included in `Authorization: Bearer <token>` header

### Issue: Token expires too quickly
**Cause**: `Jwt__ExpirationMinutes` is too low  
**Solution**: Increase to 60-120 minutes (balance security vs UX)

### Issue: "Invalid signature" error
**Cause**: Secret key mismatch between token generation and validation  
**Solution**: Verify `Jwt__Secret` is identical in all environments

### Issue: Tokens still valid after changing secret
**Cause**: Old tokens were signed with old secret  
**Solution**: All users must log in again to get new tokens

## Future Enhancements

1. **Refresh Tokens**: Allow users to get new access tokens without re-logging in
2. **Token Revocation**: Implement a blacklist for compromised tokens
3. **Role-Based Access**: Add roles (admin, user) to claims for authorization
4. **Remember Me**: Longer-lived refresh tokens for "remember me" functionality

## Additional Resources

- [JWT.io](https://jwt.io) - Decode and verify JWTs
- [RFC 7519](https://tools.ietf.org/html/rfc7519) - JWT specification
- [Microsoft JWT docs](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/jwt-authn)
- [OWASP JWT Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/JSON_Web_Token_for_Java_Cheat_Sheet.html)
