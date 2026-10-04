# 🔐 Password Policy & Account Security

## Password Requirements

VNVTStore enforces strong password policies to protect user accounts from unauthorized access.

### ✅ **Password Policy Rules**

All passwords must meet the following criteria:

1. **Minimum Length:** 8 characters
2. **Uppercase Letter:** At least 1 (A-Z)
3. **Lowercase Letter:** At least 1 (a-z)
4. **Number:** At least 1 (0-9)
5. **Special Character:** At least 1 from `@$!%*?&`

### **Validation Pattern:**
```regex
^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$
```

**Breakdown:**
- `(?=.*[a-z])` - Contains at least one lowercase letter
- `(?=.*[A-Z])` - Contains at least one uppercase letter
- `(?=.*\d)` - Contains at least one digit
- `(?=.*[@$!%*?&])` - Contains at least one special character
- `[A-Za-z\d@$!%*?&]{8,}` - Only allows specified characters, minimum 8 length

---

## ✅ **Implementation**

### **Backend Validation (FluentValidation)**

Password validation is enforced in:
- **Registration:** `RegisterCommandValidator`
- **Password Reset:** `ResetPasswordCommandValidator`

**Location:** `VNVTStore.Application/Auth/Validators/AuthValidators.cs`

```csharp
RuleFor(x => x.password)
    .NotEmpty().WithMessage(_ => MessageConstants.Get(MessageConstants.PasswordTooWeak))
    .MinimumLength(8).WithMessage(_ => MessageConstants.Get(MessageConstants.PasswordTooWeak))
    .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$")
    .WithMessage(_ => MessageConstants.Get(MessageConstants.PasswordTooWeak));
```

**Error Message (Vietnamese):**
> Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường, số và ký tự đặc biệt

**Error Message (English):**
> Password must be at least 8 characters long, including uppercase, lowercase, numbers, and special characters

---

## 🧪 **Testing Password Policy**

### **Valid Passwords:**
✅ `Password123!`  
✅ `Secure@Pass1`  
✅ `MyP@ssw0rd`  
✅ `Admin!234`  
✅ `Test$1234`  

### **Invalid Passwords:**

| Password | Reason | Fix |
|----------|--------|-----|
| `password` | No uppercase, no number, no special char | `Password123!` |
| `PASSWORD123!` | No lowercase | `Password123!` |
| `Password!` | No number | `Password123!` |
| `Password123` | No special character | `Password123!` |
| `Pass1!` | Too short (< 8 chars) | `Password1!` |
| `Password 123!` | Space not allowed | `Password123!` |
| `Пароль123!` | Non-ASCII characters | `Password123!` |

---

## 🔒 **Additional Security Measures**

### **1. Password Hashing**
- **Algorithm:** BCrypt
- **Salt Rounds:** 10 (configurable)
- **Service:** `IPasswordHasher` in `VNVTStore.Infrastructure`

```csharp
// Hash password on registration
var hashedPassword = _passwordHasher.Hash(password);

// Verify password on login
var isValid = _passwordHasher.Verify(password, hashedPassword);
```

### **2. Account Lockout (Task #8 - To Be Implemented)**
- Lock account after 5 failed login attempts
- Lockout duration: 15 minutes
- Exponential backoff for repeated failures

### **3. Password Reset Security**
- Reset tokens expire after 1 hour
- Tokens are single-use only
- Email verification required
- Old password becomes invalid after reset

### **4. Session Management (Task #12 - To Be Implemented)**
- JWT token expiration: 1 hour (configurable)
- Refresh token rotation
- Logout from all devices
- Force re-authentication for sensitive operations

---

## 📋 **Best Practices**

### **For Users:**
1. ✅ Use unique passwords for each service
2. ✅ Use a password manager (1Password, Bitwarden, LastPass)
3. ✅ Enable 2FA when available (Task #11 - To Be Implemented)
4. ✅ Change password every 90 days
5. ❌ Never share passwords
6. ❌ Don't reuse old passwords
7. ❌ Don't write passwords down

### **For Administrators:**
1. ✅ Enforce password expiration (optional, via custom policy)
2. ✅ Monitor failed login attempts
3. ✅ Audit password changes
4. ✅ Implement rate limiting on auth endpoints (✅ Already implemented)
5. ✅ Use HTTPS only (no plain HTTP)
6. ✅ Store JWT secrets in secure vaults (✅ Already implemented)

---

## 🔄 **Password Change Flow**

### **1. Change Password (Authenticated User)**
```
POST /api/v1/users/change-password
Authorization: Bearer <token>

{
  "currentPassword": "OldPass123!",
  "newPassword": "NewPass456@"
}
```

**Validation:**
1. Verify current password
2. Validate new password against policy
3. Hash new password
4. Update database
5. Invalidate existing sessions (optional)

### **2. Forgot Password Flow**
```
Step 1: Request Reset
POST /api/v1/auth/forgot-password
{ "email": "user@example.com" }

Step 2: Receive Email with Token
User receives email: "Reset your password: <link with token>"

Step 3: Reset Password
POST /api/v1/auth/reset-password
{
  "email": "user@example.com",
  "token": "abc123...",
  "newPassword": "NewPass456@"
}
```

**Security:**
- Token expires after 1 hour
- Token is single-use
- Email must match token
- New password validated against policy

---

## 🛡️ **Frontend Integration**

### **Password Strength Indicator (Recommended)**

```typescript
// utils/passwordStrength.ts
export const validatePassword = (password: string): {
  isValid: boolean;
  errors: string[];
  strength: 'weak' | 'medium' | 'strong';
} => {
  const errors: string[] = [];
  
  if (password.length < 8) {
    errors.push('At least 8 characters');
  }
  if (!/[a-z]/.test(password)) {
    errors.push('One lowercase letter');
  }
  if (!/[A-Z]/.test(password)) {
    errors.push('One uppercase letter');
  }
  if (!/\d/.test(password)) {
    errors.push('One number');
  }
  if (!/[@$!%*?&]/.test(password)) {
    errors.push('One special character (@$!%*?&)');
  }
  
  const strength = errors.length === 0 ? 'strong' : errors.length <= 2 ? 'medium' : 'weak';
  
  return {
    isValid: errors.length === 0,
    errors,
    strength
  };
};
```

**Usage in Register Form:**
```tsx
const PasswordInput = () => {
  const [password, setPassword] = useState('');
  const validation = validatePassword(password);
  
  return (
    <div>
      <input 
        type="password" 
        value={password}
        onChange={(e) => setPassword(e.target.value)}
      />
      
      {/* Strength Indicator */}
      <div className={`strength-${validation.strength}`}>
        {validation.strength === 'strong' && '✅ Strong'}
        {validation.strength === 'medium' && '⚠️ Medium'}
        {validation.strength === 'weak' && '❌ Weak'}
      </div>
      
      {/* Requirements Checklist */}
      <ul>
        <li className={password.length >= 8 ? 'valid' : 'invalid'}>
          8+ characters
        </li>
        <li className={/[a-z]/.test(password) ? 'valid' : 'invalid'}>
          Lowercase letter
        </li>
        <li className={/[A-Z]/.test(password) ? 'valid' : 'invalid'}>
          Uppercase letter
        </li>
        <li className={/\d/.test(password) ? 'valid' : 'invalid'}>
          Number
        </li>
        <li className={/[@$!%*?&]/.test(password) ? 'valid' : 'invalid'}>
          Special character (@$!%*?&)
        </li>
      </ul>
    </div>
  );
};
```

---

## 🔗 **References**

- [OWASP Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)
- [NIST Password Guidelines](https://pages.nist.gov/800-63-3/sp800-63b.html)
- [BCrypt Documentation](https://github.com/BcryptNet/bcrypt.net)
